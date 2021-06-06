using System.Collections.Generic;
using System.Linq;
using Vostok.Logging.Abstractions;

namespace SpaceHosting.Index
{
    internal sealed class IndexStore<TId, TData, TVector> : IIndexStore<TId, TData, TVector>
        where TId : notnull
        where TVector : IVector
    {
        private readonly ILog log;
        private readonly IIndex<TVector> index;
        private readonly IIndexIdMapping<TId> idMapping;
        private readonly IIndexDataStorage<TId, TData> storage;
        private readonly IEqualityComparer<TId> idComparer;

        public IndexStore(
            ILog log,
            IIndex<TVector> index,
            IIndexIdMapping<TId> idMapping,
            IIndexDataStorage<TId, TData> storage,
            IEqualityComparer<TId> idComparer)
        {
            this.log = log;
            this.index = index;
            this.idMapping = idMapping;
            this.storage = storage;
            this.idComparer = idComparer;
        }

        public long Count => index.VectorCount;

        public void AddBatch(IndexDataPoint<TId, TData, TVector>[] dataPoints)
        {
            log.Debug(
                $"Adding batch of {dataPoints.Length}, before: " +
                $"index size = {index.VectorCount}, " +
                $"id map size = {idMapping.Count}, " +
                $"data size = {storage.Count}");

            var dataPointsWithIndexIds = dataPoints
                .GroupBy(x => x.Id, idComparer)
                .Select(x => x.Last())
                .Select(
                    x => new
                    {
                        IndexId = idMapping.FindIndexIdById(x.Id),
                        DataPoint = x
                    })
                .ToList();

            var dataPointsWithIndexIdsToRemove = dataPointsWithIndexIds
                .Where(x => x.IndexId.HasValue && x.DataPoint.IsDeleted)
                .Select(x => new {x.DataPoint, IndexId = x.IndexId!.Value})
                .ToArray();
            var dataPointsWithIndexIdsToUpdate = dataPointsWithIndexIds
                .Where(x => x.IndexId.HasValue && !x.DataPoint.IsDeleted)
                .Select(x => new {x.DataPoint, IndexId = x.IndexId!.Value})
                .ToArray();
            var dataPointsWithIndexIdsToSkip = dataPointsWithIndexIds
                .Where(x => !x.IndexId.HasValue && x.DataPoint.IsDeleted)
                .Select(x => new {x.DataPoint})
                .ToArray();
            var dataPointsWithIndexIdsToAdd = dataPointsWithIndexIds
                .Where(x => !x.IndexId.HasValue && !x.DataPoint.IsDeleted)
                .Select(
                    x => new
                    {
                        x.DataPoint,
                        IndexId = idMapping.Add(x.DataPoint.Id)
                    })
                .ToArray();

            log.Info(
                $"Adding batch of {dataPoints.Length}: " +
                $"{dataPointsWithIndexIds.Count} without duplicates, " +
                $"{dataPointsWithIndexIdsToSkip.Length} to skip, " +
                $"{dataPointsWithIndexIdsToRemove.Length} to remove, " +
                $"{dataPointsWithIndexIdsToUpdate.Length} to update, " +
                $"{dataPointsWithIndexIdsToAdd.Length} to add");

            if (dataPointsWithIndexIdsToRemove.Any() || dataPointsWithIndexIdsToUpdate.Any())
            {
                var ids = dataPointsWithIndexIdsToRemove
                    .Concat(dataPointsWithIndexIdsToUpdate)
                    .Select(x => x.IndexId)
                    .ToArray();

                var nRemoved = index.DeleteBatch(ids);
                if (nRemoved != ids.Length)
                    log.Warn($"{index.GetType().Name} failed to remove {ids.Length} elements from index, removed {nRemoved}");
            }

            if (dataPointsWithIndexIdsToUpdate.Any() || dataPointsWithIndexIdsToAdd.Any())
            {
                index.AddBatch(
                    dataPointsWithIndexIdsToUpdate
                        .Concat(dataPointsWithIndexIdsToAdd)
                        .Select(dp => (dp.IndexId, dp.DataPoint.Vector))
                        .ToArray());
            }

            foreach (var dataPointWithIndexId in dataPointsWithIndexIdsToRemove)
            {
                idMapping.Delete(dataPointWithIndexId.DataPoint.Id);
                storage.Delete(dataPointWithIndexId.DataPoint.Id);
            }

            foreach (var dataPointWithIndexId in dataPointsWithIndexIdsToUpdate)
            {
                storage.Update(dataPointWithIndexId.DataPoint.Id, dataPointWithIndexId.DataPoint.Data);
            }

            foreach (var dataPointWithIndexId in dataPointsWithIndexIdsToAdd)
            {
                storage.Add(dataPointWithIndexId.DataPoint.Id, dataPointWithIndexId.DataPoint.Data);
            }

            log.Debug(
                $"Adding batch of {dataPoints.Length}, after: " +
                $"index size = {index.VectorCount}, " +
                $"id map size = {idMapping.Count}, " +
                $"data size = {storage.Count}");
        }

        public IReadOnlyList<IndexQueryResult<TId, TData, TVector>> FindNearest(IndexQueryDataPoint<TVector>[] queryDataPoints, int limitPerQuery)
        {
            log.Info("Starting search in index");
            var queryVectors = queryDataPoints.Select(x => x.Vector).ToArray();
            var nearest = index.FindNearest(queryVectors, limitPerQuery);
            log.Info("Finished search in index");

            var queryResults = nearest
                .Select(
                    x => x
                        .Select(y => new {IndexId = y.Id, y.Distance, Id = idMapping.GetIdByIndexId(y.Id), y.Vector})
                        .Select(
                            y => new IndexFoundDataPoint<TId, TData, TVector>
                            {
                                Id = y.Id,
                                Data = storage.Get(y.Id),
                                Vector = y.Vector,
                                Distance = y.Distance
                            })
                        .ToArray())
                .Zip(
                    queryDataPoints,
                    (foundPoints, queryPoint) => new IndexQueryResult<TId, TData, TVector>
                    {
                        QueryDataPoint = queryPoint,
                        Nearest = foundPoints
                    })
                .ToArray();

            return queryResults;
        }

        public void Dispose()
        {
            index.Dispose();
        }
    }
}

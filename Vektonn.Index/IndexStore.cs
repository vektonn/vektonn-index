using System.Collections.Generic;
using System.Linq;
using Vostok.Logging.Abstractions;

namespace Vektonn.Index
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

        public void UpdateIndex(IndexDataPointOrTombstone<TId, TData, TVector>[] dataPointOrTombstones)
        {
            log.Debug(
                $"Adding batch of {dataPointOrTombstones.Length}, before: " +
                $"index size = {index.VectorCount}, " +
                $"id map size = {idMapping.Count}, " +
                $"data size = {storage.Count}");

            var dataPointsWithIndexIds = dataPointOrTombstones
                .GroupBy(x => x.GetId(), idComparer)
                .Select(x => x.Last())
                .Select(
                    x => new
                    {
                        IndexId = idMapping.FindIndexIdById(x.GetId()),
                        DataPointOrTombstone = x
                    })
                .ToList();

            var tombstonesWithIndexIdsToRemove = dataPointsWithIndexIds
                .Where(x => x.IndexId.HasValue && x.DataPointOrTombstone.Tombstone != null)
                .Select(x => (Tombstone: x.DataPointOrTombstone.Tombstone!, IndexId: x.IndexId!.Value))
                .ToArray();
            var dataPointsWithIndexIdsToUpdate = dataPointsWithIndexIds
                .Where(x => x.IndexId.HasValue && x.DataPointOrTombstone.Tombstone == null)
                .Select(x => (DataPoint: x.DataPointOrTombstone.DataPoint!, IndexId: x.IndexId!.Value))
                .ToArray();
            var dataPointsWithIndexIdsToSkip = dataPointsWithIndexIds
                .Where(x => !x.IndexId.HasValue && x.DataPointOrTombstone.Tombstone != null)
                .ToArray();
            var dataPointsWithIndexIdsToAdd = dataPointsWithIndexIds
                .Where(x => !x.IndexId.HasValue && x.DataPointOrTombstone.Tombstone == null)
                .Select(
                    x =>
                    (
                        DataPoint: x.DataPointOrTombstone.DataPoint!,
                        IndexId: idMapping.Add(x.DataPointOrTombstone.DataPoint!.Id)
                    ))
                .ToArray();

            log.Debug(
                $"Adding batch of {dataPointOrTombstones.Length}: " +
                $"{dataPointsWithIndexIds.Count} without duplicates, " +
                $"{dataPointsWithIndexIdsToSkip.Length} to skip, " +
                $"{tombstonesWithIndexIdsToRemove.Length} to remove, " +
                $"{dataPointsWithIndexIdsToUpdate.Length} to update, " +
                $"{dataPointsWithIndexIdsToAdd.Length} to add");

            if (tombstonesWithIndexIdsToRemove.Any() || dataPointsWithIndexIdsToUpdate.Any())
            {
                var ids = tombstonesWithIndexIdsToRemove.Select(t => t.IndexId)
                    .Concat(dataPointsWithIndexIdsToUpdate.Select(t => t.IndexId))
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
                        .Select(t => (t.IndexId, t.DataPoint.Vector))
                        .ToArray());
            }

            foreach (var (tombstone, _) in tombstonesWithIndexIdsToRemove)
            {
                idMapping.Delete(tombstone.Id);
                storage.Delete(tombstone.Id);
            }

            foreach (var (dataPoint, _) in dataPointsWithIndexIdsToUpdate)
                storage.Update(dataPoint.Id, dataPoint.Data);

            foreach (var (dataPoint, _) in dataPointsWithIndexIdsToAdd)
                storage.Add(dataPoint.Id, dataPoint.Data);

            log.Debug(
                $"Adding batch of {dataPointOrTombstones.Length}, after: " +
                $"index size = {index.VectorCount}, " +
                $"id map size = {idMapping.Count}, " +
                $"data size = {storage.Count}");
        }

        public IReadOnlyList<IndexSearchResultItem<TId, TData, TVector>> FindNearest(TVector[] queryVectors, int limitPerQuery, bool retrieveVectors)
        {
            var nearest = index.FindNearest(queryVectors, limitPerQuery, retrieveVectors);

            var queryResults = nearest
                .Select(
                    tuples => tuples
                        .Select(
                            t =>
                            {
                                var id = idMapping.GetIdByIndexId(t.Id);
                                return new IndexFoundDataPoint<TId, TData, TVector>(
                                    Id: id,
                                    Data: storage.Get(id),
                                    Vector: t.Vector,
                                    Distance: t.Distance
                                );
                            })
                        .ToArray())
                .Zip(
                    queryVectors,
                    (foundPoints, queryVector) => new IndexSearchResultItem<TId, TData, TVector>(queryVector, foundPoints))
                .ToArray();

            return queryResults;
        }

        public void Dispose()
        {
            index.Dispose();
        }
    }
}

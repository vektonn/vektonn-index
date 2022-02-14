using System;
using System.Collections.Generic;
using System.Linq;

namespace Vektonn.Index.Faiss
{
    internal class FaissIndex : IIndex<DenseVector>
    {
        public readonly IntPtr IndexPtr;
        private readonly IntPtr idMapPtr;
        private readonly int vectorDimension;

        public FaissIndex(int vectorDimension, FaissMetricType metric, HnswParams? hnswParams)
        {
            if (vectorDimension <= 0)
                throw new ArgumentException(nameof(vectorDimension));

            this.vectorDimension = vectorDimension;

            var indexType = hnswParams == null
                ? "Flat"
                : $"HNSW{hnswParams.M},Flat";

            Description = $"FAISS index with VectorDimension: {vectorDimension}, Metric: {metric}, Type: {indexType}";

            FaissApi.faiss_index_factory(ref IndexPtr, vectorDimension, indexType, metric).ThrowOnFaissError();

            if (hnswParams != null)
            {
                using var pSpace = new FaissParameterSpace();
                pSpace.SetIndexParameter(IndexPtr, "efConstruction", hnswParams.EfConstruction);
                pSpace.SetIndexParameter(IndexPtr, "efSearch", hnswParams.EfSearch);

                Description += $"(efConstruction={hnswParams.EfConstruction}, efSearch={hnswParams.EfSearch})";
            }

            FaissApi.faiss_IndexIDMap2_new(ref idMapPtr, IndexPtr).ThrowOnFaissError();
        }

        public string Description { get; }

        public int VectorCount { get; private set; }

        public long DeleteBatch(long[] ids)
        {
            using var selector = new FaissIdSelectorBatch(ids);

            var nRemoved = new IntPtr();
            FaissApi.faiss_Index_remove_ids(idMapPtr, selector.Ptr, ref nRemoved).ThrowOnFaissError();

            VectorCount -= ids.Length;

            return nRemoved.ToInt64();
        }

        public void AddBatch((long Id, DenseVector Vector)[] data)
        {
            if (data.Any(v => v.Vector.Dimension != vectorDimension))
                throw new ArgumentException(nameof(vectorDimension));

            // todo: non-32-bit floats in Faiss
            var vectors = data.SelectMany(dp => dp.Vector.Coordinates.Select(x => (float)x)).ToArray();
            var ids = data.Select(dp => dp.Id).ToArray();

            FaissApi.faiss_Index_add_with_ids(idMapPtr, data.Length, vectors, ids).ThrowOnFaissError();

            VectorCount += data.Length;
        }

        public IReadOnlyList<IReadOnlyList<(long Id, double Distance, DenseVector? Vector)>> FindNearest(DenseVector[] queryVectors, int limitPerQuery, bool retrieveVectors)
        {
            foreach (var queryVector in queryVectors)
            {
                if (queryVector.Dimension != vectorDimension)
                    throw new InvalidOperationException($"queryVector.Dimension ({queryVector.Dimension}) != vectorDimension ({vectorDimension})");
            }

            var queriesCount = queryVectors.Length;

            // todo: non-32-bit floats in Faiss
            var faissQuery = new float[queriesCount * vectorDimension];
            for (var queryIndex = 0; queryIndex < queriesCount; queryIndex++)
            for (var i = 0; i < vectorDimension; i++)
                faissQuery[queryIndex * vectorDimension + i] = (float)queryVectors[queryIndex].Coordinates[i];

            var foundIds = new long[queriesCount * limitPerQuery];
            var foundDistances = new float[queriesCount * limitPerQuery];
            FaissApi.faiss_Index_search(idMapPtr, queriesCount, faissQuery, limitPerQuery, foundDistances, foundIds).ThrowOnFaissError();

            var nearest = new IReadOnlyList<(long Id, double Distance, DenseVector? Vector)>[queriesCount];
            for (var queryIndex = 0; queryIndex < queriesCount; queryIndex++)
                nearest[queryIndex] = GetNearestForQuery(queryIndex, limitPerQuery, foundIds, foundDistances, retrieveVectors);

            return nearest;
        }

        public void Dispose()
        {
            FaissApi.faiss_Index_free(IndexPtr);
            FaissApi.faiss_Index_free(idMapPtr);
        }

        private IReadOnlyList<(long Id, double Distance, DenseVector? Vector)> GetNearestForQuery(int queryIndex, int limitPerQuery, long[] foundIds, float[] foundDistances, bool retrieveVectors)
        {
            var offsetInResults = queryIndex * limitPerQuery;

            var nearestForQuery = new List<(long Id, double Distance, DenseVector? Vector)>();
            for (var i = 0; i < limitPerQuery; i++)
            {
                var id = foundIds[offsetInResults + i];
                if (id == -1)
                    break;

                var distance = (double)foundDistances[offsetInResults + i];
                var vector = retrieveVectors ? RetrieveVector(id) : null;
                nearestForQuery.Add((id, distance, vector));
            }

            return nearestForQuery;
        }

        private DenseVector RetrieveVector(long id)
        {
            var vector = new float[vectorDimension]; // todo: non-32-bit floats in Faiss
            FaissApi.faiss_Index_reconstruct(idMapPtr, id, vector).ThrowOnFaissError();

            var coordinates = new double[vectorDimension];
            for (var i = 0; i < vectorDimension; i++)
                coordinates[i] = vector[i];

            return new DenseVector(coordinates);
        }
    }
}

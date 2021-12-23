using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace Vektonn.Index.Faiss
{
    internal class FaissIndex : IIndex<DenseVector>
    {
        public readonly IntPtr IdMapPtr;
        public readonly IntPtr IndexPtr;

        private readonly string description;
        private readonly FaissMetricType metric;
        private readonly int vectorDimension;

        public FaissIndex(string description, FaissMetricType metric, int vectorDimension)
        {
            if (vectorDimension <= 0)
                throw new ArgumentException(nameof(vectorDimension));

            this.description = description;
            this.metric = metric;
            this.vectorDimension = vectorDimension;

            FaissApi.faiss_index_factory(ref IndexPtr, vectorDimension, description, metric).ThrowOnFaissError();

            FaissApi.faiss_IndexIDMap2_new(ref IdMapPtr, IndexPtr).ThrowOnFaissError();
        }

        public string Description => $"FAISS index with VectorDimension: {vectorDimension}, Description: {description}, Metric: {metric}";

        public int VectorCount { get; private set; }

        public long DeleteBatch(long[] ids)
        {
            using var selector = new FaissIdSelectorBatch(ids);

            var nRemoved = new IntPtr();
            FaissApi.faiss_Index_remove_ids(IdMapPtr, selector.Ptr, ref nRemoved).ThrowOnFaissError();

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

            FaissApi.faiss_Index_add_with_ids(IdMapPtr, data.Length, vectors, ids).ThrowOnFaissError();

            VectorCount += data.Length;
        }

        public IReadOnlyList<(long Id, double Distance, DenseVector Vector)[]> FindNearest(DenseVector[] queryVectors, int limitPerQuery)
        {
            if (queryVectors.Any(v => v.Dimension != vectorDimension))
                throw new ArgumentException(nameof(vectorDimension));

            const int maxBatchSize = 10;
            return queryVectors
                .Batch(maxBatchSize)
                .SelectMany(x => FindNearestBatch(x.Select(y => y.Coordinates).ToArray(), limitPerQuery))
                .Select(
                    resultsForQueryVector => resultsForQueryVector
                        .Select(
                            result =>
                                (result.Id,
                                    result.Distance,
                                    GetVector(result.Id))
                        )
                        .ToArray())
                .ToArray();
        }

        public void Dispose()
        {
            FaissApi.faiss_Index_free(IndexPtr);
            FaissApi.faiss_Index_free(IdMapPtr);
        }

        private IEnumerable<(long Id, double Distance)[]> FindNearestBatch(double[][] queryVectors, int limitPerQuery)
        {
            var queriesCount = queryVectors.Length;

            var foundIds = new long[queriesCount * limitPerQuery];
            var foundDistances = new float[queriesCount * limitPerQuery];

            // todo: non-32-bit floats in Faiss
            var faissQuery = queryVectors.SelectMany(q => q.Select(x => (float)x)).ToArray();

            FaissApi.faiss_Index_search(IdMapPtr, queriesCount, faissQuery, limitPerQuery, foundDistances, foundIds).ThrowOnFaissError();

            return foundIds
                .Zip(foundDistances, (id, distance) => (id, (double)distance))
                .Batch(limitPerQuery)
                .Select(x => x.Where(y => y.Item1 != -1).ToArray());
        }

        private DenseVector GetVector(long id)
        {
            var vector = new float[vectorDimension]; // todo: non-32-bit floats in Faiss
            FaissApi.faiss_Index_reconstruct(IdMapPtr, id, vector).ThrowOnFaissError();
            var coordinates = vector.Select(x => (double)x).ToArray();
            return new DenseVector(coordinates);
        }
    }
}

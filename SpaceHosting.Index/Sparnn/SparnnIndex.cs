using System;
using System.Collections.Generic;
using System.Linq;
using SpaceHosting.Index.Sparnn.Distances;
using SpaceHosting.Index.Sparnn.Helpers;

namespace SpaceHosting.Index.Sparnn
{
    internal class SparnnIndex : IIndex<SparseVector>
    {
        private readonly MatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory;
        private readonly int indicesNumber;
        private readonly int clusterSize;
        private readonly int vectorDimension;
        private IMultiClusterIndex<long>? multiClusterIndex;

        public SparnnIndex(MatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory, int indicesNumber, int clusterSize, int vectorDimension)
        {
            this.matrixMetricSearchSpaceFactory = matrixMetricSearchSpaceFactory;
            this.indicesNumber = indicesNumber;
            this.clusterSize = clusterSize;
            this.vectorDimension = vectorDimension;
        }

        public string Description => $"SPARNN index with VectorDimension: {vectorDimension}, ClusterSize: {clusterSize}, IndicesNumber: {indicesNumber}";

        public int VectorCount { get; private set; }

        public void AddBatch((long Id, SparseVector Vector)[] data)
        {
            if (data.Any(v => v.Vector.Dimension != vectorDimension))
                throw new ArgumentException(nameof(vectorDimension));

            var (ids, featureVectors) = data.Select(x => (x.Id, x.Vector.ToIndexVector()));

            if (multiClusterIndex is null)
                multiClusterIndex = new MultiClusterIndex<long>(featureVectors, ids, matrixMetricSearchSpaceFactory, clusterSize, indicesNumber);
            else
                multiClusterIndex.Insert(featureVectors, ids);

            VectorCount += data.Length;
        }

        public long DeleteBatch(long[] ids)
        {
            if (multiClusterIndex is null)
                return 0;

            multiClusterIndex.Delete(ids);

            VectorCount -= ids.Length;

            return ids.Length;
        }

        public IReadOnlyList<(long Id, double Distance, SparseVector Vector)[]> FindNearest(SparseVector[] queryVectors, int limitPerQuery)
        {
            if (queryVectors.Any(v => v.Dimension != vectorDimension))
                throw new ArgumentException(nameof(vectorDimension));

            if (multiClusterIndex is null)
                return queryVectors.Select(_ => Array.Empty<(long, double, SparseVector)>()).ToArray();

            const int clusterToSearchNumber = 2; //number of branches (clusters) to search at each level.
            //This increases recall at the cost of some speed

            return multiClusterIndex.Search(queryVectors.ToIndexVectors(), limitPerQuery, clusterToSearchNumber, indicesNumber)
                .Select(
                    vectorResults => vectorResults
                        .Select(r => (r.Element, r.Distance, r.Vector.ToModelVector()))
                        .ToArray())
                .ToArray();
        }

        public void Dispose()
        {
        }
    }
}

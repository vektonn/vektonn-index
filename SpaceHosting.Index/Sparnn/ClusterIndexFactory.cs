using System;
using System.Collections.Generic;
using SpaceHosting.Index.Sparnn.Clusters;
using SpaceHosting.Index.Sparnn.Distances;
using MSparseVector = MathNet.Numerics.LinearAlgebra.Double.SparseVector;

namespace SpaceHosting.Index.Sparnn
{
    internal static class ClusterIndexFactory
    {
        public static IClusterIndex<TRecord> Create<TRecord>(
            Random random,
            IList<MSparseVector> featureVectors,
            TRecord[] recordsData,
            IMatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory,
            int? desiredClusterSize,
            IClusterIndex<TRecord>? invoker)
            where TRecord : notnull
        {
            var recordsCount = recordsData.Length;
            var maxClusterSize = desiredClusterSize ?? Math.Max((int)Math.Sqrt(recordsCount), 1000);

            if (invoker is null)
                return new RootClusterIndex<TRecord>(random, featureVectors, recordsData, matrixMetricSearchSpaceFactory, maxClusterSize);

            var levelsCount = Math.Log(recordsCount, maxClusterSize);

            if (levelsCount > 1.4)
                return new NonTerminalClusterIndex<TRecord>(random, featureVectors, recordsData, matrixMetricSearchSpaceFactory, maxClusterSize);

            return new TerminalClusterIndex<TRecord>(random, featureVectors, recordsData, matrixMetricSearchSpaceFactory, maxClusterSize);
        }
    }
}

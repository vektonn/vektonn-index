using System;
using System.Collections.Generic;
using Vektonn.Index.Sparnn.Clusters;
using Vektonn.Index.Sparnn.Distances;
using MSparseVector = MathNet.Numerics.LinearAlgebra.Double.SparseVector;

namespace Vektonn.Index.Sparnn
{
    internal static class ClusterIndexFactory
    {
        public static IClusterIndex<TRecord> Create<TRecord>(
            Func<Random> rngFactory,
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
                return new RootClusterIndex<TRecord>(rngFactory, featureVectors, recordsData, matrixMetricSearchSpaceFactory, maxClusterSize);

            var levelsCount = Math.Log(recordsCount, maxClusterSize);

            if (levelsCount > 1.4)
                return new NonTerminalClusterIndex<TRecord>(rngFactory, featureVectors, recordsData, matrixMetricSearchSpaceFactory, maxClusterSize);

            return new TerminalClusterIndex<TRecord>(rngFactory, featureVectors, recordsData, matrixMetricSearchSpaceFactory, maxClusterSize);
        }
    }
}

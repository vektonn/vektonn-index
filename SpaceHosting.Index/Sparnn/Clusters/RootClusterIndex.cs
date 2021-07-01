using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceHosting.Index.Sparnn.Distances;
using MSparseVector = MathNet.Numerics.LinearAlgebra.Double.SparseVector;

namespace SpaceHosting.Index.Sparnn.Clusters
{
    internal class RootClusterIndex<TRecord> : BaseClusterIndex<TRecord>
        where TRecord : notnull
    {
        private readonly IMatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory;
        private IClusterIndex<TRecord> root = null!;

        public RootClusterIndex(
            Func<Random> rngFactory,
            IList<MSparseVector> featureVectors,
            TRecord[] recordsData,
            IMatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory,
            int desiredClusterSize)
            : base(rngFactory, desiredClusterSize)
        {
            this.matrixMetricSearchSpaceFactory = matrixMetricSearchSpaceFactory;
            Init(featureVectors, recordsData);
        }

        public override bool IsOverflowed => false;

        public override async Task InsertAsync(IList<MSparseVector> featureVectors, TRecord[] records)
        {
            if (!root.IsOverflowed)
            {
                await root.InsertAsync(featureVectors, records).ConfigureAwait(false);
                return;
            }

            Reindex(featureVectors, records);
        }

        public override (IList<MSparseVector> featureVectors, IList<TRecord> records) GetChildData()
        {
            return root.GetChildData();
        }

        public override Task DeleteAsync(IList<TRecord> recordsToBeDeleted)
        {
            return root.DeleteAsync(recordsToBeDeleted);
        }

        protected override Task<IEnumerable<NearestSearchResult<TRecord>[]>> SearchInternalAsync(IList<MSparseVector> featureVectors, int resultsNumber, int clustersSearchNumber)
        {
            return root.SearchAsync(featureVectors, resultsNumber, clustersSearchNumber);
        }

        protected override void Init(IList<MSparseVector> featureVectors, TRecord[] recordsData)
        {
            root = ClusterIndexFactory.Create(rngFactory, featureVectors, recordsData, matrixMetricSearchSpaceFactory, desiredClusterSize, this);
        }
    }
}

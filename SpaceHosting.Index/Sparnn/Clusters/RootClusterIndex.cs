using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceHosting.Index.Sparnn.Distances;

namespace SpaceHosting.Index.Sparnn.Clusters
{
    internal class RootClusterIndex<TRecord> : BaseClusterIndex<TRecord>
    {
        private readonly MatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory;
        private IClusterIndex<TRecord> root;

        public RootClusterIndex(
            IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors,
            TRecord[] recordsData,
            MatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory,
            int desiredClusterSize)
            : base(desiredClusterSize)
        {
            this.matrixMetricSearchSpaceFactory = matrixMetricSearchSpaceFactory;
            Init(featureVectors, recordsData);
        }

        public override bool IsOverflowed => false;

        public override async Task InsertAsync(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TRecord[] records)
        {
            if (!root.IsOverflowed)
            {
                await root.InsertAsync(featureVectors, records).ConfigureAwait(false);
                return;
            }

            Reindex(featureVectors, records);
        }

        public override (IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, IList<TRecord> records) GetChildData()
        {
            return root.GetChildData();
        }

        public override Task DeleteAsync(IList<TRecord> recordsToBeDeleted)
        {
            return root.DeleteAsync(recordsToBeDeleted);
        }

        protected override Task<IEnumerable<NearestSearchResult<TRecord>[]>> SearchInternalAsync(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, int resultsNumber, int clustersSearchNumber)
        {
            return root.SearchAsync(featureVectors, resultsNumber, clustersSearchNumber);
        }

        protected override void Init(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TRecord[] recordsData)
        {
            root = ClusterIndexFactory.Create(featureVectors, recordsData, matrixMetricSearchSpaceFactory, desiredClusterSize, this);
        }
    }
}

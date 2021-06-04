using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using SpaceHosting.Index.Sparnn.Distances;

namespace SpaceHosting.Index.Sparnn.Clusters
{
    internal abstract class BaseClusterIndex<TRecord> : IClusterIndex<TRecord>
    {
        protected readonly int desiredClusterSize;
        protected readonly int searchBatchSize;

        protected BaseClusterIndex(int desiredClusterSize)
        {
            this.desiredClusterSize = desiredClusterSize;
            searchBatchSize = desiredClusterSize;
        }

        public abstract bool IsOverflowed { get; }

        public async Task<IEnumerable<NearestSearchResult<TRecord>[]>> SearchAsync(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, int resultsNumber, int clustersToSearchNumber)
        {
            var searchTasks = featureVectors
                .Batch(searchBatchSize)
                .Select(vectorsBatch => SearchInternalAsync(vectorsBatch.ToArray(), resultsNumber, clustersToSearchNumber))
                .ToArray();

            return (await Task.WhenAll(searchTasks).ConfigureAwait(false))
                .SelectMany(r => r);
        }

        public abstract Task InsertAsync(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TRecord[] records);
        public abstract (IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, IList<TRecord> records) GetChildData();
        public abstract Task DeleteAsync(IList<TRecord> recordsToBeDeleted);
        protected abstract Task<IEnumerable<NearestSearchResult<TRecord>[]>> SearchInternalAsync(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, int resultsNumber, int clustersSearchNumber);
        protected abstract void Init(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TRecord[] records);

        protected void Reindex(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TRecord[] records)
        {
            var (existingFeatureVectors, existingRecords) = GetChildData();

            var totalRecords = existingRecords
                .Concat(records)
                .ToArray();

            var totalFeatureVectors = existingFeatureVectors
                .Concat(featureVectors)
                .ToArray();

            Init(totalFeatureVectors, totalRecords);
        }
    }
}

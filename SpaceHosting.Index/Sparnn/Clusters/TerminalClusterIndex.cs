using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaceHosting.Index.Sparnn.Distances;
using SpaceHosting.Index.Sparnn.Helpers;
using MSparseVector = MathNet.Numerics.LinearAlgebra.Double.SparseVector;

namespace SpaceHosting.Index.Sparnn.Clusters
{
    internal sealed class TerminalClusterIndex<TRecord> : BaseClusterIndex<TRecord>
        where TRecord : notnull
    {
        private readonly MatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory;
        private IMatrixMetricSearchSpace<TRecord> recordSpace = null!;
        private RecordsToIndexMap recordsToIndex = null!;

        public TerminalClusterIndex(
            IList<MSparseVector> featureVectors,
            TRecord[] recordsData,
            MatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory,
            int desiredClusterSize)
            : base(desiredClusterSize)
        {
            this.matrixMetricSearchSpaceFactory = matrixMetricSearchSpaceFactory;
            Init(featureVectors, recordsData);
        }

        public override bool IsOverflowed => recordSpace.Elements.Count > desiredClusterSize * 5;

        public override Task InsertAsync(IList<MSparseVector> featureVectors, TRecord[] records)
        {
            return Task.Run(
                () =>
                {
                    recordsToIndex.AddRecords(records);
                    Reindex(featureVectors, records);
                });
        }

        public override (IList<MSparseVector> featureVectors, IList<TRecord> records) GetChildData()
        {
            return (recordSpace.FeatureMatrix.Vectors, recordSpace.Elements);
        }

        public override Task DeleteAsync(IList<TRecord> recordsToBeDeleted)
        {
            return Task.Run(() => DeleteInternal(recordsToBeDeleted));
        }

        protected override Task<IEnumerable<NearestSearchResult<TRecord>[]>> SearchInternalAsync(IList<MSparseVector> featureVectors, int resultsNumber, int clustersSearchNumber)
        {
            return recordSpace.SearchNearestAsync(featureVectors, resultsNumber);
        }

        protected override void Init(IList<MSparseVector> featureVectors, TRecord[] recordsData)
        {
            recordSpace = matrixMetricSearchSpaceFactory.Create(featureVectors, recordsData, searchBatchSize);
            recordsToIndex = new RecordsToIndexMap(recordsData);
        }

        private void DeleteInternal(IList<TRecord> recordsToBeDeleted)
        {
            var indexesOfEntitiesToBeDeleted = recordsToIndex
                .GetIndexes(recordsToBeDeleted)
                .Where(i => i != null)
                .Select(i => i!.Value)
                .ToHashSet();

            if (indexesOfEntitiesToBeDeleted.Count == 0)
                return;

            var newRecords = recordSpace.Elements
                .Where((record, i) => !indexesOfEntitiesToBeDeleted.Contains(i));
            var newFeatureVectors = recordSpace.FeatureMatrix.Vectors
                .DeleteRows(indexesOfEntitiesToBeDeleted.ToArray());

            Init(newFeatureVectors, newRecords.ToArray());
        }

        private class RecordsToIndexMap
        {
            private readonly IDictionary<TRecord, int> recordsToIndex;

            public RecordsToIndexMap(TRecord[] recordsData)
            {
                recordsToIndex = recordsData
                    .Select((record, i) => new {Record = record, Index = i})
                    .ToDictionary(x => x.Record, x => x.Index);
            }

            public void AddRecords(TRecord[] recordsData)
            {
                var nextIndex = recordsToIndex.Keys.Count;
                foreach (var record in recordsData)
                {
                    recordsToIndex[record] = nextIndex++;
                }
            }

            public IEnumerable<int?> GetIndexes(IEnumerable<TRecord> recordsToBeDeleted)
            {
                foreach (var record in recordsToBeDeleted)
                {
                    if (recordsToIndex.TryGetValue(record, out var i))
                    {
                        yield return i;
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }
    }
}

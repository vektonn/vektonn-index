using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using SpaceHosting.Index.Sparnn.Distances;
using SpaceHosting.Index.Sparnn.Helpers;

namespace SpaceHosting.Index.Sparnn.Clusters
{
    internal sealed class NonTerminalClusterIndex<TRecord> : BaseClusterIndex<TRecord>
        where TRecord : notnull
    {
        private readonly MatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory;
        private IMatrixMetricSearchSpace<IClusterIndex<TRecord>> clusterSpace = null!;

        public NonTerminalClusterIndex(
            IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors,
            TRecord[] recordsData,
            MatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory,
            int desiredClusterSize)
            : base(desiredClusterSize)
        {
            this.matrixMetricSearchSpaceFactory = matrixMetricSearchSpaceFactory;
            Init(featureVectors, recordsData);
        }

        public override bool IsOverflowed => clusterSpace.Elements.Count > desiredClusterSize * 5;

        public override async Task InsertAsync(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TRecord[] records)
        {
            if (clusterSpace.Elements.Any(c => c.IsOverflowed))
            {
                Reindex(featureVectors, records);
                return;
            }

            var featuresGroupedByCluster = await DivideOnClustersAsync(clusterSpace, featureVectors, records).ConfigureAwait(false);

            var insertTasks = featuresGroupedByCluster
                .Select(g => g.NearestCluster.InsertAsync(g.FeatureVectors, g.Records))
                .ToArray();

            await Task.WhenAll(insertTasks).ConfigureAwait(false);
        }

        public override (IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, IList<TRecord> records) GetChildData()
        {
            var resultFeatureMatrices = new List<MathNet.Numerics.LinearAlgebra.Double.SparseVector>();
            var resultRecordArrays = new List<TRecord>();

            foreach (var cluster in clusterSpace.Elements)
            {
                var (featureMatrices, recordArrays) = cluster.GetChildData();
                resultFeatureMatrices.AddRange(featureMatrices);
                resultRecordArrays.AddRange(recordArrays);
            }

            return (resultFeatureMatrices, resultRecordArrays);
        }

        public override async Task DeleteAsync(IList<TRecord> recordsToBeDeleted)
        {
            var deleteTasks = clusterSpace.Elements
                .Select(c => c.DeleteAsync(recordsToBeDeleted))
                .ToArray();

            await Task.WhenAll(deleteTasks).ConfigureAwait(false);
        }

        protected override async Task<IEnumerable<NearestSearchResult<TRecord>[]>> SearchInternalAsync(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, int resultsNumber, int clustersSearchNumber)
        {
            var nearestClustersToVectors = (await clusterSpace.SearchNearestAsync(featureVectors, clusterSpace.Elements.Count).ConfigureAwait(false))
                .Select(searchResults => searchResults.Select(result => result.Element))
                .ToArray();

            var beginClustersSearchNumber = clustersSearchNumber;
            var vectorsGroupedByCluster = nearestClustersToVectors
                .SelectMany(
                    (clusters, i) => clusters.Take(beginClustersSearchNumber)
                        .Select(cluster => new {Vector = featureVectors[i], Cluster = cluster}))
                .GroupBy(x => x.Cluster, x => x.Vector)
                .ToArray();

            var searchTasks = vectorsGroupedByCluster
                .Select(x => x.Key.SearchAsync(x.ToArray(), resultsNumber, clustersSearchNumber))
                .ToArray();

            var resultsPerVector = (await Task.WhenAll(searchTasks).ConfigureAwait(false))
                .SelectMany(
                    (results, clusterIndex) => results.Zip(
                        vectorsGroupedByCluster[clusterIndex],
                        (result, vector) => new {Vector = vector, Results = result}))
                .SelectMany(x => x.Results.Select(result => new {x.Vector, Result = result}))
                .ToLookup(x => x.Vector, x => x.Result);

            var enrichTasks = featureVectors
                .Select(
                    (requestedVector, i) => new
                    {
                        RequestedVector = requestedVector,
                        FoundDataPoints = resultsPerVector[requestedVector].ToArray(),
                        RemainedClustersToSearch = nearestClustersToVectors[i].Skip(beginClustersSearchNumber)
                    })
                .Select(x => EnrichAsync(x.RequestedVector, x.FoundDataPoints, x.RemainedClustersToSearch))
                .ToArray();

            return (await Task.WhenAll(enrichTasks).ConfigureAwait(false))
                .Select(x => x.TakeKBest(resultsNumber, r => r.Distance));

            async Task<IList<NearestSearchResult<TRecord>>> EnrichAsync(MathNet.Numerics.LinearAlgebra.Double.SparseVector requestedVector, IList<NearestSearchResult<TRecord>> foundDataPoints, IEnumerable<IClusterIndex<TRecord>> remainedClusters)
            {
                IList<NearestSearchResult<TRecord>> remainedDataPoints = new NearestSearchResult<TRecord>[0];
                if (foundDataPoints.Count < resultsNumber)
                {
                    remainedDataPoints = await SearchVectorInClustersAsync(requestedVector, remainedClusters, resultsNumber - foundDataPoints.Count, clustersSearchNumber).ConfigureAwait(false);
                }

                return foundDataPoints.Concat(remainedDataPoints).ToArray();
            }
        }

        protected override void Init(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TRecord[] recordsData)
        {
            var clusterSize = Math.Min(desiredClusterSize, recordsData.Length);
            var clusterNumbers = Enumerable.Range(0, clusterSize).ToArray();
            var clusterSelectionVectors = featureVectors.RandomSubset(clusterSize).ToArray();

            var tempCosineDistanceSpace = matrixMetricSearchSpaceFactory.Create(clusterSelectionVectors, clusterNumbers, searchBatchSize);

            var featuresGroupedByCluster = DivideOnClustersAsync(tempCosineDistanceSpace, featureVectors, recordsData).GetAwaiter().GetResult();

            var (childClusterLeaderVectors, childClusters) = featuresGroupedByCluster
                .Select(
                    x => (
                        clusterLeaderVector: clusterSelectionVectors[x.NearestCluster],
                        cluster: ClusterIndexFactory.Create(x.FeatureVectors, x.Records, matrixMetricSearchSpaceFactory, desiredClusterSize, this)));

            clusterSpace = matrixMetricSearchSpaceFactory.Create(childClusterLeaderVectors, childClusters.ToArray(), searchBatchSize);
        }

        private async Task<IList<NearestSearchResult<TRecord>>> SearchVectorInClustersAsync(MathNet.Numerics.LinearAlgebra.Double.SparseVector vector, IEnumerable<IClusterIndex<TRecord>> clusters, int resultsNumber, int clustersSearchNumber)
        {
            var currentResultsToReturn = new List<NearestSearchResult<TRecord>>();
            foreach (var cluster in clusters)
            {
                var clusterSearchResults = (await cluster.SearchAsync(new[] {vector}, resultsNumber, clustersSearchNumber).ConfigureAwait(false))
                    .SelectMany(foundResults => foundResults)
                    .ToArray();
                currentResultsToReturn.AddRange(clusterSearchResults);

                if (currentResultsToReturn.Count >= resultsNumber)
                    break;
            }

            return currentResultsToReturn;
        }

        private static async Task<IEnumerable<(T NearestCluster, MathNet.Numerics.LinearAlgebra.Double.SparseVector[] FeatureVectors, TRecord[] Records)>> DivideOnClustersAsync<T>(IMatrixMetricSearchSpace<T> space, IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TRecord[] recordsData)
        {
            var nearestClusters = await space.SearchNearestAsync(featureVectors, 1).ConfigureAwait(false);

            return nearestClusters
                .Select(
                    (results, i) => new
                    {
                        NearestCluster = results.First().Element,
                        FeatureVector = featureVectors[i],
                        Record = recordsData[i]
                    })
                .GroupBy(x => x.NearestCluster, x => (x.FeatureVector, x.Record))
                .Select(
                    g =>
                    {
                        var (clusterFeatureVectors, clusterRecords) = g.ToArray();
                        return (NearestCluster: g.Key,
                            clusterFeatureVectors,
                            clusterRecords);
                    });
        }
    }
}

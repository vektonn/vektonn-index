using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using SpaceHosting.Index.Sparnn.Clusters;
using SpaceHosting.Index.Sparnn.Distances;
using SpaceHosting.Index.Sparnn.Helpers;
using MSparseVector = MathNet.Numerics.LinearAlgebra.Double.SparseVector;
namespace SpaceHosting.Index.Sparnn
{
    internal class MultiClusterIndex<TRecord> : IMultiClusterIndex<TRecord>
        where TRecord : notnull
    {
        private readonly IClusterIndex<TRecord>[] indices;

        public MultiClusterIndex(
            IList<MSparseVector> featureVectors,
            TRecord[] recordsData,
            MatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory,
            int? desiredClusterSize,
            int indicesNumber = 2)
        {
            indices = Enumerable.Range(0, indicesNumber)
                .Select(_ => ClusterIndexFactory.Create(featureVectors, recordsData, matrixMetricSearchSpaceFactory, desiredClusterSize, invoker: null))
                .ToArray();
        }

        public IEnumerable<NearestSearchResult<TRecord>[]> Search(
            IList<MSparseVector> featureVectors,
            int resultsNumber,
            int clustersToSearchNumber,
            int? indicesToSearchNumberInput = null)
        {
            var indicesToSearchNumber = indicesToSearchNumberInput ?? indices.Length;

            var indexSearchTasks = indices
                .Take(indicesToSearchNumber)
                .Select(index => index.SearchAsync(featureVectors, resultsNumber, clustersToSearchNumber))
                .ToArray();

            var indexSearchResults = Task.WhenAll(indexSearchTasks).GetAwaiter().GetResult();

            foreach (var vectorSearchResults in indexSearchResults.HStack())
            {
                //results from several indices can duplicate each other
                var uniqueSearchResults = vectorSearchResults
                    .DistinctBy(result => result.Element)
                    .ToArray();
                yield return uniqueSearchResults.TakeKBest(resultsNumber, r => r.Distance);
            }
        }

        public void Insert(IList<MSparseVector> featureVectors, TRecord[] recordsData)
        {
            var insertionTasks = indices
                .Select(index => index.InsertAsync(featureVectors, recordsData))
                .ToArray();

            Task.WaitAll(insertionTasks);
        }

        public void Delete(TRecord[] records)
        {
            var deleteTasks = indices
                .Select(index => index.DeleteAsync(records))
                .ToArray();

            Task.WaitAll(deleteTasks);
        }
    }
}

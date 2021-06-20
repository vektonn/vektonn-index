using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using SpaceHosting.Index.Sparnn.Helpers;
using MSparseVector = MathNet.Numerics.LinearAlgebra.Double.SparseVector;

namespace SpaceHosting.Index.Sparnn.Distances
{
    internal class JaccardBinarySingleFeatureOrientedSpace<TElement> : MatrixMetricSearchSpace<TElement>
    {
        private readonly int[][] nonZerosIndexes;
        private readonly int vectorSize;

        public JaccardBinarySingleFeatureOrientedSpace(IList<MSparseVector> featureVectors, TElement[] elements, int searchBatchSize):
            base(featureVectors, elements, searchBatchSize)
        {
            nonZerosIndexes = featureVectors.Select(x => x.NonZerosIndices()).ToArray();
            vectorSize = featureVectors.First().Count;
        }

        public override Task<IEnumerable<NearestSearchResult<TElement>[]>> SearchNearestAsync(IList<MSparseVector> featureVectors, int resultsNumber)
        {
            var features = featureVectors.Select(x => x.NonZerosIndices()).ToArray();

            IEnumerable<Tuple<int, double>> CreateNonZeroIndexes((int, double) pairs)
            {
                return nonZerosIndexes[pairs.Item1].Select(x => new Tuple<int, double>(x, 1.0));
            }

            NearestSearchResult<TElement> CreateNearestSearchResult((int, double) pairs)
            {
                return new(pairs.Item2, Elements[pairs.Item1], MSparseVector.OfIndexedEnumerable(vectorSize, CreateNonZeroIndexes(pairs)));
            }

            var res = SearchNearestAsyncInternalAsync(features, nonZerosIndexes, resultsNumber)
                .Select(searchResult => searchResult.Select(CreateNearestSearchResult).ToArray());

            return Task.FromResult(res);
        }

        protected override Matrix<double> GetDistances(SparseMatrix featureMatrix) =>
            throw new NotImplementedException();

        private static IEnumerable<List<(int, double)>> SearchNearestAsyncInternalAsync(int[][] featureVectors, int[][] searchSpace, int resultsNumber)
        {
            for (int i = 0; i < featureVectors.Length; i++)
            {
                var distances = new double[searchSpace.Length];
                for (int j = 0; j < searchSpace.Length; j++)
                {
                    distances[j] = featureVectors[i].JaccardBinaryDistance(searchSpace[j]);
                }
                yield return distances.TakeKBest(resultsNumber);
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MoreLinq;
using Vektonn.Index.Sparnn.Helpers;

namespace Vektonn.Index.Sparnn.Distances
{
    internal abstract class MatrixMetricSearchSpace<TElement> : IMatrixMetricSearchSpace<TElement>
    {
        private readonly int searchBatchSize;
        private readonly double[] zerosDistances;

        protected MatrixMetricSearchSpace(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TElement[] elements, int searchBatchSize)
        {
            if (featureVectors.Count != elements.Length)
                throw new ArgumentException("The number of feature vectors does not match the number of elements");

            FeatureMatrix = new SparseVectorsList(featureVectors);
            Elements = elements;
            this.searchBatchSize = searchBatchSize;
            zerosDistances = new double[elements.Length];
        }

        public SparseVectorsList FeatureMatrix { get; }

        public IList<TElement> Elements { get; }

        public async Task<IEnumerable<NearestSearchResult<TElement>[]>> SearchNearestAsync(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, int resultsNumber)
        {
            var searchTasks = featureVectors
                .Batch(searchBatchSize)
                .Select(SparseMatrixExtensions.CreateFromVectors)
                .Select(featureMatrix => Task.Run(() => SearchNearestInternal(featureMatrix, resultsNumber)))
                .ToArray();

            return (await Task.WhenAll(searchTasks).ConfigureAwait(false))
                .SelectMany(r => r);
        }

        protected abstract Matrix<double> GetDistances(SparseMatrix featureMatrix);

        private NearestSearchResult<TElement>[][] SearchNearestInternal(SparseMatrix featureMatrix, int resultsNumber)
        {
            if (FeatureMatrix.Vectors.Count == 0)
                return featureMatrix.EnumerateVectors().Select(_ => new NearestSearchResult<TElement>[0]).ToArray();

            var distanceMatrix = GetDistances(featureMatrix);

            return distanceMatrix.ToRowArrays()
                .Select(
                    distancesForVector => (distancesForVector ?? zerosDistances)
                        .Select((d, i) => (Distance: d, Index: i))
                        .ToArray()
                        .TakeKBest(resultsNumber, x => x.Distance)
                        .Select(x => new NearestSearchResult<TElement>(x.Distance, Elements[x.Index], FeatureMatrix[x.Index]))
                        .ToArray())
                .ToArray();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Vektonn.Index.Sparnn.Helpers;

namespace Vektonn.Index.Sparnn.Distances
{
    internal class JaccardBinaryDistanceSpace<TElement> : MatrixMetricSearchSpace<TElement>
    {
        public JaccardBinaryDistanceSpace(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TElement[] elements, int searchBatchSize)
            : base(featureVectors, elements, searchBatchSize)
        {
        }

        protected override Matrix<double> GetDistances(SparseMatrix featureMatrix)
        {
            var result = (SparseMatrixExtensions.CreateFromVectors(FeatureMatrix.Vectors) * featureMatrix.Transpose()).Transpose();
            var featureVectorsNonZeroCount = featureMatrix
                .EnumerateRows()
                .Select(row => row.Storage.EnumerateNonZero().Count())
                .ToArray();

            result.MapIndexedInplace(
                (row, col, val) => Map(val, featureVectorsNonZeroCount[row], FeatureMatrix[col].NonZerosCount),
                Zeros.Include);
            return result;
        }

        private static double Map(double nonZerosMatches, int nonZerosInFeature, int nonZerosInSpace)
        {
            return 1 - nonZerosMatches / (nonZerosInSpace + nonZerosInFeature - nonZerosMatches);
        }
    }
}

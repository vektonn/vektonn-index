using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using SpaceHosting.Index.Sparnn.Helpers;

namespace SpaceHosting.Index.Sparnn.Distances
{
    internal class CosineDistanceSpace<TElement> : MatrixMetricSearchSpace<TElement>
    {
        private readonly Vector<double> matrixNorm;

        public CosineDistanceSpace(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TElement[] elements, int searchBatchSize)
            : base(featureVectors, elements, searchBatchSize)
        {
            matrixNorm = MatrixNorm(FeatureMatrix.Vectors);
        }

        protected override Matrix<double> GetDistances(SparseMatrix featureMatrix)
        {
            var dotProduct = (FeatureMatrix * (SparseMatrix)featureMatrix.Transpose()).Transpose();
            var featuresMatrixNorm = MatrixNorm(featureMatrix.EnumerateVectors());
            var x = 1.0 / (featuresMatrixNorm.ToColumnMatrix() * matrixNorm.ToRowMatrix());

            var distances = 1 - dotProduct.PointwiseMultiply(x);
            return distances;
        }

        private static Vector<double> MatrixNorm(IEnumerable<MathNet.Numerics.LinearAlgebra.Double.SparseVector> vectors)
        {
            var norms = vectors.Select(v => v.L2Norm());

            return MathNet.Numerics.LinearAlgebra.Double.DenseVector.OfEnumerable(norms);
        }
    }
}

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;
using SpaceHosting.Index.Sparnn.Helpers;

namespace SpaceHosting.Index.Tests.Sparnn.Helpers.ListExtensionsTests
{
    public class CreateMatrixTests
    {
        private readonly IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> vectors = new[]
        {
            MathNet.Numerics.LinearAlgebra.Double.SparseVector.OfEnumerable(new[] {1.0, 0.0, 2.0, 0.0, 0.0}),
            MathNet.Numerics.LinearAlgebra.Double.SparseVector.OfEnumerable(new[] {1.0, 0.0, 2.0, 0.0, 1.0}),
            MathNet.Numerics.LinearAlgebra.Double.SparseVector.OfEnumerable(new[] {0.0, 1.0, 2.0, 0.0, 0.0}),
            MathNet.Numerics.LinearAlgebra.Double.SparseVector.OfEnumerable(new[] {1.1, 0.0, 0.0, 5.0, 0.0}),
        };

        [Test]
        public void Should_equal_to_origin_method_result()
        {
            var libraryMethodResult = SparseMatrix.OfRowVectors(vectors);

            var actualResult = SparseMatrixExtensions.CreateFromVectors(vectors);

            Assert.AreEqual(libraryMethodResult, actualResult);
        }
    }
}

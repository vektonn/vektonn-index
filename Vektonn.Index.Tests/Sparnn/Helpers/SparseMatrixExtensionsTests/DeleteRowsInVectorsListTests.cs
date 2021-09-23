using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vektonn.Index.Sparnn.Helpers;

namespace Vektonn.Index.Tests.Sparnn.Helpers.SparseMatrixExtensionsTests
{
    public class DeleteRowsInVectorsListTests
    {
        [TestCase(0, new double[] {1, 2}, Description = "Delete first row")]
        [TestCase(1, new double[] {0, 2}, Description = "Delete middle row")]
        [TestCase(2, new double[] {0, 1}, Description = "Delete last row")]
        public void DeleteOneRow(int i, double[] notDeletedRows)
        {
            var array = new[]
            {
                new double[] {0},
                new double[] {1},
                new double[] {2},
            };
            var vectors = array.Select(MathNet.Numerics.LinearAlgebra.Double.SparseVector.OfEnumerable).ToArray();

            var resultVectors = vectors.DeleteRows(i);

            Assert.That(resultVectors.Count, Is.EqualTo(2));
            Assert.That(GetFlatValues(resultVectors), Is.EqualTo(notDeletedRows));
        }

        [TestCase(0, 1, new double[] {2, 3, 4})]
        [TestCase(0, 2, new double[] {1, 3, 4})]
        [TestCase(0, 4, new double[] {1, 2, 3})]
        [TestCase(1, 2, new double[] {0, 3, 4})]
        [TestCase(1, 3, new double[] {0, 2, 4})]
        [TestCase(1, 4, new double[] {0, 2, 3})]
        [TestCase(3, 4, new double[] {0, 1, 2})]
        [TestCase(4, 3, new double[] {0, 1, 2})]
        public void DeleteTwoRows(int i1, int i2, double[] notDeletedRows)
        {
            var array = new[]
            {
                new double[] {0},
                new double[] {1},
                new double[] {2},
                new double[] {3},
                new double[] {4},
            };
            var vectors = array.Select(MathNet.Numerics.LinearAlgebra.Double.SparseVector.OfEnumerable).ToArray();

            var resultVectors = vectors.DeleteRows(i1, i2);

            Assert.That(resultVectors.Count, Is.EqualTo(3));
            Assert.That(GetFlatValues(resultVectors), Is.EqualTo(notDeletedRows));
        }

        private double[] GetFlatValues(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> vectors)
            => vectors
                .SelectMany(v => v.Storage.Enumerate().ToArray())
                .ToArray();
    }
}

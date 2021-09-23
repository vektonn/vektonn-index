using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;
using Vektonn.Index.Sparnn.Helpers;

namespace Vektonn.Index.Tests.Sparnn.Helpers.SparseMatrixExtensionsTests
{
    public class DeleteRowsTests
    {
        [TestCase(0, new[] {1, 2}, Description = "Delete first row")]
        [TestCase(1, new[] {0, 2}, Description = "Delete middle row")]
        [TestCase(2, new[] {0, 1}, Description = "Delete last row")]
        public void DeleteOneRow(int i, int[] notDeletedRows)
        {
            var array = new[]
            {
                new double[] {0},
                new double[] {1},
                new double[] {2},
            };
            var matrix = SparseMatrix.OfRowArrays(array);

            var resultMatrix = matrix.DeleteRows(i);

            Assert.That(resultMatrix.RowCount, Is.EqualTo(2));
            Assert.That(resultMatrix.Column(0).ToArray(), Is.EqualTo(notDeletedRows));
        }

        [TestCase(0, 1, new[] {2, 3, 4})]
        [TestCase(0, 2, new[] {1, 3, 4})]
        [TestCase(0, 4, new[] {1, 2, 3})]
        [TestCase(1, 2, new[] {0, 3, 4})]
        [TestCase(1, 3, new[] {0, 2, 4})]
        [TestCase(1, 4, new[] {0, 2, 3})]
        [TestCase(3, 4, new[] {0, 1, 2})]
        [TestCase(4, 3, new[] {0, 1, 2})]
        public void DeleteTwoRows(int i1, int i2, int[] notDeletedRows)
        {
            var array = new[]
            {
                new double[] {0},
                new double[] {1},
                new double[] {2},
                new double[] {3},
                new double[] {4},
            };
            var matrix = SparseMatrix.OfRowArrays(array);

            var resultMatrix = matrix.DeleteRows(i1, i2);

            Assert.That(resultMatrix.RowCount, Is.EqualTo(3));
            Assert.That(resultMatrix.Column(0).ToArray(), Is.EqualTo(notDeletedRows));
        }
    }
}

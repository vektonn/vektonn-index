using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SpaceHosting.Index.Sparnn.Distances;

namespace SpaceHosting.Index.Tests.Sparnn.Distances
{
    public class JaccardBinarySingleFeatureOrientedSpaceTests
    {
        [Test]
        public void SanityCheck()
        {
            var baseVectors = new[,]
            {
                {1.0, 0.0, 0.0, 1.0, 1.0},
                {1.0, 1.0, 0.0, 0.0, 0.0},
                {0.0, 0.0, 1.0, 0.0, 0.0},
                {0.0, 1.0, 1.0, 0.0, 0.0}
            };

            var searchVectors = new[,]
            {
                {0.0, 1.0, 1.0, 0.0, 0.0},
                {0.0, 0.0, 1.0, 1.0, 1.0},
                {1.0, 1.0, 1.0, 0.0, 1.0}
            };

            var expectedResults = new[,]
            {
                {5.0 / 5.0, 2.0 / 3.0, 1.0 / 2.0, 0.0 / 2.0},
                {2.0 / 4.0, 5.0 / 5.0, 2.0 / 3.0, 3.0 / 4.0},
                {3.0 / 5.0, 2.0 / 4.0, 3.0 / 4.0, 2.0 / 4.0}
            };

            var jaccardDistanceSpace = CreateJaccardBinaryDistanceSpace(baseVectors);
            var actualResults = jaccardDistanceSpace.SearchNearestAsync(CreateSparseVectors(searchVectors), baseVectors.Length).GetAwaiter().GetResult().ToArray();
            Assert.AreEqual(actualResults.Length, expectedResults.GetLength(0));
            for (var i = 0; i < actualResults.Length; i++)
            {
                Assert.AreEqual(actualResults[i].Length, expectedResults.GetLength(1));
                foreach (var nearestSearchResult in actualResults[i])
                {
                    Assert.AreEqual(expectedResults[i, nearestSearchResult.Element], nearestSearchResult.Distance, 1e-6);
                }
            }
        }

        [Test]
        public void OnlyZeroDistance_ShouldNotThrow()
        {
            var baseVectors = new[,]
            {
                {1.0, 0.0, 0.0, 0.0, 0.0},
                {1.0, 0.0, 0.0, 0.0, 0.0},
                {1.0, 0.0, 0.0, 0.0, 0.0},
            };

            var searchVectors = new[,]
            {
                {1.0, 0.0, 0.0, 0.0, 0.0}
            };

            var expectedResults = new[,]
            {
                {0.0 / 1.0, 0.0 / 1.0, 0.0 / 1.0}
            };

            var jaccardDistanceSpace = CreateJaccardBinaryDistanceSpace(baseVectors);
            var actualResults = jaccardDistanceSpace.SearchNearestAsync(CreateSparseVectors(searchVectors), baseVectors.Length).GetAwaiter().GetResult().ToArray();
            Assert.AreEqual(actualResults.Length, expectedResults.GetLength(0));
            for (var i = 0; i < actualResults.Length; i++)
            {
                Assert.AreEqual(actualResults[i].Length, expectedResults.GetLength(1));
                foreach (var nearestSearchResult in actualResults[i])
                {
                    Assert.AreEqual(expectedResults[i, nearestSearchResult.Element], nearestSearchResult.Distance, 1e-6);
                }
            }
        }

        private static JaccardBinarySingleFeatureOrientedSpace<int> CreateJaccardBinaryDistanceSpace(double[,] matrix)
        {
            var vectorsCount = matrix.GetLength(0);
            var baseVectors = CreateSparseVectors(matrix);
            var indexes = Enumerable
                .Range(0, vectorsCount)
                .ToArray();

            return new JaccardBinarySingleFeatureOrientedSpace<int>(baseVectors, indexes);
        }

        private static IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> CreateSparseVectors(double[,] matrix)
        {
            var vectorsCount = matrix.GetLength(0);
            var vectorsLength = matrix.GetLength(1);
            return Enumerable
                .Range(0, vectorsCount)
                .Select(row => MathNet.Numerics.LinearAlgebra.Double.SparseVector.Create(vectorsLength, col => matrix[row, col]))
                .ToList();
        }
    }
}

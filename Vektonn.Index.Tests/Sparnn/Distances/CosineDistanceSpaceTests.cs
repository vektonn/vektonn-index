using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;
using Vektonn.Index.Sparnn.Distances;
using Vektonn.Index.Sparnn.Helpers;
using MSparseVector = MathNet.Numerics.LinearAlgebra.Double.SparseVector;

namespace Vektonn.Index.Tests.Sparnn.Distances
{
    public class CosineDistanceSpaceTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void SearchInSpace_ReturnsRequestedVectorsResultsCount(int searchVectorsCount)
        {
            var vectors = CreateRandomVectors(10, 10);
            var records = CreateRandomRecords(10);
            var space = CreateSpace(vectors, records);
            var searchVectors = CreateRandomVectors(searchVectorsCount, 10).ToArray();

            var results = space.SearchNearestAsync(searchVectors, 2).GetAwaiter().GetResult().ToArray();

            Assert.That(results.Length, Is.EqualTo(searchVectorsCount));
        }

        [Test]
        public void SearchInEmptySpace_ReturnsEmptyResults()
        {
            var emptyVectors = SparseMatrix.Create(0, 10, 0).EnumerateVectors().ToArray();
            var space = CreateSpace(emptyVectors, new string[0]);
            var searchVectors = CreateRandomVectors(5, 10).ToArray();

            var results = space.SearchNearestAsync(searchVectors, 2).GetAwaiter().GetResult().ToArray();

            Assert.True(results.All(resultsPerVector => resultsPerVector.Length == 0));
        }

        [TestCase(1, ExpectedResult = 1)]
        [TestCase(2, ExpectedResult = 2)]
        [TestCase(10, ExpectedResult = 10)]
        [TestCase(100, ExpectedResult = 10)]
        public int SearchInSpace_ReturnsRequestedNearestVectorsCount(int requestedResultsCount)
        {
            var vectors = CreateRandomVectors(10, 10);
            var records = CreateRandomRecords(10);
            var space = CreateSpace(vectors, records);
            var searchVectors = CreateRandomVectors(5, 10).ToArray();

            var results = space.SearchNearestAsync(searchVectors, requestedResultsCount).GetAwaiter().GetResult().ToArray();

            return results.First().Length;
        }

        [Test]
        public void SearchInSpace_FoundInFactNearestVectors()
        {
            var featureVectors = ToVectors(
                new[]
                {
                    new[] {1.0, 0.0, 0.0},
                    new[] {0.0, 2.0, 0.0},
                });
            var records = new[] {"1", "2"};
            var space = CreateSpace(featureVectors, records);

            var results = space.SearchNearestAsync(new[] {featureVectors[1]}, 1).GetAwaiter().GetResult().First();

            Assert.That(results[0].Vector, Is.EqualTo(featureVectors[1]));
        }

        [Test]
        public void SearchInSpace_FoundInFactTwoNearestVectors()
        {
            var featureVectors = ToVectors(
                new[]
                {
                    new[] {1.0, 0.0, 0.0, 0.0},
                    new[] {0.0, 2.0, 0.0, 0.0},
                    new[] {0.0, 0.0, 3.0, 0.0},
                    new[] {0.0, 0.0, 3.0, 0.1},
                });
            var records = new[] {"1", "2", "3", "4"};
            var space = CreateSpace(featureVectors, records);

            var actualNearestVectors = space.SearchNearestAsync(new[] {featureVectors[2]}, 2)
                .GetAwaiter()
                .GetResult()
                .First()
                .Select(result => result.Vector)
                .ToArray();

            Assert.That(actualNearestVectors, Is.EqualTo(new[] {featureVectors[2], featureVectors[3]}));
        }

        private static IMatrixMetricSearchSpace<T> CreateSpace<T>(IList<MSparseVector> featureVectors, T[] records)
        {
            return new CosineDistanceSpace<T>(featureVectors, records, 1);
        }

        private static IList<MSparseVector> CreateRandomVectors(int rows, int columns)
        {
            var random = new Random();
            return SparseMatrix.Create(rows, columns, (i, j) => random.NextDouble()).EnumerateVectors().ToArray();
        }

        private static int[] CreateRandomRecords(int count)
        {
            var random = new Random();
            return Enumerable.Range(0, count).Select(_ => random.Next()).ToArray();
        }

        private IList<MSparseVector> ToVectors(double[][] rowsArray) => rowsArray.Select(MSparseVector.OfEnumerable).ToArray();
    }
}

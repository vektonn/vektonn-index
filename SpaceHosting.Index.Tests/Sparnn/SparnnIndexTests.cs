using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpaceHosting.Index.Sparnn;
using SpaceHosting.Index.Sparnn.Distances;

namespace SpaceHosting.Index.Tests.Sparnn
{
    public class SparnnIndexTests
    {
        private const int VectorDimension = 1024;
        private const double SinglePrecisionEpsilon = 1e-06;

        private readonly Random random = new Random();

        [Test]
        [Repeat(100)]
        public void FindNearest_AddRandomDataPointsAndSearch_Cosine()
        {
            using var index = new SparnnIndex(
                rngFactory: () => new Random(),
                new MatrixMetricSearchSpaceFactory(MatrixMetricSearchSpaceAlgorithm.Cosine),
                indicesNumber: 2,
                clusterSize: 1000,
                VectorDimension);

            FindNearest_AddRandomDataPointsAndSearch(index, binary: false);
        }

        [Test]
        [Repeat(100)]
        public void FindNearest_AddRandomDataPointsAndSearch_JaccardBinary()
        {
            using var index = new SparnnIndex(
                rngFactory: () => new Random(),
                new MatrixMetricSearchSpaceFactory(MatrixMetricSearchSpaceAlgorithm.JaccardBinary),
                indicesNumber: 2,
                clusterSize: 1000,
                VectorDimension);

            FindNearest_AddRandomDataPointsAndSearch(index, binary: true);
        }

        private void FindNearest_AddRandomDataPointsAndSearch(SparnnIndex index, bool binary)
        {
            var firstIndexDataPoint = (Id: 1, Vector: RandomVector(binary));
            var secondIndexDataPoint = (Id: 2, Vector: RandomVector(binary));
            var indexDataPoints = new (long Id, SparseVector Vector)[]
            {
                firstIndexDataPoint,
                secondIndexDataPoint,
            };

            index.AddBatch(indexDataPoints);

            var foundDataPoints = index.FindNearest(new[] {firstIndexDataPoint.Vector}, limitPerQuery: indexDataPoints.Length).Single();

            foundDataPoints.Length.Should().Be(2);

            foundDataPoints[0].Id.Should().Be(1);
            foundDataPoints[0].Distance.Should().BeApproximately(0.0, SinglePrecisionEpsilon);
            foundDataPoints[0].Vector.Should().BeEquivalentTo(firstIndexDataPoint.Vector);

            foundDataPoints[1].Id.Should().Be(2);
            foundDataPoints[1].Distance.Should().BeGreaterThan(SinglePrecisionEpsilon);
            foundDataPoints[1].Vector.Should().BeEquivalentTo(secondIndexDataPoint.Vector);
        }

        private SparseVector RandomVector(bool binary, double density = 0.01)
        {
            var columnIndices = new List<int>();
            var coordinates = new List<double>();
            for (var d = 0; d < VectorDimension; d++)
            {
                if (!(random.NextDouble() < density))
                    continue;

                columnIndices.Add(d);
                coordinates.Add(binary ? 1.0 : random.NextDouble());
            }

            return new SparseVector(VectorDimension, columnIndices.ToArray(), coordinates.ToArray());
        }
    }
}

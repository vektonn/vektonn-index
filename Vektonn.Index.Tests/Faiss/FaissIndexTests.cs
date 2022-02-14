using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.Index.Faiss;

namespace Vektonn.Index.Tests.Faiss
{
    [Category("RequiresNativeFaissLibrary")]
    public class FaissIndexTests
    {
        // from https://en.wikipedia.org/wiki/Machine_epsilon
        private const double HalfPrecisionEpsilon = 1e-03;
        private const double SinglePrecisionEpsilon = 1e-06;

        private readonly Random random = new Random();

        [Test]
        public void Tune_HnswParameters()
        {
            using var index = new FaissIndex(
                vectorDimension: 42,
                FaissMetricType.METRIC_L2,
                new HnswParams(M: 64, EfConstruction: 800, EfSearch: 200));

            using var parameterSpace = new FaissParameterSpace();

            parameterSpace.GetIndexParameter(index.IndexPtr, "efSearch").Should().Be(200);
            parameterSpace.GetIndexParameter(index.IndexPtr, "efConstruction").Should().Be(800);

            parameterSpace.SetIndexParameter(index.IndexPtr, "efSearch", 100);
            parameterSpace.SetIndexParameter(index.IndexPtr, "efConstruction", 500);

            parameterSpace.GetIndexParameter(index.IndexPtr, "efSearch").Should().Be(100);
            parameterSpace.GetIndexParameter(index.IndexPtr, "efConstruction").Should().Be(500);
        }

        [Test]
        [Repeat(1000)]
        public void FindNearest_AddRandomDataPointsAndSearch_FlatL2()
        {
            using var index = new FaissIndex(vectorDimension: 2, FaissMetricType.METRIC_L2, hnswParams: null);

            var firstIndexDataPoint = (Id: 1, Vector: RandomVector(2));
            var secondIndexDataPoint = (Id: 2, Vector: RandomVector(2));
            var indexDataPoints = new (long Id, DenseVector Vector)[]
            {
                firstIndexDataPoint,
                secondIndexDataPoint,
            };

            index.AddBatch(indexDataPoints);

            var queryDataPoints = indexDataPoints.Select(x => x.Vector).ToArray();

            var foundDataPoints = index.FindNearest(queryDataPoints, limitPerQuery: 2, retrieveVectors: true);

            var firstQueryFoundDataPoints = foundDataPoints[0];
            Assert.AreEqual(firstIndexDataPoint.Id, firstQueryFoundDataPoints[0].Id);
            Assert.AreEqual(0.0, firstQueryFoundDataPoints[0].Distance, SinglePrecisionEpsilon);
            Assert.AreEqual(secondIndexDataPoint.Id, firstQueryFoundDataPoints[1].Id);
            Assert.IsTrue(!0.0.Equals(firstQueryFoundDataPoints[1].Distance));

            var secondQueryFoundDataPoints = foundDataPoints[1];
            Assert.AreEqual(secondIndexDataPoint.Id, secondQueryFoundDataPoints[0].Id);
            Assert.AreEqual(0.0, secondQueryFoundDataPoints[0].Distance, SinglePrecisionEpsilon);
            Assert.AreEqual(firstIndexDataPoint.Id, secondQueryFoundDataPoints[1].Id);
            Assert.IsTrue(!0.0.Equals(secondQueryFoundDataPoints[1].Distance));
        }

        [Test]
        public void FindNearest_AddStaticDataPoints_FlatL2()
        {
            using var index = new FaissIndex(vectorDimension: 2, FaissMetricType.METRIC_L2, hnswParams: null);

            var firstIndexDataPoint = (Id: 1, Vector: Vector(1.0, 2.0));
            var secondIndexDataPoint = (Id: 2, Vector: Vector(10.0, 20.0));
            var indexDataPoints = new (long Id, DenseVector Vector)[]
            {
                firstIndexDataPoint,
                secondIndexDataPoint,
            };

            index.AddBatch(indexDataPoints);

            var queryDataPoint = Vector(2.0, 3.0);
            var queryDataPoints = new[] {queryDataPoint};

            var foundDataPoints = index.FindNearest(queryDataPoints, limitPerQuery: 2, retrieveVectors: true);

            var firstQueryFoundDataPoints = foundDataPoints[0];
            Assert.AreEqual(firstIndexDataPoint.Id, firstQueryFoundDataPoints[0].Id);
            Assert.AreEqual(2.0, firstQueryFoundDataPoints[0].Distance, SinglePrecisionEpsilon);
            Assert.AreEqual(secondIndexDataPoint.Id, firstQueryFoundDataPoints[1].Id);
            Assert.AreEqual(353.0, firstQueryFoundDataPoints[1].Distance, SinglePrecisionEpsilon);
        }

        [Test]
        public void FindNearest_AddAndDeleteStaticDataPointsAndSearch_FlatL2()
        {
            using var index = new FaissIndex(vectorDimension: 2, FaissMetricType.METRIC_L2, hnswParams: null);

            var firstIndexDataPoint = (Id: 1, Vector: Vector(1.0, 2.0));
            var secondIndexDataPoint = (Id: 2, Vector: Vector(10.0, 20.0));
            var thirdIndexDataPoint = (Id: 2, Vector: Vector(11.0, 21.0));
            var indexDataPoints = new (long Id, DenseVector Vector)[]
            {
                firstIndexDataPoint,
                secondIndexDataPoint,
                thirdIndexDataPoint
            };

            index.AddBatch(indexDataPoints);
            index.DeleteBatch(new long[] {firstIndexDataPoint.Id});

            var queryDataPoint = Vector(2.0, 3.0);
            var queryDataPoints = new[] {queryDataPoint};

            var foundDataPoints = index.FindNearest(queryDataPoints, limitPerQuery: 2, retrieveVectors: true);

            var firstQueryFoundDataPoints = foundDataPoints[0];
            Assert.AreEqual(secondIndexDataPoint.Id, firstQueryFoundDataPoints[0].Id);
            Assert.AreEqual(353.0, firstQueryFoundDataPoints[0].Distance, SinglePrecisionEpsilon);
            Assert.AreEqual(thirdIndexDataPoint.Id, firstQueryFoundDataPoints[1].Id);
            Assert.AreEqual(405.0, firstQueryFoundDataPoints[1].Distance, SinglePrecisionEpsilon);
        }

        [Test]
        public void FindNearest_AddAndUpdateStaticDataPointsAndSearch_FlatL2()
        {
            using var index = new FaissIndex(vectorDimension: 2, FaissMetricType.METRIC_L2, hnswParams: null);

            var firstIndexDataPoint = (Id: 1, Vector: Vector(1.0, 2.0));
            var secondIndexDataPoint = (Id: 2, Vector: Vector(10.0, 20.0));
            var thirdIndexDataPoint = (Id: 3, Vector: Vector(11.0, 21.0));
            var indexDataPoints = new (long Id, DenseVector Vector)[]
            {
                firstIndexDataPoint,
                secondIndexDataPoint,
                thirdIndexDataPoint
            };

            var firstIndexDataPointUpdate = (Id: 1, Vector: Vector(5.0, 6.0));
            var updateDataPoints = new (long Id, DenseVector Vector)[]
            {
                firstIndexDataPointUpdate
            };

            index.AddBatch(indexDataPoints);
            index.DeleteBatch(new long[] {firstIndexDataPoint.Id});
            index.AddBatch(updateDataPoints);

            var queryDataPoint = Vector(2.0, 3.0);
            var queryDataPoints = new[] {queryDataPoint};

            var foundDataPoints = index.FindNearest(queryDataPoints, limitPerQuery: 2, retrieveVectors: true);

            var firstFoundDataPoint = foundDataPoints[0][0];
            firstFoundDataPoint.Id.Should().Be(firstIndexDataPointUpdate.Id);
            firstFoundDataPoint.Vector.Should().BeEquivalentTo(firstIndexDataPointUpdate.Vector);
            firstFoundDataPoint.Distance.Should().BeApproximately(18.0, SinglePrecisionEpsilon);

            var secondFoundDataPoint = foundDataPoints[0][1];
            secondFoundDataPoint.Id.Should().Be(secondIndexDataPoint.Id);
            secondFoundDataPoint.Vector.Should().BeEquivalentTo(secondIndexDataPoint.Vector);
            secondFoundDataPoint.Distance.Should().BeApproximately(353.0, SinglePrecisionEpsilon);
        }

        [Test]
        public void FindNearest_InitEmptyDataPointsAndSearch_FlatL2()
        {
            using var index = new FaissIndex(vectorDimension: 2, FaissMetricType.METRIC_L2, hnswParams: null);

            var foundDataPoints = index.FindNearest(new[] {Vector(5.0, 6.0)}, limitPerQuery: 2, retrieveVectors: true);

            foundDataPoints[0].Should().BeEmpty();
        }

        [Test]
        public void FindNearest_AddAndClearStaticDataPointsAndSearch_FlatL2()
        {
            using var index = new FaissIndex(vectorDimension: 2, FaissMetricType.METRIC_L2, hnswParams: null);

            var firstIndexDataPoint = (Id: 1, Vector: Vector(1.0, 2.0));
            var secondIndexDataPoint = (Id: 2, Vector: Vector(10.0, 20.0));
            var indexDataPoints = new (long Id, DenseVector Vector)[]
            {
                firstIndexDataPoint,
                secondIndexDataPoint,
            };

            index.AddBatch(indexDataPoints);
            index.DeleteBatch(indexDataPoints.Select(x => x.Id).ToArray());

            var foundDataPoints = index.FindNearest(new[] {Vector(5.0, 6.0)}, limitPerQuery: 2, retrieveVectors: true);

            foundDataPoints[0].Should().BeEmpty();
        }

        private DenseVector RandomVector(int dimensionCount)
        {
            var coordinates = Enumerable.Range(0, dimensionCount).Select(_ => random.NextDouble()).ToArray();
            return Vector(coordinates);
        }

        private static DenseVector Vector(params double[] coordinates)
        {
            return new DenseVector(coordinates);
        }
    }
}

using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpaceHosting.Index.Faiss;

namespace SpaceHosting.Index.Tests.Faiss
{
    [Category("RequiresNativeFaissLibrary")]
    public class FaissIndexTests
    {
        // from https://en.wikipedia.org/wiki/Machine_epsilon
        private const double halfPrecisionEpsilon = 1e-03;
        private const double singlePrecisionEpsilon = 1e-06;
        private static readonly Random random = new Random();

        [Test]
        public void FindNearest_AddRandomDataPointsAndSearch_FlatL2()
        {
            for (var i = 0; i < 1000; i++)
            {
                var index = new FaissIndex("Flat", FaissMetricType.METRIC_L2, 2);

                var firstIndexDataPoint = (Id: 1, Vector: CreateRandomVector(2));
                var secondIndexDataPoint = (Id: 2, Vector: CreateRandomVector(2));
                var indexDataPoints = new (long Id, DenseVector Vector)[]
                {
                    firstIndexDataPoint,
                    secondIndexDataPoint,
                };

                index.AddBatch(indexDataPoints);

                var queryDataPoints = indexDataPoints.Select(x => x.Vector).ToArray();

                var foundDataPoints = index.FindNearest(queryDataPoints, 2).ToArray();

                var firstQueryFoundDataPoints = foundDataPoints[0];
                Assert.AreEqual(firstIndexDataPoint.Id, firstQueryFoundDataPoints[0].Id);
                Assert.AreEqual(0.0, firstQueryFoundDataPoints[0].Distance, singlePrecisionEpsilon);
                Assert.AreEqual(secondIndexDataPoint.Id, firstQueryFoundDataPoints[1].Id);
                Assert.IsTrue(!0.0.Equals(firstQueryFoundDataPoints[1].Distance));

                var secondQueryFoundDataPoints = foundDataPoints[1];
                Assert.AreEqual(secondIndexDataPoint.Id, secondQueryFoundDataPoints[0].Id);
                Assert.AreEqual(0.0, secondQueryFoundDataPoints[0].Distance, singlePrecisionEpsilon);
                Assert.AreEqual(firstIndexDataPoint.Id, secondQueryFoundDataPoints[1].Id);
                Assert.IsTrue(!0.0.Equals(secondQueryFoundDataPoints[1].Distance));

                index.Dispose();
            }
        }

        [Test]
        public void FindNearest_AddStaticDataPoints_FlatL2()
        {
            var index = new FaissIndex("Flat", FaissMetricType.METRIC_L2, 2);

            var firstIndexDataPoint = (Id: 1, Vector: CreateVector(1.0, 2.0));
            var secondIndexDataPoint = (Id: 2, Vector: CreateVector(10.0, 20.0));
            var indexDataPoints = new (long Id, DenseVector Vector)[]
            {
                firstIndexDataPoint,
                secondIndexDataPoint,
            };

            index.AddBatch(indexDataPoints);

            var queryDataPoint = CreateVector(2.0, 3.0);
            var queryDataPoints = new[] {queryDataPoint};

            var foundDataPoints = index.FindNearest(queryDataPoints, 2).ToArray();

            var firstQueryFoundDataPoints = foundDataPoints[0];
            Assert.AreEqual(firstIndexDataPoint.Id, firstQueryFoundDataPoints[0].Id);
            Assert.AreEqual(2.0, firstQueryFoundDataPoints[0].Distance, singlePrecisionEpsilon);
            Assert.AreEqual(secondIndexDataPoint.Id, firstQueryFoundDataPoints[1].Id);
            Assert.AreEqual(353.0, firstQueryFoundDataPoints[1].Distance, singlePrecisionEpsilon);

            index.Dispose();
        }

        [Test]
        public void FindNearest_AddAndDeleteStaticDataPointsAndSearch_FlatL2()
        {
            var index = new FaissIndex("Flat", FaissMetricType.METRIC_L2, 2);

            var firstIndexDataPoint = (Id: 1, Vector: CreateVector(1.0, 2.0));
            var secondIndexDataPoint = (Id: 2, Vector: CreateVector(10.0, 20.0));
            var thirdIndexDataPoint = (Id: 2, Vector: CreateVector(11.0, 21.0));
            var indexDataPoints = new (long Id, DenseVector Vector)[]
            {
                firstIndexDataPoint,
                secondIndexDataPoint,
                thirdIndexDataPoint
            };

            index.AddBatch(indexDataPoints);
            index.DeleteBatch(new long[] {firstIndexDataPoint.Id});

            var queryDataPoint = CreateVector(2.0, 3.0);
            var queryDataPoints = new[] {queryDataPoint};

            var foundDataPoints = index.FindNearest(queryDataPoints, 2).ToArray();

            var firstQueryFoundDataPoints = foundDataPoints[0];
            Assert.AreEqual(secondIndexDataPoint.Id, firstQueryFoundDataPoints[0].Id);
            Assert.AreEqual(353.0, firstQueryFoundDataPoints[0].Distance, singlePrecisionEpsilon);
            Assert.AreEqual(thirdIndexDataPoint.Id, firstQueryFoundDataPoints[1].Id);
            Assert.AreEqual(405.0, firstQueryFoundDataPoints[1].Distance, singlePrecisionEpsilon);

            index.Dispose();
        }

        [Test]
        public void FindNearest_AddAndUpdateStaticDataPointsAndSearch_FlatL2()
        {
            var index = new FaissIndex("Flat", FaissMetricType.METRIC_L2, 2);

            var firstIndexDataPoint = (Id: 1, Vector: CreateVector(1.0, 2.0));
            var secondIndexDataPoint = (Id: 2, Vector: CreateVector(10.0, 20.0));
            var thirdIndexDataPoint = (Id: 3, Vector: CreateVector(11.0, 21.0));
            var indexDataPoints = new (long Id, DenseVector Vector)[]
            {
                firstIndexDataPoint,
                secondIndexDataPoint,
                thirdIndexDataPoint
            };

            var firstIndexDataPointUpdate = (Id: 1, Vector: CreateVector(5.0, 6.0));
            var updateDataPoints = new (long Id, DenseVector Vector)[]
            {
                firstIndexDataPointUpdate
            };

            index.AddBatch(indexDataPoints);
            index.DeleteBatch(new long[] {firstIndexDataPoint.Id});
            index.AddBatch(updateDataPoints);

            var queryDataPoint = CreateVector(2.0, 3.0);
            var queryDataPoints = new[] {queryDataPoint};

            var foundDataPoints = index.FindNearest(queryDataPoints, 2).ToArray();

            var firstFoundDataPoint = foundDataPoints[0][0];
            firstFoundDataPoint.Id.Should().Be(firstIndexDataPointUpdate.Id);
            firstFoundDataPoint.Vector.Should().BeEquivalentTo(firstIndexDataPointUpdate.Vector);
            firstFoundDataPoint.Distance.Should().BeApproximately(18.0, singlePrecisionEpsilon);

            var secondFoundDataPoint = foundDataPoints[0][1];
            secondFoundDataPoint.Id.Should().Be(secondIndexDataPoint.Id);
            secondFoundDataPoint.Vector.Should().BeEquivalentTo(secondIndexDataPoint.Vector);
            secondFoundDataPoint.Distance.Should().BeApproximately(353.0, singlePrecisionEpsilon);

            index.Dispose();
        }

        [Test]
        public void FindNearest_InitEmptyDataPointsAndSearch_FlatL2()
        {
            var index = new FaissIndex("Flat", FaissMetricType.METRIC_L2, 2);

            var foundDataPoints = index.FindNearest(new[] {CreateVector(5.0, 6.0)}, 2).ToArray();

            CollectionAssert.IsEmpty(foundDataPoints[0]);

            index.Dispose();
        }

        [Test]
        public void FindNearest_AddAndClearStaticDataPointsAndSearch_FlatL2()
        {
            var index = new FaissIndex("Flat", FaissMetricType.METRIC_L2, 2);

            var firstIndexDataPoint = (Id: 1, Vector: CreateVector(1.0, 2.0));
            var secondIndexDataPoint = (Id: 2, Vector: CreateVector(10.0, 20.0));
            var indexDataPoints = new (long Id, DenseVector Vector)[]
            {
                firstIndexDataPoint,
                secondIndexDataPoint,
            };

            index.AddBatch(indexDataPoints);
            index.DeleteBatch(indexDataPoints.Select(x => x.Id).ToArray());

            var foundDataPoints = index.FindNearest(new[] {CreateVector(5.0, 6.0)}, 2).ToArray();

            CollectionAssert.IsEmpty(foundDataPoints[0]);

            index.Dispose();
        }

        private DenseVector CreateRandomVector(int dimensionCount)
        {
            var coordinates = Enumerable.Range(0, dimensionCount).Select(_ => random.NextDouble()).ToArray();
            return CreateVector(coordinates);
        }

        private DenseVector CreateVector(params double[] coordinates)
        {
            if (coordinates is null)
                throw new ArgumentNullException();

            return new DenseVector(coordinates);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using Vostok.Logging.Abstractions;

namespace SpaceHosting.Index.Tests
{
    public class IndexStoreTests
    {
        private const double singlePrecisionEpsilon = 1e-06;
        private static readonly Random random = new Random();
        private ILog log;
        private IIndexIdMapping<int> idMapping;
        private IIndexDataStorage<int, string> storage;
        private IIndex<DenseVector> index;

        [SetUp]
        public void SetUp()
        {
            log = A.Fake<ILog>();
            idMapping = A.Fake<IIndexIdMapping<int>>();
            storage = A.Fake<IIndexDataStorage<int, string>>();
            index = A.Fake<IIndex<DenseVector>>();
        }

        [Test]
        public void AddBatch()
        {
            var dataPoints = new[]
            {
                new IndexDataPoint<int, string, DenseVector> {Id = 11, Data = "data_11", IsDeleted = false, Vector = new DenseVector(GetRandomCoordinates())},
                new IndexDataPoint<int, string, DenseVector> {Id = 12, Data = "data_12", IsDeleted = false, Vector = new DenseVector(GetRandomCoordinates())},
                new IndexDataPoint<int, string, DenseVector> {Id = 21, Data = "data_21", IsDeleted = false, Vector = new DenseVector(GetRandomCoordinates())},
                new IndexDataPoint<int, string, DenseVector> {Id = 22, Data = "data_22", IsDeleted = false, Vector = new DenseVector(GetRandomCoordinates())},
                new IndexDataPoint<int, string, DenseVector> {Id = 31, Data = "data_31", IsDeleted = true, Vector = new DenseVector(GetRandomCoordinates())},
                new IndexDataPoint<int, string, DenseVector> {Id = 32, Data = "data_32", IsDeleted = true, Vector = new DenseVector(GetRandomCoordinates())},
                new IndexDataPoint<int, string, DenseVector> {Id = 41, Data = "data_41", IsDeleted = true, Vector = new DenseVector(GetRandomCoordinates())},
                new IndexDataPoint<int, string, DenseVector> {Id = 42, Data = "data_42", IsDeleted = true, Vector = new DenseVector(GetRandomCoordinates())},
            };

            A.CallTo(() => idMapping.Count).Returns(4);
            A.CallTo(() => idMapping.FindIndexIdById(11)).Returns(1011);
            A.CallTo(() => idMapping.FindIndexIdById(12)).Returns(1012);
            A.CallTo(() => idMapping.FindIndexIdById(21)).Returns(null);
            A.CallTo(() => idMapping.FindIndexIdById(22)).Returns(null);
            A.CallTo(() => idMapping.FindIndexIdById(31)).Returns(1031);
            A.CallTo(() => idMapping.FindIndexIdById(32)).Returns(1032);
            A.CallTo(() => idMapping.FindIndexIdById(41)).Returns(null);
            A.CallTo(() => idMapping.FindIndexIdById(42)).Returns(null);

            A.CallTo(() => idMapping.Add(21)).Returns(1021);
            A.CallTo(() => idMapping.Add(22)).Returns(1022);

            A.CallTo(() => storage.Count).Returns(4);

            A.CallTo(() => index.VectorCount).Returns(10);

            var vectorsToAdd = new[]
            {
                dataPoints[0].Vector,
                dataPoints[1].Vector,
                dataPoints[2].Vector,
                dataPoints[3].Vector,
            };

            var indexStore = new IndexStore<int, string, DenseVector>(log, index, idMapping, storage, EqualityComparer<int>.Default);
            indexStore.AddBatch(dataPoints);

            A.CallTo(() => idMapping.Delete(31)).MustHaveHappenedOnceExactly();
            A.CallTo(() => idMapping.Delete(32)).MustHaveHappenedOnceExactly();

            A.CallTo(() => storage.Update(11, "data_11")).MustHaveHappenedOnceExactly();
            A.CallTo(() => storage.Update(12, "data_12")).MustHaveHappenedOnceExactly();
            A.CallTo(() => storage.Add(21, "data_21")).MustHaveHappenedOnceExactly();
            A.CallTo(() => storage.Add(22, "data_22")).MustHaveHappenedOnceExactly();
            A.CallTo(() => storage.Delete(31)).MustHaveHappenedOnceExactly();
            A.CallTo(() => storage.Delete(32)).MustHaveHappenedOnceExactly();

            var deleteBatchSequence = new long[] {1031, 1032, 1011, 1012};
            A.CallTo(() => index.DeleteBatch(A<long[]>.That.IsSameSequenceAs(deleteBatchSequence))).MustHaveHappenedOnceExactly();

            var addBatchSequence = new (long Id, DenseVector Vector)[]
            {
                (Id: 1011, Vector: vectorsToAdd[0]),
                (Id: 1012, Vector: vectorsToAdd[1]),
                (Id: 1021, Vector: vectorsToAdd[2]),
                (Id: 1022, Vector: vectorsToAdd[3]),
            };
            A.CallTo(() => index.AddBatch(A<(long Id, DenseVector Vector)[]>.That.IsSameSequenceAs(addBatchSequence))).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void FindNearest_AttachVector()
        {
            var dataPoint1 = new IndexQueryDataPoint<DenseVector> {Vector = new DenseVector(GetRandomCoordinates())};
            var dataPoint2 = new IndexQueryDataPoint<DenseVector> {Vector = new DenseVector(GetRandomCoordinates())};
            var vector1Nearest1 = new DenseVector(GetRandomCoordinates());
            var vector1Nearest2 = new DenseVector(GetRandomCoordinates());

            var queryDataPoints = new[]
            {
                dataPoint1,
                dataPoint2,
            };

            A.CallTo(
                    () => index.FindNearest(
                        A<DenseVector[]>.That.IsSameSequenceAs(dataPoint1.Vector, dataPoint2.Vector),
                        2))
                .Returns(
                    new[]
                    {
                        new (long Id, double Distance, DenseVector Vector)[]
                        {
                            (Id: 1011, Distance: 0.11, vector1Nearest1),
                            (Id: 1012, Distance: 0.12, vector1Nearest2),
                        },
                        new (long Id, double Distance, DenseVector Vector)[] {}
                    });

            A.CallTo(() => idMapping.GetIdByIndexId(1011)).Returns(11);
            A.CallTo(() => idMapping.GetIdByIndexId(1012)).Returns(12);
            A.CallTo(() => storage.Get(11)).Returns("data_11");
            A.CallTo(() => storage.Get(12)).Returns("data_12");

            var indexStore = new IndexStore<int, string, DenseVector>(log, index, idMapping, storage, EqualityComparer<int>.Default);
            var foundQueryResults = indexStore.FindNearest(queryDataPoints, 2).ToArray();

            Assert.Multiple(
                () =>
                {
                    Assert.AreEqual(2, foundQueryResults.Length);
                    Assert.AreEqual(2, foundQueryResults[0].Nearest.Length);
                    Assert.AreEqual(0, foundQueryResults[1].Nearest.Length);

                    CollectionAssert.AreEqual(dataPoint1.Vector.Coordinates, foundQueryResults[0].QueryDataPoint.Vector.Coordinates);
                    CollectionAssert.AreEqual(dataPoint2.Vector.Coordinates, foundQueryResults[1].QueryDataPoint.Vector.Coordinates);

                    var firstQueryFirstFoundDataPoint = foundQueryResults[0].Nearest[0];
                    Assert.AreEqual(11, firstQueryFirstFoundDataPoint.Id);
                    Assert.AreEqual("data_11", firstQueryFirstFoundDataPoint.Data);
                    Assert.That(firstQueryFirstFoundDataPoint.Vector.Coordinates, Is.EqualTo(vector1Nearest1.Coordinates).AsCollection.Within(singlePrecisionEpsilon));
                    Assert.AreEqual(0.11, firstQueryFirstFoundDataPoint.Distance, singlePrecisionEpsilon);

                    var firstQuerySecondFoundDataPoint = foundQueryResults[0].Nearest[1];
                    Assert.AreEqual(12, firstQuerySecondFoundDataPoint.Id);
                    Assert.AreEqual("data_12", firstQuerySecondFoundDataPoint.Data);
                    Assert.That(firstQuerySecondFoundDataPoint.Vector.Coordinates, Is.EqualTo(vector1Nearest2.Coordinates).AsCollection.Within(singlePrecisionEpsilon));
                    Assert.AreEqual(0.12, firstQuerySecondFoundDataPoint.Distance, singlePrecisionEpsilon);
                });
        }

        private double[] GetRandomCoordinates()
        {
            return new[] {random.NextDouble(), random.NextDouble()};
        }
    }
}

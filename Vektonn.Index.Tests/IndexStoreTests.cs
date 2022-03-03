using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using Vostok.Logging.Abstractions;

namespace Vektonn.Index.Tests
{
    public class IndexStoreTests
    {
        private const double SinglePrecisionEpsilon = 1e-06;

        private readonly Random random = new Random();

        private ILog log = null!;
        private IIndexIdMapping<int> idMapping = null!;
        private IIndexDataStorage<int, string> storage = null!;
        private IIndex<DenseVector> index = null!;

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
            var dataPointOrTombstones = new[]
            {
                new IndexDataPointOrTombstone<int, string, DenseVector>(new IndexDataPoint<int, string, DenseVector>(Id: 11, Data: "data_11", Vector: new DenseVector(GetRandomCoordinates()))),
                new IndexDataPointOrTombstone<int, string, DenseVector>(new IndexDataPoint<int, string, DenseVector>(Id: 12, Data: "data_12", Vector: new DenseVector(GetRandomCoordinates()))),
                new IndexDataPointOrTombstone<int, string, DenseVector>(new IndexDataPoint<int, string, DenseVector>(Id: 21, Data: "data_21", Vector: new DenseVector(GetRandomCoordinates()))),
                new IndexDataPointOrTombstone<int, string, DenseVector>(new IndexDataPoint<int, string, DenseVector>(Id: 22, Data: "data_22", Vector: new DenseVector(GetRandomCoordinates()))),

                new IndexDataPointOrTombstone<int, string, DenseVector>(new IndexTombstone<int>(Id: 31)),
                new IndexDataPointOrTombstone<int, string, DenseVector>(new IndexTombstone<int>(Id: 32)),
                new IndexDataPointOrTombstone<int, string, DenseVector>(new IndexTombstone<int>(Id: 41)),
                new IndexDataPointOrTombstone<int, string, DenseVector>(new IndexTombstone<int>(Id: 42)),
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
                dataPointOrTombstones[0].DataPoint!.Vector,
                dataPointOrTombstones[1].DataPoint!.Vector,
                dataPointOrTombstones[2].DataPoint!.Vector,
                dataPointOrTombstones[3].DataPoint!.Vector,
            };

            var indexStore = new IndexStore<int, string, DenseVector>(log, index, idMapping, storage, EqualityComparer<int>.Default);
            indexStore.UpdateIndex(dataPointOrTombstones);

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

            var addBatchSequence = new (long Id, DenseVector? Vector)[]
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
            var queryVector1 = new DenseVector(GetRandomCoordinates());
            var queryVector2 = new DenseVector(GetRandomCoordinates());
            var vector1Nearest1 = new DenseVector(GetRandomCoordinates());
            var vector1Nearest2 = new DenseVector(GetRandomCoordinates());

            var queryDataPoints = new[]
            {
                queryVector1,
                queryVector2,
            };

            A.CallTo(
                    () => index.FindNearest(
                        A<DenseVector[]>.That.IsSameSequenceAs(queryVector1, queryVector2),
                        2,
                        true))
                .Returns(
                    new[]
                    {
                        new (long Id, double Distance, DenseVector? Vector)[]
                        {
                            (Id: 1011, Distance: 0.11, vector1Nearest1),
                            (Id: 1012, Distance: 0.12, vector1Nearest2),
                        },
                        Array.Empty<(long Id, double Distance, DenseVector? Vector)>()
                    });

            A.CallTo(() => idMapping.GetIdByIndexId(1011)).Returns(11);
            A.CallTo(() => idMapping.GetIdByIndexId(1012)).Returns(12);
            A.CallTo(() => storage.Get(11)).Returns("data_11");
            A.CallTo(() => storage.Get(12)).Returns("data_12");

            var indexStore = new IndexStore<int, string, DenseVector>(log, index, idMapping, storage, EqualityComparer<int>.Default);
            var foundQueryResults = indexStore.FindNearest(queryDataPoints, limitPerQuery: 2, retrieveVectors: true).ToArray();

            Assert.Multiple(
                () =>
                {
                    Assert.AreEqual(2, foundQueryResults.Length);
                    Assert.AreEqual(2, foundQueryResults[0].NearestDataPoints.Length);
                    Assert.AreEqual(0, foundQueryResults[1].NearestDataPoints.Length);

                    CollectionAssert.AreEqual(queryVector1.Coordinates, foundQueryResults[0].QueryVector.Coordinates);
                    CollectionAssert.AreEqual(queryVector2.Coordinates, foundQueryResults[1].QueryVector.Coordinates);

                    var firstQueryFirstFoundDataPoint = foundQueryResults[0].NearestDataPoints[0];
                    Assert.AreEqual(11, firstQueryFirstFoundDataPoint.Id);
                    Assert.AreEqual("data_11", firstQueryFirstFoundDataPoint.Data);
                    Assert.That(firstQueryFirstFoundDataPoint.Vector!.Coordinates, Is.EqualTo(vector1Nearest1.Coordinates).AsCollection.Within(SinglePrecisionEpsilon));
                    Assert.AreEqual(0.11, firstQueryFirstFoundDataPoint.Distance, SinglePrecisionEpsilon);

                    var firstQuerySecondFoundDataPoint = foundQueryResults[0].NearestDataPoints[1];
                    Assert.AreEqual(12, firstQuerySecondFoundDataPoint.Id);
                    Assert.AreEqual("data_12", firstQuerySecondFoundDataPoint.Data);
                    Assert.That(firstQuerySecondFoundDataPoint.Vector!.Coordinates, Is.EqualTo(vector1Nearest2.Coordinates).AsCollection.Within(SinglePrecisionEpsilon));
                    Assert.AreEqual(0.12, firstQuerySecondFoundDataPoint.Distance, SinglePrecisionEpsilon);
                });
        }

        private double[] GetRandomCoordinates()
        {
            return new[] {random.NextDouble(), random.NextDouble()};
        }
    }
}

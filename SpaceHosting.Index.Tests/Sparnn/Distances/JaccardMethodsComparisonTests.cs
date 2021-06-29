using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MoreLinq.Extensions;
using NUnit.Framework;
using SpaceHosting.Index.Sparnn;
using SpaceHosting.Index.Sparnn.Distances;
using MSparseVector = MathNet.Numerics.LinearAlgebra.Double.SparseVector;

namespace SpaceHosting.Index.Tests.Sparnn.Distances
{
    [TestFixture]
    public class JaccardMethodsComparisonTests
    {
        private const int VectorSpaceSize = 15_000;
        private const int VectorSize = 15_000;
        private const int MinimumValuable = 5;
        private const int MaximumValuable = 15;
        private const int FeatureVectorsCount = 1;

        private const int indicesNumber = 2;
        private const int clusterSize = 1000;
        private static Random Random = null!;
        private SparnnIndex jaccardBinaryIndex = null!;
        private SparnnIndex jaccardBinarySingleOrientedIndex = null!;
        private SparseVector[] vectorsToSearch = null!;

        [SetUp]
        public void SetUp()
        {
            Random = new Random(42);
            jaccardBinaryIndex = new SparnnIndex(new TestMatrixMetricSearchSpaceFactory(newVersion: false), indicesNumber, clusterSize, VectorSize);
            jaccardBinarySingleOrientedIndex = new SparnnIndex(new TestMatrixMetricSearchSpaceFactory(newVersion: true), indicesNumber, clusterSize, VectorSize);

            var baseVectors = GenerateVectors(VectorSpaceSize).Select((x, i) => ((long)i, x));
            foreach (var batch in baseVectors.Batch(size: 1000, b => b.ToArray()))
            {
                jaccardBinaryIndex.AddBatch(batch);
                jaccardBinarySingleOrientedIndex.AddBatch(batch);
            }

            vectorsToSearch = GenerateVectors(FeatureVectorsCount).ToArray();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        public void FindNearestReturnsSameResult(int kNearestCount)
        {
            double AccumulateNearestVectorResultDistance(SparnnIndex sparnnIndex)
            {
                var searchResult = sparnnIndex.FindNearest(vectorsToSearch, kNearestCount);
                searchResult.Should().HaveCount(1);
                return searchResult[0].Select(x => x.Distance).Sum();
            }

            var firstMethodAccumulatedError = AccumulateNearestVectorResultDistance(jaccardBinaryIndex);
            var secondMethodAccumulatedError = AccumulateNearestVectorResultDistance(jaccardBinarySingleOrientedIndex);
            (firstMethodAccumulatedError - secondMethodAccumulatedError).Should().BeLessThan(double.Epsilon);
        }

        private static IEnumerable<Tuple<int, double>> GetIndexedValues(int count)
        {
            return Enumerable
                .Range(0, VectorSize)
                .RandomSubset(count, Random)
                .Select(i => new Tuple<int, double>(i, 1.0));
        }

        private static IEnumerable<SparseVector> GenerateVectors(int count)
        {
            return Enumerable.Range(0, count)
                .Select(_ => Random.Next(MinimumValuable, MaximumValuable))
                .Select(valuableCount => MSparseVector.OfIndexedEnumerable(VectorSize, GetIndexedValues(valuableCount)).ToModelVector());
        }

        private class TestMatrixMetricSearchSpaceFactory : IMatrixMetricSearchSpaceFactory
        {
            private readonly bool newVersion;

            public TestMatrixMetricSearchSpaceFactory(bool newVersion)
            {
                this.newVersion = newVersion;
            }

            public IMatrixMetricSearchSpace<TElement> Create<TElement>(IList<MSparseVector> featureVectors, TElement[] elements, int searchBatchSize)
            {
                return newVersion
                    ? new JaccardBinarySingleFeatureOrientedSpace<TElement>(featureVectors, elements)
                    : new JaccardBinaryDistanceSpace<TElement>(featureVectors, elements, searchBatchSize);
            }
        }
    }
}

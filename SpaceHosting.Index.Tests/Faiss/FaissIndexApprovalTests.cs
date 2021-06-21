using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ApprovalTests.Reporters;
using FluentAssertions;
using MathNet.Numerics;
using MoreLinq;
using NUnit.Framework;
using SpaceHosting.Index.Faiss;
using SpaceHosting.Index.Tests.Helpers;

namespace SpaceHosting.Index.Tests.Faiss
{
    [Category("RequiresNativeFaissLibrary")]
    [UseReporter(typeof(ApprovalTestsReporter))]
    public class FaissIndexApprovalTests
    {
        private const double SinglePrecisionEpsilon = 1e-06;

        private const string SampleVectorsFileName = "sample-dense-vectors.json";
        private const int ExpectedTotalVectorsCount = 11530;
        private const int ExpectedDistinctVectorsCount = 10996;
        private const int ExpectedScatteredVectorsCount = 10979;

        private readonly Random deterministicRandom = new(Seed: 0);

        [Test]
        public void SearchVectorsThemselves()
        {
            var vectors = ReadAllVectors();

            using var index = NewFaissIndex(vectors);

            foreach (var (_, vector) in vectors)
            {
                var nearest = index.FindNearest(new[] {vector}, limitPerQuery: 1).Single().Single();

                L2(nearest.Vector, vector).Should().BeApproximately(0.0, SinglePrecisionEpsilon);
                nearest.Distance.Should().BeApproximately(0.0, SinglePrecisionEpsilon);
            }
        }

        [Test]
        public void SearchVectorsThemselves_Scattered()
        {
            var scatteredVectors = GetScatteredVectors(ReadAllVectors());

            using var index = NewFaissIndex(scatteredVectors);

            foreach (var (id, vector) in scatteredVectors)
            {
                var (l, distance, denseVector) = index.FindNearest(new[] {vector}, limitPerQuery: 1).Single().Single();

                L2(denseVector, vector).Should().Be(0.0);
                distance.Should().Be(0.0);
                l.Should().Be(id);
                denseVector.Should().BeEquivalentTo(vector);
            }
        }

        [Test]
        public void SearchSomeNonExistingVectors()
        {
            var allVectors = ReadAllVectors();

            var sampleVectors = allVectors.RandomSubset(100, deterministicRandom).ToList();
            var sampleVectorsReordered = sampleVectors.Shuffle(deterministicRandom).ToList();
            var vectorsToSearch = sampleVectors.Zip(sampleVectorsReordered).Select(t => MidPoint(t.First.Vector, t.Second.Vector)).ToArray();
            vectorsToSearch.VerifyApprovalAsJson(nameof(vectorsToSearch));

            using var index = NewFaissIndex(allVectors);
            var searchResults = index.FindNearest(vectorsToSearch, limitPerQuery: 3);

            searchResults.VerifyApprovalAsJson(nameof(searchResults));
        }

        private static IList<(long Id, DenseVector Vector)> ReadAllVectors()
        {
            var json = File.ReadAllText(SampleVectorsFileName);

            var vectors = JsonSerializer.Deserialize<List<double[]>>(json)!;
            vectors.Count.Should().Be(ExpectedTotalVectorsCount);

            var vectorDimension = vectors.First().Length;
            foreach (var vector in vectors)
                vector.Length.Should().Be(vectorDimension);

            return vectors.Select((v, i) => (Id: (long)i, Vector: DenseVector(v))).ToArray();
        }

        private static IList<(long Id, DenseVector Vector)> GetScatteredVectors(IList<(long Id, DenseVector Vector)> vectors)
        {
            var distinctVectors = vectors.DistinctBy(t => string.Join("_", t.Vector.Coordinates)).ToArray();
            distinctVectors.Length.Should().Be(ExpectedDistinctVectorsCount);

            var idsToExclude = new HashSet<long>();
            for (var i = 0; i < distinctVectors.Length; i++)
            for (var j = i + 1; j < distinctVectors.Length; j++)
            {
                if (L2(distinctVectors[i].Vector, distinctVectors[j].Vector) < SinglePrecisionEpsilon)
                    idsToExclude.Add(distinctVectors[j].Id);
            }

            var scatteredVectors = distinctVectors.Where(t => !idsToExclude.Contains(t.Id)).ToArray();
            scatteredVectors.Length.Should().Be(ExpectedScatteredVectorsCount);

            for (var i = 0; i < scatteredVectors.Length; i++)
            for (var j = i + 1; j < scatteredVectors.Length; j++)
            {
                var l2 = L2(scatteredVectors[i].Vector, scatteredVectors[j].Vector);
                if (l2 < SinglePrecisionEpsilon)
                    throw new AssertionException($"L2={l2} for i={i}, j={j}");
            }

            return scatteredVectors;
        }

        private static FaissIndex NewFaissIndex(IList<(long Id, DenseVector Vector)> vectors)
        {
            var vectorDimension = vectors.First().Vector.Dimension;
            var index = new FaissIndex(Algorithms.FaissIndexTypeFlat, FaissMetricType.METRIC_L2, vectorDimension);

            foreach (var batch in vectors.Batch(size: 1000, b => b.ToArray()))
                index.AddBatch(batch);

            return index;
        }

        private static DenseVector DenseVector(double[] v) => new(v);

        private static double L2(DenseVector v1, DenseVector v2) => Distance.Euclidean(v1.Coordinates, v2.Coordinates);

        private static DenseVector MidPoint(DenseVector v1, DenseVector v2)
        {
            var resultCoordinates = new double[v1.Dimension];
            for (var i = 0; i < v1.Dimension; i++)
                resultCoordinates[i] = (v1.Coordinates[i] + v2.Coordinates[i]) / 2;

            return new DenseVector(resultCoordinates);
        }
    }
}

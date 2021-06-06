using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MoreLinq.Extensions;
using SpaceHosting.Index.Sparnn.Distances;

namespace SpaceHosting.Index.Benchmarks
{
    public class DistanceSpaceBenchmarks
    {
        private const int VectorSpaceSize = 15_000;
        private const int VectorSize = 15_000;
        private const int MinimumValuable = 5;
        private const int MaximumValuable = 15;

        [Params(100, 1000)]
        public int featureVectorsCount;

        private readonly int searchBatchSize = Math.Max((int)Math.Sqrt(VectorSpaceSize), 1000);
        private readonly int[] elements = Enumerable.Range(0, VectorSpaceSize).ToArray();

        private JaccardBinaryDistanceSpace<int> jaccard = null!;
        private CosineDistanceSpace<int> cosine = null!;
        private IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> vectorsToSearch = null!;

        [GlobalSetup]
        public void Setup()
        {
            var baseVectors = GenerateVectors(42, VectorSpaceSize).ToList();
            vectorsToSearch = GenerateVectors(420, featureVectorsCount).ToList();
            jaccard = new JaccardBinaryDistanceSpace<int>(baseVectors, elements, searchBatchSize);
            cosine = new CosineDistanceSpace<int>(baseVectors, elements, searchBatchSize);
        }

        [Benchmark]
        public void JaccardBinary() => jaccard.SearchNearestAsync(vectorsToSearch, 20).Result.Consume();

        [Benchmark]
        public void Cosine() => cosine.SearchNearestAsync(vectorsToSearch, 20).Result.Consume();

        private IEnumerable<MathNet.Numerics.LinearAlgebra.Double.SparseVector> GenerateVectors(int seed, int count)
        {
            var rnd = new Random(seed);
            return Enumerable.Range(0, count)
                .Select(
                    _ =>
                    {
                        var valuableCount = rnd.Next(MinimumValuable, MaximumValuable);
                        return MathNet.Numerics.LinearAlgebra.Double.SparseVector.OfIndexedEnumerable(VectorSize, GetIndexedValues(valuableCount, rnd));
                    });
        }

        private IEnumerable<Tuple<int, double>> GetIndexedValues(int count, Random rnd)
        {
            return Enumerable
                .Range(0, VectorSize)
                .RandomSubset(count)
                .Select(i => new Tuple<int, double>(i, rnd.NextDouble()));
        }
    }
}

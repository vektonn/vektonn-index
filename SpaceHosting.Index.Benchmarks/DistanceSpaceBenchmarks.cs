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
        public int FeatureVectorsCount;

        private readonly int searchBatchSize = Math.Max((int)Math.Sqrt(VectorSpaceSize), 1000);
        private readonly int[] elements = Enumerable.Range(0, VectorSpaceSize).ToArray();

        private JaccardBinaryDistanceSpace<int> jaccardBinaryDistanceSpace = null!;
        private CosineDistanceSpace<int> cosineDistanceSpace = null!;
        private JaccardBinarySingleFeatureOrientedSpace<int> jaccardSingleFeatureOrientedSpace = null!;
        private IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> vectorsToSearch = null!;

        [GlobalSetup]
        public void Setup()
        {
            var baseVectors = GenerateVectors(42, VectorSpaceSize).ToList();
            vectorsToSearch = GenerateVectors(420, FeatureVectorsCount).ToList();
            jaccardBinaryDistanceSpace = new JaccardBinaryDistanceSpace<int>(baseVectors, elements, searchBatchSize);
            cosineDistanceSpace = new CosineDistanceSpace<int>(baseVectors, elements, searchBatchSize);
            jaccardSingleFeatureOrientedSpace = new JaccardBinarySingleFeatureOrientedSpace<int>(baseVectors, elements, searchBatchSize);
        }


        [Benchmark]
        public void JaccardBinarySingleFeatureOriented() => jaccardSingleFeatureOrientedSpace.SearchNearestAsync(vectorsToSearch, 20).GetAwaiter().GetResult().Consume();

        [Benchmark]
        public void JaccardBinary() => jaccardBinaryDistanceSpace.SearchNearestAsync(vectorsToSearch, 20).GetAwaiter().GetResult().Consume();

        [Benchmark]
        public void Cosine() => cosineDistanceSpace.SearchNearestAsync(vectorsToSearch, 20).GetAwaiter().GetResult().Consume();

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

        private static IEnumerable<Tuple<int, double>> GetIndexedValues(int count, Random rnd)
        {
            return Enumerable
                .Range(0, VectorSize)
                .RandomSubset(count)
                .Select(i => new Tuple<int, double>(i, rnd.NextDouble()));
        }
    }
}

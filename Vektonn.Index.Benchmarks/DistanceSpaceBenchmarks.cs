using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MoreLinq.Extensions;
using Vektonn.Index.Sparnn.Distances;
using MSparseVector = MathNet.Numerics.LinearAlgebra.Double.SparseVector;

namespace Vektonn.Index.Benchmarks
{
    [AllStatisticsColumn]
    public class DistanceSpaceBenchmarks
    {
        private const int VectorSpaceSize = 15_000;
        private const int VectorSize = 15_000;
        private const int MinimumValuable = 5;
        private const int MaximumValuable = 15;

        [Params(1, 10, 100, 1000)]
        public int FeatureVectorsCount;

        [Params(1, 10, 50)]
        public int KnnCount;

        private readonly int searchBatchSize = Math.Max((int)Math.Sqrt(VectorSpaceSize), 1000);
        private readonly int[] elements = Enumerable.Range(0, VectorSpaceSize).ToArray();

        private JaccardBinaryDistanceSpace<int> jaccardBinaryDistanceSpace = null!;
        private CosineDistanceSpace<int> cosineDistanceSpace = null!;
        private JaccardBinarySingleFeatureOrientedSpace<int> jaccardSingleFeatureOrientedSpace = null!;
        private IList<MSparseVector> vectorsToSearch = null!;

        [GlobalSetup]
        public void Setup()
        {
            var baseVectors = GenerateVectors(42, VectorSpaceSize).ToList();
            vectorsToSearch = GenerateVectors(420, FeatureVectorsCount).ToList();
            jaccardBinaryDistanceSpace = new JaccardBinaryDistanceSpace<int>(baseVectors, elements, searchBatchSize);
            cosineDistanceSpace = new CosineDistanceSpace<int>(baseVectors, elements, searchBatchSize);
            jaccardSingleFeatureOrientedSpace = new JaccardBinarySingleFeatureOrientedSpace<int>(baseVectors, elements);
        }

        [Benchmark]
        public void JaccardBinarySingleFeatureOriented() => DoBenchmarkThenConsume(jaccardSingleFeatureOrientedSpace);

        [Benchmark]
        public void JaccardBinary() => DoBenchmarkThenConsume(jaccardBinaryDistanceSpace);

        [Benchmark]
        public void Cosine() => DoBenchmarkThenConsume(cosineDistanceSpace);

        private void DoBenchmarkThenConsume(IMatrixMetricSearchSpace<int> matrixMetricSearchSpace)
        {
            matrixMetricSearchSpace.SearchNearestAsync(vectorsToSearch, KnnCount).GetAwaiter().GetResult().Consume();
        }

        private static IEnumerable<MSparseVector> GenerateVectors(int seed, int count)
        {
            var rnd = new Random(seed);
            return Enumerable.Range(0, count)
                .Select(
                    _ =>
                    {
                        var valuableCount = rnd.Next(MinimumValuable, MaximumValuable);
                        return MSparseVector.OfIndexedEnumerable(VectorSize, GetIndexedValues(valuableCount));
                    });
        }

        private static IEnumerable<Tuple<int, double>> GetIndexedValues(int count)
        {
            return Enumerable
                .Range(0, VectorSize)
                .RandomSubset(count)
                .Select(i => new Tuple<int, double>(i, 1.0));
        }
    }
}

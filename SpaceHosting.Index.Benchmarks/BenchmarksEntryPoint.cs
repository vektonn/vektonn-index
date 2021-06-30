using System;
using BenchmarkDotNet.Running;

namespace SpaceHosting.Index.Benchmarks
{
    public static class BenchmarksEntryPoint
    {
        public static void Main()
        {
            var summary = BenchmarkRunner.Run<DistanceSpaceBenchmarks>();
            Console.WriteLine(summary.ToString());
        }
    }
}

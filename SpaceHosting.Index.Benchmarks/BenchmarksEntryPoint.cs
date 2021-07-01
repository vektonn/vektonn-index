using System;
using BenchmarkDotNet.Running;

namespace SpaceHosting.Index.Benchmarks
{
    public static class BenchmarksEntryPoint
    {
        public static void Main()
        {
            BenchmarkRunner.Run<SparnnIndexBenchmarks>();
            Console.ReadLine();
        }
    }
}

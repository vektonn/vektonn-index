using System;
using BenchmarkDotNet.Running;

namespace Vektonn.Index.Benchmarks
{
    public static class BenchmarksEntryPoint
    {
        public static void Main()
        {
            BenchmarkRunner.Run<IndexStoreMemoryUsageBenchmarks>();
            Console.ReadLine();
        }
    }
}

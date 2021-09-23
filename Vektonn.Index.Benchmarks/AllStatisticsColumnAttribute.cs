using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;

namespace Vektonn.Index.Benchmarks
{
    public class AllStatisticsColumnAttribute : ColumnConfigBaseAttribute
    {
        public AllStatisticsColumnAttribute()
            : base(
                StatisticColumn.Mean,
                StatisticColumn.StdErr,
                StatisticColumn.StdDev,
                StatisticColumn.Q3,
                StatisticColumn.P95,
                StatisticColumn.OperationsPerSecond)
        {
        }
    }
}

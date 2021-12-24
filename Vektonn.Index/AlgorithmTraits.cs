using System;
using System.ComponentModel;
using Vektonn.Index.Faiss;

namespace Vektonn.Index
{
    public static class AlgorithmTraits
    {
        public static bool VectorsAreSparse(string indexAlgorithm)
        {
            if (indexAlgorithm.StartsWith(Algorithms.SparnnIndex))
                return true;

            if (indexAlgorithm.StartsWith(Algorithms.FaissIndex))
                return false;

            throw new InvalidOperationException($"Invalid {nameof(indexAlgorithm)}: {indexAlgorithm}");
        }

        public static ListSortDirection GetMergeSortDirection(string indexAlgorithm)
        {
            if (indexAlgorithm.StartsWith(Algorithms.SparnnIndex))
                return ListSortDirection.Ascending;

            if (indexAlgorithm.StartsWith(Algorithms.FaissIndex))
            {
                var metricType = IndexParamsHelpers.TryGetFaissMetricType(indexAlgorithm);
                return metricType switch
                {
                    FaissMetricType.METRIC_L2 => ListSortDirection.Ascending,
                    FaissMetricType.METRIC_INNER_PRODUCT => ListSortDirection.Descending,
                    _ => throw new InvalidOperationException($"Invalid {nameof(indexAlgorithm)}: {indexAlgorithm}")
                };
            }

            throw new InvalidOperationException($"Invalid {nameof(indexAlgorithm)}: {indexAlgorithm}");
        }
    }
}

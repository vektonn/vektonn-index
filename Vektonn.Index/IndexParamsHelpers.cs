using System;
using System.Collections.Generic;
using System.Linq;
using Vektonn.Index.Faiss;

namespace Vektonn.Index
{
    internal static class IndexParamsHelpers
    {
        public static FaissMetricType? TryGetFaissMetricType(string indexAlgorithm)
        {
            var parts = indexAlgorithm.Split('.', StringSplitOptions.RemoveEmptyEntries);

            return parts.LastOrDefault() switch
            {
                "L2" => FaissMetricType.METRIC_L2,
                "IP" => FaissMetricType.METRIC_INNER_PRODUCT,
                _ => null
            };
        }

        public static HnswParams? TryGetHnswParams(IReadOnlyDictionary<string, string> indexParams)
        {
            var m = indexParams.TryGetIntValue(IndexParamsKeys.Hnsw.M);
            var efConstruction = indexParams.TryGetIntValue(IndexParamsKeys.Hnsw.EfConstruction);
            var efSearch = indexParams.TryGetIntValue(IndexParamsKeys.Hnsw.EfSearch);

            if (m == null && efConstruction == null && efSearch == null)
                return null;

            if (m == null || efConstruction == null || efSearch == null)
                throw new InvalidOperationException($"All HNSW params must be set: {IndexParamsKeys.Hnsw.M}, {IndexParamsKeys.Hnsw.EfConstruction}, {IndexParamsKeys.Hnsw.EfSearch}, ");

            return new HnswParams(m.Value, efConstruction.Value, efSearch.Value);
        }

        private static int? TryGetIntValue(this IReadOnlyDictionary<string, string> indexParams, string paramKey)
        {
            if (!indexParams.TryGetValue(paramKey, out var valueStr))
                return null;

            if (!int.TryParse(valueStr, out var paramValue))
                throw new InvalidOperationException($"Invalid value '{valueStr}' for param '{paramKey}'");

            return paramValue;
        }
    }
}

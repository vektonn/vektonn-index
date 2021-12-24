using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.Index.Faiss;

namespace Vektonn.Index.Tests
{
    internal class IndexParamsHelpersTests
    {
        [TestCase("FaissIndex.L2", FaissMetricType.METRIC_L2)]
        [TestCase("FaissIndex.IP", FaissMetricType.METRIC_INNER_PRODUCT)]
        [TestCase("SparnnIndex.Cosine", null)]
        [TestCase("SparnnIndex.JaccardBinary", null)]
        public void TryGetFaissMetricType(string indexAlgorithm, FaissMetricType? expectedMetricType)
        {
            IndexParamsHelpers.TryGetFaissMetricType(indexAlgorithm).Should().Be(expectedMetricType);
        }

        [Test]
        public void TryGetHnswParams()
        {
            var indexParams = new Dictionary<string, string>();
            IndexParamsHelpers.TryGetHnswParams(indexParams).Should().Be(null);

            indexParams[IndexParamsKeys.Hnsw.M] = "16";

            Action action = () => IndexParamsHelpers.TryGetHnswParams(indexParams);
            action.Should().Throw<InvalidOperationException>();

            indexParams[IndexParamsKeys.Hnsw.EfConstruction] = "500";
            indexParams[IndexParamsKeys.Hnsw.EfSearch] = "100";

            IndexParamsHelpers.TryGetHnswParams(indexParams).Should().Be(new HnswParams(16, 500, 100));
        }
    }
}

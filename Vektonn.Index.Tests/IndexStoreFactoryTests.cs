using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Logging.Abstractions;

namespace Vektonn.Index.Tests
{
    public class IndexStoreFactoryTests
    {
        private static readonly string[] FaissIndexAlgorithms =
        {
            Algorithms.FaissIndexIP,
            Algorithms.FaissIndexL2,
        };

        private static readonly string[] SparnnIndexAlgorithms =
        {
            Algorithms.SparnnIndexCosine,
            Algorithms.SparnnIndexJaccardBinary,
        };

        private readonly IndexStoreFactory<int, string> indexStoreFactory = new(new SilentLog());

        [Category("RequiresNativeFaissLibrary")]
        [TestCaseSource(nameof(FaissIndexAlgorithms))]
        public void Create_FaissIndex(string algorithm)
        {
            var indexStore = indexStoreFactory.Create<DenseVector>(
                algorithm,
                vectorDimension: 42,
                withDataStorage: true,
                EqualityComparer<int>.Default);

            indexStore.Should().BeOfType<IndexStore<int, string, DenseVector>>();
        }

        [TestCaseSource(nameof(SparnnIndexAlgorithms))]
        public void Create_SparnnIndex(string algorithm)
        {
            var indexStore = indexStoreFactory.Create<SparseVector>(
                algorithm,
                vectorDimension: 42,
                withDataStorage: false,
                EqualityComparer<int>.Default);

            indexStore.Should().BeOfType<IndexStore<int, string, SparseVector>>();
        }
    }
}

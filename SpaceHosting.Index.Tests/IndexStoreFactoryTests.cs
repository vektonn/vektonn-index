using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Logging.Abstractions;

namespace SpaceHosting.Index.Tests
{
    public class IndexStoreFactoryTests
    {
        private readonly IndexStoreFactory<int, string> indexStoreFactory = new IndexStoreFactory<int, string>(new SilentLog());

        [Test]
        [Category("RequiresNativeFaissLibrary")]
        public void Create_FaissIndex()
        {
            var indexStore = indexStoreFactory.Create<DenseVector>(
                Algorithms.FaissIndexFlatIP,
                vectorDimension: 42,
                withDataStorage: true,
                EqualityComparer<int>.Default);

            indexStore.Should().BeOfType<IndexStore<int, string, DenseVector>>();
        }

        [Test]
        public void Create_SparnnIndex()
        {
            var indexStore = indexStoreFactory.Create<SparseVector>(
                Algorithms.SparnnIndexJaccardBinary,
                vectorDimension: 42,
                withDataStorage: false,
                EqualityComparer<int>.Default);

            indexStore.Should().BeOfType<IndexStore<int, string, SparseVector>>();
        }
    }
}

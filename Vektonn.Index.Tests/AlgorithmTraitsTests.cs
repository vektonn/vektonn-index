using System.ComponentModel;
using FluentAssertions;
using NUnit.Framework;

namespace Vektonn.Index.Tests
{
    public class AlgorithmTraitsTests
    {
        [TestCase("FaissIndex.L2", false)]
        [TestCase("FaissIndex.IP", false)]
        [TestCase("SparnnIndex.Cosine", true)]
        [TestCase("SparnnIndex.JaccardBinary", true)]
        public void VectorsAreSparse(string indexAlgorithm, bool expectedSparse)
        {
            AlgorithmTraits.VectorsAreSparse(indexAlgorithm).Should().Be(expectedSparse);
        }

        [TestCase("FaissIndex.L2", ListSortDirection.Ascending)]
        [TestCase("FaissIndex.IP", ListSortDirection.Descending)]
        [TestCase("SparnnIndex.Cosine", ListSortDirection.Ascending)]
        [TestCase("SparnnIndex.JaccardBinary", ListSortDirection.Ascending)]
        public void GetMergeSortDirection(string indexAlgorithm, ListSortDirection expectedSortDirection)
        {
            AlgorithmTraits.GetMergeSortDirection(indexAlgorithm).Should().Be(expectedSortDirection);
        }
    }
}

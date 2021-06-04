using System;
using NUnit.Framework;
using SpaceHosting.Index.Sparnn.Helpers;

namespace SpaceHosting.Index.Tests.Sparnn.Helpers.ListExtensionsTests
{
    [TestFixture]
    public class TakeKBestTests
    {
        [TestCase(-1)]
        [TestCase(0)]
        public void ThrowsWhenKIsIncorrect(int k)
        {
            var array = new[] {1, 2, 3};

            Assert.Throws<ArgumentException>(() => array.TakeKBest(k, e => e));
        }

        [TestCase(4)]
        [TestCase(40)]
        public void KMoreThanCount_ReturnsLengthCount(int k)
        {
            var array = new[] {2, 1, 3};

            var result = array.TakeKBest(k, e => e);

            Assert.That(result.Length, Is.EqualTo(array.Length));
            Assert.That(result, Is.EqualTo(new[] {1, 2, 3}));
        }

        [TestCase(1)]
        [TestCase(2)]
        public void ReturnsRequestedCount(int k)
        {
            var array = new[] {2, 1, 3};

            var result = array.TakeKBest(k, e => e);

            Assert.That(result.Length, Is.EqualTo(k));
        }

        [TestCase(1, new[] {2, 1, 3, 0}, new[] {0})]
        [TestCase(2, new[] {2, 1, 3, 0}, new[] {0, 1})]
        [TestCase(3, new[] {2, 1, 3, 0}, new[] {0, 1, 2})]
        [TestCase(2, new[] {0, 1, 3, 0}, new[] {0, 0})]
        [TestCase(2, new[] {0, 1, 2, 3}, new[] {0, 1})]
        public void ReturnsTheBestElements(int k, int[] array, int[] expectedResult)
        {
            var result = array.TakeKBest(k, e => e);

            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}

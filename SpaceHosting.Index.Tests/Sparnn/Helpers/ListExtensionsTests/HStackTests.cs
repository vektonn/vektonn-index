using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SpaceHosting.Index.Sparnn.Helpers;

namespace SpaceHosting.Index.Tests.Sparnn.Helpers.ListExtensionsTests
{
    [TestFixture]
    public class HStackTests
    {
        [Test]
        public void SuccessfulCase()
        {
            var matrix1 = (IEnumerable<int[]>)new[]
            {
                new[] {11, 12, 13},
                new[] {21, 22, 23},
            };

            var matrix2 = (IEnumerable<int[]>)new[]
            {
                new[] {14, 15, 16},
                new[] {24, 25},
            };

            var result = new[] {matrix1, matrix2}.HStack().ToArray();

            Assert.That(result[0], Is.EquivalentTo(new[] {11, 12, 13, 14, 15, 16}));
            Assert.That(result[1], Is.EquivalentTo(new[] {21, 22, 23, 24, 25}));
        }

        [Test]
        public void DifferentRowsNumber_Throws()
        {
            var matrix1 = (IEnumerable<int[]>)new[]
            {
                new[] {11, 12, 13},
                new[] {21, 22, 23},
            };

            var matrix2 = (IEnumerable<int[]>)new[]
            {
                new[] {14, 15, 16},
                new[] {24, 25},
                new[] {34, 35},
            };

            Assert.Throws<Exception>(() => new[] {matrix1, matrix2}.HStack().ToArray());
        }

        [Test]
        public void Success_if_rows_more_than_columns()
        {
            var matrix1 = (IEnumerable<int[]>)new[]
            {
                new[] {11, 12},
                new[] {21, 22},
                new[] {31, 32},
                new[] {41, 42},
            };

            var matrix2 = (IEnumerable<int[]>)new[]
            {
                new[] {13, 14},
                new[] {23, 24},
                new[] {33, 34},
                new[] {43, 44},
            };

            var result = new[] {matrix1, matrix2}.HStack().ToArray();

            Assert.That(result[0], Is.EquivalentTo(new[] {11, 12, 13, 14}));
            Assert.That(result[1], Is.EquivalentTo(new[] {21, 22, 23, 24}));
            Assert.That(result[2], Is.EquivalentTo(new[] {31, 32, 33, 34}));
            Assert.That(result[3], Is.EquivalentTo(new[] {41, 42, 43, 44}));
        }

        [Test]
        public void Returns_requested_count_of_rows()
        {
            var matrix1 = (IEnumerable<int[]>)new[]
            {
                new[] {11, 12},
                new[] {21, 22},
                new[] {31, 32},
                new[] {41, 42},
            };

            var matrix2 = (IEnumerable<int[]>)new[]
            {
                new[] {13, 14},
                new[] {23, 24},
                new[] {33, 34},
                new[] {43, 44},
            };

            var result = new[] {matrix1, matrix2}.HStack().ToArray();

            Assert.That(result.Length, Is.EqualTo(4));
        }
    }
}

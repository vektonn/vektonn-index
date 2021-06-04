using System;
using System.Linq;

namespace SpaceHosting.Index
{
    public sealed class SparseVector : IVector
    {
        public SparseVector(int dimension, int[] columnIndices, double[] coordinates)
        {
            if (coordinates.Length != columnIndices.Length)
                throw new ArgumentException("Can't create sparse vector. Column indices count does not matches values count");

            if (columnIndices.Any(i => i >= dimension))
                throw new ArgumentException("Can't create sparse vector. Column index is more than vector dimension");

            if (columnIndices.Distinct().Count() != columnIndices.Length)
                throw new ArgumentException("Can't create sparse vector. Column indices have duplicates");

            Dimension = dimension;
            ColumnIndices = columnIndices;
            Coordinates = coordinates;
        }

        public int Dimension { get; }
        public int[] ColumnIndices { get; }
        public double[] Coordinates { get; }
    }
}

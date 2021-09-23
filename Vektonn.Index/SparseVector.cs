using System;
using System.Linq;

namespace Vektonn.Index
{
    public sealed class SparseVector : IVector
    {
        public SparseVector(int dimension, double[] coordinates, int[] coordinateIndices)
        {
            if (coordinates.Length != coordinateIndices.Length)
                throw new ArgumentException("Can't create sparse vector. Coordinate indices count does not match Coordinates count");

            if (coordinateIndices.Any(i => i >= dimension))
                throw new ArgumentException("Can't create sparse vector. Coordinate index is more than vector dimension");

            if (coordinateIndices.Distinct().Count() != coordinateIndices.Length)
                throw new ArgumentException("Can't create sparse vector. Coordinate indices have duplicates");

            Dimension = dimension;
            Coordinates = coordinates;
            CoordinateIndices = coordinateIndices;
        }

        public int Dimension { get; }
        public double[] Coordinates { get; }
        public int[] CoordinateIndices { get; }
    }
}

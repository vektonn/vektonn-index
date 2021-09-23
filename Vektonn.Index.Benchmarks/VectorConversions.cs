using System.Linq;

namespace Vektonn.Index.Benchmarks
{
    public static class VectorConversions
    {
        public static DenseVector ToDenseVector(this double?[] coordinates)
        {
            if (coordinates.All(c => c == null))
                return new DenseVector(new double[coordinates.Length]);

            return new DenseVector(coordinates.Cast<double>().ToArray());
        }

        public static SparseVector ToSparseVector(this double?[] coordinates)
        {
            var dimension = coordinates.Length;
            var nonNullCount = coordinates.Count(c => c != null);
            var indices = new int[nonNullCount];
            var coords = new double[nonNullCount];

            var j = 0;
            for (var i = 0; i < dimension; i++)
            {
                var coordinate = coordinates[i];
                if (coordinate == null)
                    continue;

                indices[j] = i;
                coords[j] = coordinate.Value;
                j++;
            }

            return new SparseVector(dimension, coords, indices);
        }
    }
}

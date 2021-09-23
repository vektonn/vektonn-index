using System.Linq;
using MathNetVector = MathNet.Numerics.LinearAlgebra.Double.SparseVector;
using MathNetVectorStorage = MathNet.Numerics.LinearAlgebra.Storage.SparseVectorStorage<double>;

namespace Vektonn.Index.Sparnn
{
    internal static class SparseVectorExtensions
    {
        public static MathNetVector[] ToIndexVectors(this SparseVector[] inputVectors)
        {
            return inputVectors.Select(ToIndexVector).ToArray();
        }

        public static MathNetVector ToIndexVector(this SparseVector inputVector)
        {
            var dimension = inputVector.Dimension;
            var vectorStorage = MathNetVectorStorage.OfValue(dimension, 0);
            vectorStorage.ValueCount = inputVector.Coordinates.Length;
            vectorStorage.Values = inputVector.Coordinates;
            vectorStorage.Indices = inputVector.CoordinateIndices;

            return new MathNetVector(vectorStorage);
        }

        public static SparseVector ToModelVector(this MathNetVector vector)
        {
            var vectorStorage = (MathNetVectorStorage)vector.Storage;
            return new SparseVector(dimension: vector.Count, vectorStorage.Values, vectorStorage.Indices);
        }
    }
}

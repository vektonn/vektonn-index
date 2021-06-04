using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using SpaceHosting.Index.Sparnn.Helpers;

namespace SpaceHosting.Index.Sparnn.Distances
{
    internal class SparseVectorsList
    {
        public SparseVectorsList(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> vectors)
        {
            Vectors = vectors;
        }

        public IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> Vectors { get; }

        public MathNet.Numerics.LinearAlgebra.Double.SparseVector this[int index] => Vectors[index];

        public static SparseMatrix operator*(SparseVectorsList one, SparseMatrix another)
        {
            return SparseMatrixExtensions.CreateFromVectors(one.Vectors) * another;
        }
    }
}

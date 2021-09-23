using System.Collections;
using System.Collections.Generic;

namespace Vektonn.Index.Benchmarks
{
    public class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
        public static readonly ArrayEqualityComparer<T> Instance = new ArrayEqualityComparer<T>();

        public bool Equals(T[]? x, T[]? y)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
        }

        public int GetHashCode(T[] x)
        {
            return StructuralComparisons.StructuralEqualityComparer.GetHashCode(x);
        }
    }
}

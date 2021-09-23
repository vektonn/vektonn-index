using System;

namespace Vektonn.Index.Sparnn.Distances
{
    internal class NearestSearchResult<TElement> : IComparable<NearestSearchResult<TElement>>
    {
        public NearestSearchResult(double distance, TElement element, MathNet.Numerics.LinearAlgebra.Double.SparseVector vector)
        {
            Distance = distance;
            Element = element;
            Vector = vector;
        }

        public double Distance { get; }
        public TElement Element { get; }
        public MathNet.Numerics.LinearAlgebra.Double.SparseVector Vector { get; }

        public int CompareTo(NearestSearchResult<TElement>? other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (ReferenceEquals(null, other))
                return 1;

            return Distance.CompareTo(other.Distance);
        }
    }
}

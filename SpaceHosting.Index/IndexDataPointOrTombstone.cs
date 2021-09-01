using System;
using System.Diagnostics.CodeAnalysis;

namespace SpaceHosting.Index
{
    public record IndexDataPointOrTombstone<TId, TData, TVector>(IndexDataPoint<TId, TData, TVector>? DataPoint, IndexTombstone<TId>? Tombstone)
        where TId : notnull
        where TVector : IVector
    {
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global")]
        public IndexDataPointOrTombstone(IndexDataPoint<TId, TData, TVector> dataPoint)
            : this(dataPoint, Tombstone: null)
        {
        }

        public IndexDataPointOrTombstone(IndexTombstone<TId> tombstone)
            : this(DataPoint: null, tombstone)
        {
        }

        public TId GetId()
        {
            if (DataPoint != null)
                return DataPoint.Id;

            if (Tombstone != null)
                return Tombstone.Id;

            throw new InvalidOperationException($"{nameof(DataPoint)} == null && {nameof(Tombstone)} == null");
        }
    }
}

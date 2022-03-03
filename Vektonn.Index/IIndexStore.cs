using System;
using System.Collections.Generic;

namespace Vektonn.Index
{
    public interface IIndexStore<TId, TData, TVector> : IDisposable
        where TId : notnull
        where TVector : IVector
    {
        long Count { get; }

        void UpdateIndex(IndexDataPointOrTombstone<TId, TData, TVector>[] dataPointOrTombstones);

        IReadOnlyList<IndexSearchResultItem<TId, TData, TVector>> FindNearest(TVector[] queryVectors, int limitPerQuery, bool retrieveVectors);
    }
}

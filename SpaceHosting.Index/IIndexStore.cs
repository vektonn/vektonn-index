using System;
using System.Collections.Generic;

namespace SpaceHosting.Index
{
    public interface IIndexStore<TId, TData, TVector> : IDisposable
        where TId : notnull
        where TVector : IVector
    {
        long Count { get; }
        void AddBatch(IndexDataPoint<TId, TData, TVector>[] dataPoints);
        IReadOnlyList<IndexQueryResult<TId, TData, TVector>> FindNearest(IndexQueryDataPoint<TVector>[] queryDataPoints, int limitPerQuery);
    }
}

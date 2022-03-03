using System;
using System.Collections.Generic;

namespace Vektonn.Index
{
    internal interface IIndex<TVector> : IDisposable
        where TVector : IVector
    {
        string Description { get; }

        int VectorCount { get; }

        void AddBatch((long Id, TVector Vector)[] data);

        long DeleteBatch(long[] ids);

        IReadOnlyList<IReadOnlyList<(long Id, double Distance, TVector? Vector)>> FindNearest(TVector[] queryVectors, int limitPerQuery, bool retrieveVectors);
    }
}

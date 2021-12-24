using System.Collections.Generic;

namespace Vektonn.Index
{
    public interface IIndexStoreFactory<TId, TData>
        where TId : notnull
    {
        IIndexStore<TId, TData, TVector> Create<TVector>(
            string algorithm,
            int vectorDimension,
            bool withDataStorage,
            IEqualityComparer<TId> idComparer,
            Dictionary<string, string>? indexParams = null)
            where TVector : IVector;
    }
}

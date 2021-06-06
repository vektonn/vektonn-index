using System.Collections.Generic;

namespace SpaceHosting.Index
{
    public interface IIndexStoreFactory<TId, TData>
        where TId : notnull
    {
        IIndexStore<TId, TData, TVector> Create<TVector>(
            string algorithm,
            int vectorDimension,
            bool withDataStorage,
            IEqualityComparer<TId> idComparer)
            where TVector : IVector;
    }
}

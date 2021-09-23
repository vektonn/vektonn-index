using System.Collections.Generic;

namespace Vektonn.Index
{
    internal class IndexDataStorage<TId, TData> : IIndexDataStorage<TId, TData>
        where TId : notnull
    {
        private readonly Dictionary<TId, TData?> storage;

        public IndexDataStorage(IEqualityComparer<TId> idComparer)
        {
            storage = new Dictionary<TId, TData?>(idComparer);
        }

        public int Count => storage.Count;

        public TData? Get(TId id)
        {
            return storage[id];
        }

        public void Add(TId id, TData? data)
        {
            storage.Add(id, data);
        }

        public void Update(TId id, TData? data)
        {
            storage[id] = data;
        }

        public void Delete(TId id)
        {
            storage.Remove(id);
        }
    }
}

using System.Collections.Generic;

namespace SpaceHosting.Index
{
    internal class IndexDataStorage<TId, TData> : IIndexDataStorage<TId, TData>
    {
        private readonly Dictionary<TId, TData> storage;

        public IndexDataStorage(IEqualityComparer<TId> idComparer)
        {
            storage = new Dictionary<TId, TData>(idComparer); // todo: test over 2GB
        }

        public int Count => storage.Count;

        public TData Get(TId id)
        {
            return storage[id];
        }

        public void Add(TId id, TData data)
        {
            storage.Add(id, data);
        }

        public void Update(TId id, TData data)
        {
            storage[id] = data;
        }

        public void Delete(TId id)
        {
            storage.Remove(id);
        }
    }
}

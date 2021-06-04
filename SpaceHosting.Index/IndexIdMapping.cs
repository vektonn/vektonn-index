using System.Collections.Generic;

namespace SpaceHosting.Index
{
    internal class IndexIdMapping<TId> : IIndexIdMapping<TId>
    {
        private readonly Dictionary<TId, long> idToIndexId;
        private readonly Dictionary<long, TId> indexIdToId;
        private long nextIndexId;

        public IndexIdMapping(IEqualityComparer<TId> idComparer)
        {
            idToIndexId = new Dictionary<TId, long>(idComparer);
            indexIdToId = new Dictionary<long, TId>();
        }

        public int Count => idToIndexId.Count;

        public long? FindIndexIdById(TId id)
        {
            return idToIndexId.TryGetValue(id, out var index) ? index : null;
        }

        public TId GetIdByIndexId(long indexId)
        {
            return indexIdToId[indexId];
        }

        public long Add(TId id)
        {
            var indexId = nextIndexId++;

            idToIndexId.Add(id, indexId);
            indexIdToId.Add(indexId, id);

            return indexId;
        }

        public void Delete(TId id)
        {
            var indexId = idToIndexId[id];
            idToIndexId.Remove(id);
            indexIdToId.Remove(indexId);
        }
    }
}

namespace Vektonn.Index
{
    internal class EmptyIndexDataStorage<TId, TData> : IIndexDataStorage<TId, TData>
        where TId : notnull
    {
        public int Count => 0;

        public TData? Get(TId id)
        {
            return default;
        }

        public void Add(TId id, TData? data)
        {
        }

        public void Update(TId id, TData? data)
        {
        }

        public void Delete(TId id)
        {
        }
    }
}

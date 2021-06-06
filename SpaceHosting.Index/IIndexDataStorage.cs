namespace SpaceHosting.Index
{
    internal interface IIndexDataStorage<in TId, TData>
        where TId : notnull
    {
        int Count { get; }
        TData? Get(TId id);
        void Add(TId id, TData? data);
        void Update(TId id, TData? data);
        void Delete(TId id);
    }
}

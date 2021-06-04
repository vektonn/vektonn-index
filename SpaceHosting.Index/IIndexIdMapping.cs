namespace SpaceHosting.Index
{
    internal interface IIndexIdMapping<TId>
    {
        int Count { get; }
        long? FindIndexIdById(TId id);
        TId GetIdByIndexId(long indexId);
        long Add(TId id);
        void Delete(TId id);
    }
}

namespace SpaceHosting.Index
{
    internal interface IIndexIdMapping<TId>
        where TId : notnull
    {
        int Count { get; }
        long? FindIndexIdById(TId id);
        TId GetIdByIndexId(long indexId);
        long Add(TId id);
        void Delete(TId id);
    }
}

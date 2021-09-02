namespace SpaceHosting.Index
{
    public record IndexSearchResultItem<TId, TData, TVector>(TVector QueryVector, IndexFoundDataPoint<TId, TData, TVector>[] NearestDataPoints)
        where TId : notnull
        where TVector : IVector;
}

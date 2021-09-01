namespace SpaceHosting.Index
{
    public record IndexFoundDataPoint<TId, TData, TVector>(TId Id, TData? Data, TVector Vector, double Distance)
        : IndexDataPoint<TId, TData, TVector>(Id, Data, Vector)
        where TId : notnull
        where TVector : IVector;
}

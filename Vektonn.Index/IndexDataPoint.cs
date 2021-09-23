namespace Vektonn.Index
{
    public record IndexDataPoint<TId, TData, TVector>(TId Id, TData? Data, TVector Vector)
        where TId : notnull
        where TVector : IVector;
}

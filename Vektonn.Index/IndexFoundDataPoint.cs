namespace Vektonn.Index
{
    public record IndexFoundDataPoint<TId, TData, TVector>(TId Id, TData? Data, TVector? Vector, double Distance)
        where TId : notnull
        where TVector : IVector;
}

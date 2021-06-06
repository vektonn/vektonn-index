namespace SpaceHosting.Index
{
    public class IndexFoundDataPoint<TId, TData, TVector> : IndexDataPoint<TId, TData, TVector>
        where TId : notnull
        where TVector : IVector
    {
        public double Distance { get; set; }
    }
}

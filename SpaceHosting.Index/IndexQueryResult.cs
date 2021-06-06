namespace SpaceHosting.Index
{
    public class IndexQueryResult<TId, TData, TVector>
        where TId : notnull
        where TVector : IVector
    {
        public IndexQueryDataPoint<TVector> QueryDataPoint { get; set; } = default!;
        public IndexFoundDataPoint<TId, TData, TVector>[] Nearest { get; set; } = default!;
    }
}

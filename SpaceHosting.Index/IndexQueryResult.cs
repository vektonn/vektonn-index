namespace SpaceHosting.Index
{
    public class IndexQueryResult<TId, TData, TVector>
        where TVector : IVector
    {
        public IndexQueryDataPoint<TVector> QueryDataPoint { get; set; }
        public IndexFoundDataPoint<TId, TData, TVector>[] Nearest { get; set; }
    }
}

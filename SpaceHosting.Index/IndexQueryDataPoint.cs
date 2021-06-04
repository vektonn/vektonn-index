namespace SpaceHosting.Index
{
    public class IndexQueryDataPoint<TVector>
        where TVector : IVector
    {
        public TVector Vector { get; set; }
    }
}

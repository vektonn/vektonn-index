namespace SpaceHosting.Index
{
    public class IndexDataPoint<TId, TData, TVector>
        where TVector : IVector
    {
        public TId Id { get; set; }
        public TData Data { get; set; }
        public bool IsDeleted { get; set; }
        public TVector Vector { get; set; }
    }
}

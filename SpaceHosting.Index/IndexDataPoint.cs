namespace SpaceHosting.Index
{
    public class IndexDataPoint<TId, TData, TVector>
        where TId : notnull
        where TVector : IVector
    {
        public TId Id { get; set; } = default!;
        public TData? Data { get; set; }
        public bool IsDeleted { get; set; }
        public TVector Vector { get; set; } = default!;
    }
}

namespace Vektonn.Index
{
    public sealed class DenseVector : IVector
    {
        public DenseVector(double[] coordinates)
        {
            Coordinates = coordinates;
        }

        public double[] Coordinates { get; }

        public int Dimension => Coordinates.Length;
    }
}

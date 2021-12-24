namespace Vektonn.Index
{
    public static class Algorithms
    {
        public const string FaissIndex = "FaissIndex";
        public const string SparnnIndex = "SparnnIndex";

        public static readonly string FaissIndexL2 = $"{FaissIndex}.L2";
        public static readonly string FaissIndexIP = $"{FaissIndex}.IP";

        public static readonly string SparnnIndexCosine = $"{SparnnIndex}.Cosine";
        public static readonly string SparnnIndexJaccardBinary = $"{SparnnIndex}.JaccardBinary";
    }
}

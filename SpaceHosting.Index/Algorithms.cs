namespace SpaceHosting.Index
{
    public static class Algorithms
    {
        public const string FaissIndex = "FaissIndex";
        public const string FaissIndexTypeFlat = "Flat";

        public const string SparnnIndex = "SparnnIndex";

        public static readonly string FaissIndexFlatL2 = $"{FaissIndex}.{FaissIndexTypeFlat}.L2";
        public static readonly string FaissIndexFlatIP = $"{FaissIndex}.{FaissIndexTypeFlat}.IP";

        public static readonly string SparnnIndexCosine = $"{SparnnIndex}.Cosine";
        public static readonly string SparnnIndexJaccardBinary = $"{SparnnIndex}.JaccardBinary";
    }
}

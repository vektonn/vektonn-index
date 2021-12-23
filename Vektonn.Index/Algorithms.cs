namespace Vektonn.Index
{
    // todo (andrew, 23.12.2021): support index hyper parameters customization (e.g. for HNSW index there are M, efConstruction, and efSearch params)
    public static class Algorithms
    {
        public const string FaissIndex = "FaissIndex";
        public const string FaissIndexTypeFlat = "Flat";
        public const string FaissIndexTypeHnswFlat = "HNSW16,Flat";

        public const string SparnnIndex = "SparnnIndex";

        public static readonly string FaissIndexFlatL2 = $"{FaissIndex}.{FaissIndexTypeFlat}.L2";
        public static readonly string FaissIndexFlatIP = $"{FaissIndex}.{FaissIndexTypeFlat}.IP";
        public static readonly string FaissIndexHnswFlatL2 = $"{FaissIndex}.{FaissIndexTypeHnswFlat}.L2";
        public static readonly string FaissIndexHnswFlatIP = $"{FaissIndex}.{FaissIndexTypeHnswFlat}.IP";

        public static readonly string SparnnIndexCosine = $"{SparnnIndex}.Cosine";
        public static readonly string SparnnIndexJaccardBinary = $"{SparnnIndex}.JaccardBinary";
    }
}

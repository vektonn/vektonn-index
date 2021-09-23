using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Vektonn.Index.Benchmarks
{
    public static class VectorFile
    {
        public static object[] ReadMetadata(string metadataFileName, int vectorCount)
        {
            var json = File.ReadAllText(metadataFileName);
            var metadata = JsonSerializer.Deserialize<object[]>(json)!;

            if (metadata.Length != vectorCount)
                throw new InvalidOperationException("All vectors must have corresponding metadata");

            return metadata;
        }

        public static SparseVector[] ReadSparseVectors(string vectorsFileName, VectorsFileFormat vectorsFileFormat)
        {
            var vectors = vectorsFileFormat switch
            {
                VectorsFileFormat.DenseVectorArrayJson => ReadDenseVectorArrayFile(vectorsFileName).Select(VectorConversions.ToSparseVector).ToArray(),
                VectorsFileFormat.SparseVectorArrayJson => ReadSparseVectorArrayFile(vectorsFileName).Select(v => new SparseVector(v.dimension, v.coordinates, v.coordinateIndices)).ToArray(),
                VectorsFileFormat.PandasDataFrameCsv => ReadPandasDataFrameCsvFile(vectorsFileName).Select(VectorConversions.ToSparseVector).ToArray(),
                VectorsFileFormat.PandasDataFrameJson => ReadPandasDataFrameJsonFile(vectorsFileName).Select(VectorConversions.ToSparseVector).ToArray(),
                _ => throw new ArgumentException($"Invalid vectorsFileFormat: {vectorsFileFormat}")
            };

            var vectorDimension = vectors.First().Dimension;
            if (vectors.Any(vector => vector.Dimension != vectorDimension))
                throw new InvalidOperationException("All vectors must have the same dimension");

            return vectors;
        }

        private static List<double?[]> ReadDenseVectorArrayFile(string vectorsFileName)
        {
            var json = File.ReadAllText(vectorsFileName);
            return JsonSerializer.Deserialize<List<double?[]>>(json)!;
        }

        private static SparseVectorDto[] ReadSparseVectorArrayFile(string vectorsFileName)
        {
            var json = File.ReadAllText(vectorsFileName);
            return JsonSerializer.Deserialize<SparseVectorDto[]>(json)!;
        }

        private static List<double?[]> ReadPandasDataFrameCsvFile(string vectorsFileName)
        {
            return File
                .ReadAllLines(vectorsFileName)
                .Select(line => line.Split(',').Select(x => string.IsNullOrEmpty(x) ? (double?)null : double.Parse(x, CultureInfo.InvariantCulture)).ToArray())
                .ToList();
        }

        private static List<double?[]> ReadPandasDataFrameJsonFile(string vectorsFileName)
        {
            return File
                .ReadAllLines(vectorsFileName)
                .Select(
                    line =>
                    {
                        var record = JsonSerializer.Deserialize<Dictionary<int, double?>>(line)!;
                        var vector = record.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToArray();
                        return vector;
                    })
                .ToList();
        }

        private class SparseVectorDto
        {
            public int dimension { get; init; }
            public double[] coordinates { get; init; } = null!;
            public int[] coordinateIndices { get; init; } = null!;
        }
    }
}

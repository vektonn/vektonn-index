using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SpaceHosting.Index.Benchmarks
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

        public static List<double?[]> ReadVectors(string vectorsFileName, VectorsFileFormat vectorsFileFormat)
        {
            var vectors = vectorsFileFormat switch
            {
                VectorsFileFormat.VectorArrayJson => ReadVectorArrayFile(vectorsFileName),
                VectorsFileFormat.PandasDataFrameCsv => ReadPandasDataFrameCsvFile(vectorsFileName),
                VectorsFileFormat.PandasDataFrameJson => ReadPandasDataFrameJsonFile(vectorsFileName),
                _ => throw new ArgumentException($"Invalid vectorsFileFormat: {vectorsFileFormat}")
            };

            var vectorDimension = vectors.First().Length;
            if (vectors.Any(vector => vector.Length != vectorDimension))
                throw new InvalidOperationException("All vectors must have the same dimension");

            return vectors;
        }

        private static List<double?[]> ReadVectorArrayFile(string vectorsFileName)
        {
            var json = File.ReadAllText(vectorsFileName);
            return JsonSerializer.Deserialize<List<double?[]>>(json)!;
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
    }
}

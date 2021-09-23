using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace Vektonn.Index.Sparnn.Helpers
{
    internal static class SparseMatrixExtensions
    {
        public static SparseMatrix VStack(this IList<SparseMatrix> matrices)
        {
            return matrices.Aggregate((current, t) => (SparseMatrix)current.Stack(t));
        }

        public static IEnumerable<MathNet.Numerics.LinearAlgebra.Double.SparseVector> EnumerateVectors(this SparseMatrix matrix)
        {
            return matrix.EnumerateRows().Cast<MathNet.Numerics.LinearAlgebra.Double.SparseVector>();
        }

        public static SparseMatrix DeleteRows(this SparseMatrix matrix, params int[] rowIndexes)
        {
            Array.Sort(rowIndexes);
            var list = new List<SparseMatrix>();
            var lastDeletedIndex = -1;

            foreach (var i in rowIndexes)
            {
                var top = (SparseMatrix)matrix.SubMatrix(lastDeletedIndex + 1, i - lastDeletedIndex - 1, 0, matrix.ColumnCount);
                list.Add(top);
                lastDeletedIndex = i;
            }

            var low = (SparseMatrix)matrix.SubMatrix(lastDeletedIndex + 1, matrix.RowCount - lastDeletedIndex - 1, 0, matrix.ColumnCount);
            list.Add(low);

            return list.VStack();
        }

        public static IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> DeleteRows(this IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> vectors, params int[] rowIndexes)
        {
            Array.Sort(rowIndexes);
            var vectorsCopy = vectors.ToArray();
            var result = new MathNet.Numerics.LinearAlgebra.Double.SparseVector[vectors.Count - rowIndexes.Length];
            var currentResultIndex = 0;
            var lastDeletedSourceIndex = -1;

            foreach (var i in rowIndexes)
            {
                Array.Copy(vectorsCopy, lastDeletedSourceIndex + 1, result, currentResultIndex, i - lastDeletedSourceIndex - 1);
                currentResultIndex += i - lastDeletedSourceIndex - 1;
                lastDeletedSourceIndex = i;
            }

            Array.Copy(vectorsCopy, lastDeletedSourceIndex + 1, result, currentResultIndex, vectors.Count - lastDeletedSourceIndex - 1);

            return result;
        }

        public static SparseMatrix CreateFromVectors(IEnumerable<MathNet.Numerics.LinearAlgebra.Double.SparseVector> vectors)
        {
            return CreateFromVectors(vectors.ToArray());
        }

        public static SparseMatrix CreateFromVectors(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> vectors)
        {
            var dimension = vectors[0].Count;
            var count = vectors.Count;
            var totalNonZeroCount = vectors.Sum(v => v.NonZerosCount);

            var matrix = new SparseMatrix(SparseCompressedRowMatrixStorage<double>.OfValue(count, dimension, 0));
            var matrixStorage = (SparseCompressedRowMatrixStorage<double>)matrix.Storage;
            var currentTotalValuesCount = 0;
            var columnIndices = new int[totalNonZeroCount];
            var values = new double[totalNonZeroCount];
            foreach (var (sparseVector, i) in vectors.Select((v, i) => (v, i)))
            {
                var vectorStorage = (SparseVectorStorage<double>)sparseVector.Storage;
                Array.Copy(vectorStorage.Values, 0, values, currentTotalValuesCount, vectorStorage.Values.Length);
                Array.Copy(vectorStorage.Indices, 0, columnIndices, currentTotalValuesCount, vectorStorage.Indices.Length);
                currentTotalValuesCount += sparseVector.NonZerosCount;
                matrixStorage.RowPointers[i + 1] = currentTotalValuesCount;
            }

            matrixStorage.Values = values;
            matrixStorage.ColumnIndices = columnIndices;
            return matrix;
        }

        public static int[] NonZerosIndices(this MathNet.Numerics.LinearAlgebra.Double.SparseVector sparseVector) => sparseVector.Storage.EnumerateNonZeroIndexed().Select(x => x.Item1).ToArray();
    }
}

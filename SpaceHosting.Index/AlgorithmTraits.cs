using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SpaceHosting.Index
{
    public static class AlgorithmTraits
    {
        private static readonly Dictionary<string, ListSortDirection> MergeSortDirectionsByAlgorithm = new()
        {
            {Algorithms.FaissIndexFlatIP, ListSortDirection.Descending},
            {Algorithms.FaissIndexFlatL2, ListSortDirection.Ascending},
            {Algorithms.SparnnIndexCosine, ListSortDirection.Ascending},
            {Algorithms.SparnnIndexJaccardBinary, ListSortDirection.Ascending},
        };

        public static bool VectorsAreSparse(string indexAlgorithm)
        {
            if (indexAlgorithm.StartsWith(Algorithms.SparnnIndex))
                return true;

            if (indexAlgorithm.StartsWith(Algorithms.FaissIndex))
                return false;

            throw new InvalidOperationException($"Invalid {nameof(indexAlgorithm)}: {indexAlgorithm}");
        }

        public static ListSortDirection GetMergeSortDirection(string indexAlgorithm)
        {
            if (!MergeSortDirectionsByAlgorithm.TryGetValue(indexAlgorithm, out var mergeSortDirection))
                throw new InvalidOperationException($"Invalid {nameof(indexAlgorithm)}: {indexAlgorithm}");

            return mergeSortDirection;
        }
    }
}

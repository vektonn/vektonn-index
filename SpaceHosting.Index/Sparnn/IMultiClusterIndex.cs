using System.Collections.Generic;
using SpaceHosting.Index.Sparnn.Distances;
using MSparseVector = MathNet.Numerics.LinearAlgebra.Double.SparseVector;

namespace SpaceHosting.Index.Sparnn
{
    internal interface IMultiClusterIndex<TRecord>
    {
        IEnumerable<NearestSearchResult<TRecord>[]> Search(
            IList<MSparseVector> featureVectors,
            int resultsNumber,
            int clustersToSearchNumber,
            int? indicesToSearchNumberInput);

        void Insert(IList<MSparseVector> featureVectors, TRecord[] recordsData);
        void Delete(TRecord[] records);
    }
}

using System.Collections.Generic;
using SpaceHosting.Index.Sparnn.Distances;

namespace SpaceHosting.Index.Sparnn
{
    internal interface IMultiClusterIndex<TRecord>
    {
        IEnumerable<NearestSearchResult<TRecord>[]> Search(
            IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors,
            int resultsNumber,
            int clustersToSearchNumber,
            int? indicesToSearchNumberInput);

        void Insert(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TRecord[] recordsData);
        void Delete(TRecord[] records);
    }
}

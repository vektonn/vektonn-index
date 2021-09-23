using System.Collections.Generic;
using System.Threading.Tasks;
using Vektonn.Index.Sparnn.Distances;

namespace Vektonn.Index.Sparnn.Clusters
{
    internal interface IClusterIndex<TRecord>
        where TRecord : notnull
    {
        bool IsOverflowed { get; }
        Task<IEnumerable<NearestSearchResult<TRecord>[]>> SearchAsync(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, int resultsNumber, int clustersToSearchNumber);
        Task InsertAsync(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TRecord[] records);
        (IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, IList<TRecord> records) GetChildData();
        Task DeleteAsync(IList<TRecord> recordsToBeDeleted);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vektonn.Index.Sparnn.Distances
{
    internal interface IMatrixMetricSearchSpace<TElement>
    {
        SparseVectorsList FeatureMatrix { get; }
        IList<TElement> Elements { get; }
        Task<IEnumerable<NearestSearchResult<TElement>[]>> SearchNearestAsync(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, int resultsNumber);
    }
}

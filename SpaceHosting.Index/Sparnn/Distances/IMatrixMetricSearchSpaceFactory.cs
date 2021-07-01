using System.Collections.Generic;

namespace SpaceHosting.Index.Sparnn.Distances
{
    internal interface IMatrixMetricSearchSpaceFactory
    {
        IMatrixMetricSearchSpace<TElement> Create<TElement>(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TElement[] elements, int searchBatchSize);
    }
}
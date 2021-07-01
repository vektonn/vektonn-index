using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SpaceHosting.Index.Faiss;
using SpaceHosting.Index.Sparnn;
using SpaceHosting.Index.Sparnn.Distances;
using Vostok.Logging.Abstractions;

namespace SpaceHosting.Index
{
    public class IndexStoreFactory<TId, TData> : IIndexStoreFactory<TId, TData>
        where TId : notnull
    {
        private readonly Dictionary<string, (string, FaissMetricType)> faissSupportedAlgorithms = new()
        {
            {Algorithms.FaissIndexFlatL2, (Algorithms.FaissIndexTypeFlat, FaissMetricType.METRIC_L2)},
            {Algorithms.FaissIndexFlatIP, (Algorithms.FaissIndexTypeFlat, FaissMetricType.METRIC_INNER_PRODUCT)}
        };

        private readonly Dictionary<string, MatrixMetricSearchSpaceAlgorithm> sparnnSupportedAlgorithms = new()
        {
            {Algorithms.SparnnIndexCosine, MatrixMetricSearchSpaceAlgorithm.Cosine},
            {Algorithms.SparnnIndexJaccardBinary, MatrixMetricSearchSpaceAlgorithm.JaccardBinary}
        };

        private readonly IIndexDataStorage<TId, TData> emptyDataStorage = new EmptyIndexDataStorage<TId, TData>();

        private readonly ILog log;

        public IndexStoreFactory(ILog log)
        {
            this.log = log.ForContext("SpaceHostingIndex");
        }

        [SuppressMessage("ReSharper", "PatternAlwaysOfType")]
        public IIndexStore<TId, TData, TVector> Create<TVector>(string algorithm, int vectorDimension, bool withDataStorage, IEqualityComparer<TId> idComparer)
            where TVector : IVector
        {
            var index = typeof(TVector) switch
            {
                Type vectorType when vectorType == typeof(DenseVector) =>
                    (IIndex<TVector>)CreateDenseIndex(algorithm, vectorDimension),
                Type vectorType when vectorType == typeof(SparseVector) =>
                    (IIndex<TVector>)CreateSparseIndex(algorithm, vectorDimension),
                _ =>
                    throw new ArgumentException($"Invalid vector type: {typeof(TVector)}")
            };
            log.Info($"Created: {index.Description}");

            var dataStorage = withDataStorage
                ? new IndexDataStorage<TId, TData>(idComparer)
                : emptyDataStorage;

            return new IndexStore<TId, TData, TVector>(
                log,
                index,
                new IndexIdMapping<TId>(idComparer),
                dataStorage,
                idComparer);
        }

        private IIndex<DenseVector> CreateDenseIndex(string algorithm, int vectorDimension)
        {
            if (!faissSupportedAlgorithms.TryGetValue(algorithm, out (string BuildSteps, FaissMetricType Metric) t))
                throw new ArgumentException($"Invalid index algorithm: {algorithm}");

            return new FaissIndex(t.BuildSteps, t.Metric, vectorDimension);
        }

        private IIndex<SparseVector> CreateSparseIndex(string algorithm, int vectorDimension)
        {
            const int indicesNumber = 2;
            const int clusterSize = 1000;

            if (!sparnnSupportedAlgorithms.TryGetValue(algorithm, out var searchSpaceAlgorithm))
                throw new ArgumentException($"Invalid index algorithm: {algorithm}");

            var matrixMetricSearchSpaceFactory = new MatrixMetricSearchSpaceFactory(searchSpaceAlgorithm);
            return new SparnnIndex(() => new Random(), matrixMetricSearchSpaceFactory, indicesNumber, clusterSize, vectorDimension);
        }
    }
}

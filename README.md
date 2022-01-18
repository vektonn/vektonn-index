
# Vektonn.Index

[![CI](https://github.com/vektonn/vektonn-index/actions/workflows/ci.yml/badge.svg)](https://github.com/vektonn/vektonn-index/actions/workflows/ci.yml)
[![NuGet Status](https://img.shields.io/nuget/v/Vektonn.Index.svg)](https://www.nuget.org/packages/Vektonn.Index/)
[![license](https://img.shields.io/hexpm/l/plug.svg?color=green)](https://github.com/vektonn/vektonn-index/blob/master/LICENSE)

**Vektonn** is a high-performance battle-tested [kNN vector search](https://en.wikipedia.org/wiki/Nearest_neighbor_search#k-nearest_neighbors) engine for your data science applications. It helps you manage vectors' lifecycle and radically reduces time to market.

See [documentation](https://vektonn.github.io/vektonn/) for more info.

Vektonn.Index is a .NET library for finding nearest neighbors in vector space. Dense and sparse vectors are supported. For dense vectors we use [Faiss](https://github.com/facebookresearch/faiss) native library. For sparse vectors we have ported to C# [PySparNN](https://github.com/facebookresearch/pysparnn) library.

Vektonn.Index key features:
* One can store arbitary metadata along with the corresponding vectors in Vektonn.Index. Thus, this metadata is returned along with the search results.
* Vektonn.Index supports incremental insertion and removal of elements in the search space.

## Supported index types and metrics
For dense vectors:
* `FaissIndex.L2` - squared Euclidean (L2) distance.
* `FaissIndex.IP` - this is typically used for maximum inner product search. This is not by itself cosine similarity, unless the vectors are normalized.

By default `FaissIndex`-es are constructed in `Flat` mode, i.e. they implement exhaustive (precise) search.
To use Faiss implementation of [HNSW index](https://arxiv.org/abs/1603.09320) 
provide `Hnsw_M`, `Hnsw_EfConstruction`, and `Hnsw_EfSearch` parameters 
to `indexStoreFactory.Create<DenseVector>()` method through its optional `indexParams` parameter.

For sparse vectors:
* `SparnnIndex.Cosine` - Cosine Distance (i.e. `1 - cosine_similarity`)
* `SparnnIndex.JaccardBinary` - Jaccard Distance for _binary_ vectors (i.e. vectors whose coordinates have the values 0 or 1)

## Usage
Suppose we have an array of dense vectors (`DenseVector[] vectors`) and an array of corresponding metadata of the same size (`object[] metadata`). All vectors have dimension `vectorDimension`. And we need to search for nearest vectors using L2 metric. For this example we will use index in `vectors` array as a unique identifier of the corresponding element in the search space.

1. Create `IndexStore` object which provides index intialization and searching API.
```
var indexStoreFactory = new IndexStoreFactory<int, object>(new SilentLog());

var indexStore = indexStoreFactory.Create<DenseVector>(
    Algorithms.FaissIndexL2,
    vectorDimension,
    withDataStorage: true,
    idComparer: EqualityComparer<int>.Default);
```

2. Build search space.
```
var indexDataPoints = vectors
    .Select((vector, index) => 
        new IndexDataPointOrTombstone<int, object, DenseVector>(
            new IndexDataPoint<int, object, DenseVector>(
                Id: index,
                Vector: vector,
                Data: metadata?[index]
            )
        )
    )
    .ToArray();

const int indexBatchSize = 1000;
foreach (var batch in indexDataPoints.Batch(indexBatchSize, b => b.ToArray()))
    indexStore.UpdateIndex(batch);
```

3. Search for `k` nearest elements for each vector in `queryVectors` array.
```
var queryResults = indexStore.FindNearest(queryVectors, limitPerQuery: k);

foreach (var queryResult in queryResults)
{
    foreach (IndexFoundDataPoint<int, object, DenseVector> dp in queryResult.NearestDataPoints)
        Console.WriteLine($"Distance: {dp.Distance}, Vector: {dp.Vector}, Id: {dp.Id}, Metadata: {dp.Data}");
}

```

## Release Notes

See [CHANGELOG](CHANGELOG.md).

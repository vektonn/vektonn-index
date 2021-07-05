# SpaceHosting.Index

[![CI](https://github.com/kontur-model-ops/space-hosting-index/actions/workflows/ci.yml/badge.svg)](https://github.com/kontur-model-ops/space-hosting-index/actions/workflows/ci.yml)
[![NuGet Status](https://img.shields.io/nuget/v/SpaceHosting.Index.svg)](https://www.nuget.org/packages/SpaceHosting.Index/)

SpaceHosting.Index is a .NET library for finding nearest neighbors in vector space. Dense and sparse vectors are supported. For dense vectors we use [Faiss](https://github.com/facebookresearch/faiss) native library. For sparse vectors we have ported to C# [PySparNN](https://github.com/facebookresearch/pysparnn) library.

SpaceHosting.Index key features:
* One can store arbitary metadata along with the corresponding vectors in SpaceHosting.Index. Thus, this metadata is returned along with the search results.
* SpaceHosting.Index supports incremental insertion and removal of elements in the search space.

## Supported index types (metrics)
For dense vectors:
* `FaissIndex.Flat.L2` - squared Euclidean (L2) distance
* `FaissIndex.Flat.IP` - this is typically used for maximum inner product search. This is not by itself cosine similarity, unless the vectors are normalized.

For sparse vectors:
* `SparnnIndex.Cosine` - Cosine Distance (i.e. `1 - cosine_similarity`)
* `SparnnIndex.JaccardBinary` - Jaccard Distance for _binary_ vectors (i.e. vectors whose coordinates have the values 0 or 1)

## Usage
Suppose we have an array of dense vectors (`DenseVector[] vectors`) and an array of corresponding metadata of the same size (`object[] metadata`). All vectors have dimension `vectorDimension`. And we need to search for nearest vectors using L2 metric. For this example we will use index in `vectors` array as a unique identifier of the corresponding element in the search space.

1. Create `IndexStore` object which provides index intialization and searching API.
```
var indexStoreFactory = new IndexStoreFactory<int, object>(new SilentLog());

var indexStore = indexStoreFactory.Create<DenseVector>(
    Algorithms.FaissIndexFlatL2,
    vectorDimension,
    withDataStorage: true,
    idComparer: EqualityComparer<int>.Default);
```

2. Build search space.
```
var indexDataPoints = vectors
    .Select((vector, index) => new IndexDataPoint<int, object, DenseVector>
    {
        Id = index,
        Vector = vector,
        Data = metadata?[index]
    })
    .ToArray();

const int indexBatchSize = 1000;
foreach (var batch in indexDataPoints.Batch(indexBatchSize, b => b.ToArray()))
    indexStore.AddBatch(batch);
```

3. Search for `k` nearest elements for each vector in `vectorsToSearch` array.
```
var queryDataPoints = vectorsToSearch
    .Select(vector => new IndexQueryDataPoint<DenseVector> { Vector = vector })
    .ToArray()

var queryResults = indexStore.FindNearest(queryDataPoints, limitPerQuery: k);

foreach (var queryResult in queryResults)
{
    foreach (IndexFoundDataPoint<int, object, DenseVector> dp in queryResult.Nearest)
        Console.WriteLine($"Distance: {dp.Distance}, Vector: {dp.Vector}, Id: {dp.Id}, Metadata: {dp.Data}");
}

```

## Release Notes

See [CHANGELOG](CHANGELOG.md).

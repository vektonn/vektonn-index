# Changelog

## v0.7.1 - 2022.03.03
- Target net6.0.
- Introduce `retrieveVectors` parameter to speed things up when there is no need in nearest vectors themselves.
- Link MKL to FAISS statically to fix FAISS usage when query batches are larger than 20 vectors.
- Align versioning scheme with the main Vektonn repository.

## v0.4.5 - 2021.12.28
- Implement hyper parameters tuning for FAISS indices.

## v0.4.1 - 2021.12.23
- Add basic support for HNSW indices.

## v0.3.1 - 2021.09.24
- Change project name from SpaceHosting to Vektonn.

## v0.2.2 - 2021.09.02
- Add `AlgorithmTraits` describing various index features.
- Fix `IndexDataPoint.Vector` nullability constraint.
- Sync `SparseVector` fields naming with `SparseVectorDto` from main Vektonn repo.
- Sync `IIndexStore` interface with `IIndexShard` interface from main Vektonn repo.

## v0.1.21 - 2021.07.01
- PR [#1](https://github.com/vektonn/vektonn-index/pull/1):
  Optimize `SparnnIndex.JaccardBinary` algorithm to work faster on 'small' requests (i.e. search k nearest vectors for just a couple of target vectors at once).

## v0.1.5 - 2021.06.07
- Initial public release of privately developed package.
- Use [SourceLink](https://github.com/dotnet/sourcelink) to help ReSharper decompiler show actual code.
- Use [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) to automate generation of assembly and nuget package versions.

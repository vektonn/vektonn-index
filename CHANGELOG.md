# Changelog

## v0.1.21 - 2021.07.01
- PR [#1](https://github.com/kontur-model-ops/space-hosting-index/pull/1):
  Optimize `SparnnIndex.JaccardBinary` algorithm to work faster on 'small' requests (i.e. search k nearest vectors for just a couple of target vectors at once).

## v0.1.5 - 2021.06.07
- Initial public release of privately developed package.
- Use [SourceLink](https://github.com/dotnet/sourcelink) to help ReSharper decompiler show actual code.
- Use [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) to automate generation of assembly and nuget package versions.

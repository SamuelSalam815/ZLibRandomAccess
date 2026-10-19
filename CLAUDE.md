# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A .NET 10 exploration of zlib's `Z_FULL_FLUSH`: gzip output is periodically fully flushed so decompression can restart at those points ("recovery points"). That gives random access into compressed logs and lets text search run in parallel across segments. The BCL `GZipStream` doesn't expose full flushes, so the repo P/Invokes into a zlib built from source.

Windows only. Most projects target `net10.0-windows`.

## Commands

```
git submodule update --init          # zlib source lives in the zlib/ submodule
dotnet build ArchiveAccessPointVisualizer.slnx
dotnet build -c Release              # use Release for benchmarks
dotnet test ArchiveAccessPointVisualizer.slnx
dotnet test ZLibWrapper.Tests --filter "FullyQualifiedName~<TestClassOrMethod>"   # single test (MSTest)
dotnet run --project Benchmarking.TextSearch       # speed benchmarks (BenchmarkDotNet)
dotnet run --project Benchmarking.CompressionRatio # writes result.csv / result.svg
```

Building `ZLibBindings` runs an MSBuild target that calls `cmake` with the **"Visual Studio 17 2022"** generator to build `zlib/build/Release/z.dll`. That means CMake and VS 2022 C++ build tools must be installed. The DLL gets copied to the output directory.

Tests use MSTest + Shouldly.

## Architecture (dependency order)

- **ZLibBindings**: raw P/Invoke layer (`ZLibLowLevelBindings`) plus the zlib constants and the `z_stream_s` / `gz_header_s` structs. Struct layout and string marshalling here are fragile (see `project-notes.md`: a wrong `z_stream_s` size showed up as `Z_VERSION_ERROR`, and marshalling a `const char*` return as `string` caused heap corruption).
- **ZLibWrapper**: managed `Stream`s over the bindings.
  - `ZLibDeflateStream` / `ZLibInflateStream` are the base streams. The `*WithRecoveryPoints` variants add full flushes and seeking.
  - `GZipWritingStreamWithRecoveryPoints` full-flushes every N uncompressed bytes and raises `RecoveryPointWritten(RecoveryPointOffset)`. A `RecoveryPointOffset` pairs an uncompressed offset with a compressed offset and the number of line feeds before the point (`LineOffset`). Points fall mid-line, so a reader that wants a whole line starts from the previous point (or the start of data, which has no point); these offsets get persisted as CSV (`RecoveryPointOffsetCsvWriter`).
  - `GZipReadingStreamWithRecoveryPoints.JumpTo(i)` resumes inflation at a recovery point.
  - The pump/navigation decision logic lives in `Logic/` and is kept separate from the I/O.
- **ArchiveViewerBackend**: search over compressed archives. `TextSearcherFactory.CreateParallelTextSearcher` opens one gzip reader per recovery point. Each reader is bounded by `StreamWithByteLimit` and overlaps the next segment slightly (so lines that cross a boundary still match), then the readers are searched in parallel (`ParallelScanTextSearcher`). Compare with `ScanTextSearcher`, which reads sequentially.
  - Showing text: `TextPresenter` renders a window of lines for the front end. It uses `LineAwareReader` (forward reads decoded straight into a `CompactingBuffer<char>`, counting lines, throwing `LineTooLongException` if a line fills the buffer) and `BackwardSeekableStream<TContext>` (owns the compressed stream; backward seeks restart from the start of a plain gzip, or jump to the nearest recovery point). `CompactingBuffer<T>` keeps its items in one contiguous run (it moves them to the start of the array when a caller needs more room at the end) so a `Decoder` can write into its free space; a ring buffer was rejected because its free space can be cut in two at the end of the array.
- **LogSimulator**: a fictitious RPG simulation (combat, dice rolls) that produces realistic log text as test and benchmark input. `LogSimulator.Application` is a console app that writes simulated logs through the compressing streams.
- **ArchiveAccessPointVisualizer**: WPF UI (work in progress).
- **Benchmarking.TextSearch**: BenchmarkDotNet benchmarks comparing sequential and parallel regex search. `GraphGenerator` turns BenchmarkDotNet CSV reports into graphs (in progress).
- **Benchmarking.CompressionRatio**: a custom runner rather than BenchmarkDotNet. `CompressionRequest.AllCombinationsOf(methods, levels, sizes)` goes through an `ICompressionBenchmarkRunner`, and the results go to `ICompressionResultExporter`s (CSV, SVG graph). `Fake*` implementations exist for tests.
- **Benchmarking.Helpers**: shared OxyPlot chart building (`BarChartBuilder` takes `BarChartBar(Category, Series, Value)` and builds a `PlotModel`). It needs exactly one bar per category/series pair. `CompressionResultGraphExporter` hasn't been migrated to it yet.

Test projects are named `<Project>.Tests`. `LogSimulator.Application.Tests.Integration` holds the integration tests.

**Integration test snapshot:** `ProgramIntegrationTest` decompresses the committed logs in `LogSimulator.Application.Tests.Integration/TestArtifacts` (1 MB of logs, 64 KB recovery point interval) and checks that the BCL, zlib and zlib-with-recovery-points outputs match. It takes about a second, so it runs with the normal tests. To regenerate the snapshot: `dotnet run --project LogSimulator.Application -- <outputDirectory> 1 64` (arguments: output directory, megabytes of logs, recovery point interval in KB), then update the timestamp in `ProgramIntegrationTest.LogTimeStamps`. The defaults (no arguments) write 128 MB to `C:\Temp`.

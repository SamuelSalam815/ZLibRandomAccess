<#
.SYNOPSIS
Runs benchmarks and rewrites the graphs in images/ that the README shows.

.PARAMETER Task
compression  Run the compression ratio benchmark and write images/compression-ratio-graph.svg.
text-search  Run the regex search benchmark and write images/text-search-graph.svg. This takes a long time.
all          Run both.

.EXAMPLE
.\benchmarks.ps1 compression
#>
param(
    [Parameter(Mandatory, Position = 0)]
    [ValidateSet('compression', 'text-search', 'all')]
    [string] $Task
)

$ErrorActionPreference = 'Stop'
$images = Join-Path $PSScriptRoot 'images'

function Invoke-Benchmark([string] $Project) {
    # Release: BenchmarkDotNet refuses Debug builds and the numbers would mean nothing.
    dotnet run -c Release --project (Join-Path $PSScriptRoot $Project) -- $images
    if ($LASTEXITCODE -ne 0) {
        throw "$Project failed with exit code $LASTEXITCODE"
    }
}

if ($Task -in 'compression', 'all') { Invoke-Benchmark 'Benchmarking.CompressionRatio' }
if ($Task -in 'text-search', 'all') { Invoke-Benchmark 'Benchmarking.TextSearch' }

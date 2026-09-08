```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 5600X 3.70GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  Job-DRSAZQ : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3

RunStrategy=Monitoring  

```
| Method                    | UncompressedLogSizeInMegabytes | Mean        | Error    | StdDev   | Ratio | RatioSD |
|-------------------------- |------------------------------- |------------:|---------:|---------:|------:|--------:|
| **SystemGzip_FindAll**        | **256**                            |  **3,866.1 ms** | **616.0 ms** | **407.5 ms** |  **1.01** |    **0.14** |
| ZlibGzip_FindAll          | 256                            |  3,983.8 ms | 746.5 ms | 493.8 ms |  1.04 |    0.15 |
| ZlibGzip_Parallel_FindAll | 256                            |    898.5 ms | 210.8 ms | 139.5 ms |  0.23 |    0.04 |
|                           |                                |             |          |          |       |         |
| **SystemGzip_FindAll**        | **512**                            |  **7,591.9 ms** | **723.9 ms** | **478.8 ms** |  **1.00** |    **0.08** |
| ZlibGzip_FindAll          | 512                            |  7,751.0 ms | 913.6 ms | 604.3 ms |  1.02 |    0.09 |
| ZlibGzip_Parallel_FindAll | 512                            |  1,776.0 ms | 238.0 ms | 157.4 ms |  0.23 |    0.02 |
|                           |                                |             |          |          |       |         |
| **SystemGzip_FindAll**        | **1024**                           | **14,900.4 ms** | **991.5 ms** | **655.8 ms** |  **1.00** |    **0.06** |
| ZlibGzip_FindAll          | 1024                           | 15,385.7 ms | 966.1 ms | 639.0 ms |  1.03 |    0.06 |
| ZlibGzip_Parallel_FindAll | 1024                           |  3,677.6 ms | 293.9 ms | 194.4 ms |  0.25 |    0.02 |

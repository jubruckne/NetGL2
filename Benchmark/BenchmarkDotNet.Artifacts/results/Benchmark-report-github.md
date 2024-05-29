```

BenchmarkDotNet v0.13.12, macOS Sonoma 14.5 (23F79) [Darwin 23.5.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 8.0.300
  [Host]     : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD


```
| Method                   | Mean       | Error    | StdDev   | Allocated |
|------------------------- |-----------:|---------:|---------:|----------:|
| SimplexKernel_SIMD_float |   969.3 ms | 14.15 ms | 13.23 ms |   2.73 KB |
| SimplexKernel_SIMD_half  | 1,380.2 ms | 10.25 ms |  9.59 ms |   2.73 KB |

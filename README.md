# JsonParserPerformance

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

A high-performance Protobuf JSON parser for .NET, built as a faster, lower-allocation drop-in alternative to `Google.Protobuf`'s built-in `JsonParser`.

## Benchmark Results
 
Tested against the full `ParserTestRoot` message — a composite message covering every Protobuf field type (scalars, repeated, maps, oneofs, well-known types, nested messages, enums).
 
| Method                     | Mean     | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |---------:|------:|-------:|-------:|----------:|------------:|
| Google_ParseParserTestRoot | 90.61 µs |  1.01 | 9.3994 | 0.2441 |  38.75 KB |        1.00 |
| Custom_ParseParserTestRoot | 55.17 µs |  **0.62** | 4.6387 |      - |  **19.19 KB** |        **0.50** |
 

| Metric | Improvement |
|--------|-------------|
|**Speed** | ~39% faster parsing |
|**Memory** | ~50% less memory allocation |
|**Gen0 GC** | ~51% reduction in Gen0 collections |
|**Gen1 GC** | Zero Gen1 allocations |

> Benchmarks run with [BenchmarkDotNet](https://benchmarkdotnet.org). See [`ParserBenchmarks.cs`](JsonParserPerformance.Benchmark/ParserBenchmarks.cs) to reproduce.

## Getting Started
 
### Installation
  
```bash
git clone https://github.com/AkshayDhola/JsonParserPerformance.git
```

### Usage
 
```csharp
using JsonParserPerformance;
 
// Drop-in replacement for Google.Protobuf.JsonParser
IMessageProtobuf result = JsonParser.Parse<IMessageProtobuf>(jsonStr);

// Optionally, you can also use the static parser directly:
IMessageProtobuf result = IMessageProtobuf.Parser.ParseFrom(jsonStr);
```
 
## Development

# Run Tests
 
```bash
dotnet test JsonParserPerformance.UnitTests
```
 
# Running Benchmarks
 
```bash
dotnet run --project JsonParserPerformance.Benchmark -c Release
```
 
## Contributing
 
See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.
 
## License
 
MIT — see [LICENSE](LICENSE) for details.
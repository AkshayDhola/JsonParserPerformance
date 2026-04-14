using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Google.Protobuf.Reflection;
using JsonParserPerformance.UnitTests.TestData;
using Parser.Test;
using JsonParserPerf = JsonParserPerformance.JsonParser;
using JsonParserGoogle = Google.Protobuf.JsonParser;

namespace JsonParserPerformance.Benchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 20, iterationCount: 100)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[OperationsPerSecond]
public class ParserBenchmarks
{
    private static readonly TypeRegistry Registry = TypeRegistry.FromMessages(
        AllScalars.Descriptor,
        AllRepeated.Descriptor,
        AllMaps.Descriptor,
        AllOneofs.Descriptor,
        Outer.Descriptor,
        AllWellKnown.Descriptor,
        WithFieldOptions.Descriptor,
        WithReserved.Descriptor,
        ParserTestRoot.Descriptor
    );

    private static readonly JsonParserGoogle GoogleParser =
        new(JsonParserGoogle.Settings.Default.WithTypeRegistry(Registry));

    private string _json = null!;

    [GlobalSetup]
    public void Setup()
    {
        _json = JsonTestData.Json;
    }

    [Benchmark(Baseline = true)]
    public ParserTestRoot GoogleProtobuf_JsonParser()
        => GoogleParser.Parse<ParserTestRoot>(_json);

    [Benchmark]
    public ParserTestRoot JsonParserPerformance_JsonParser()
        => JsonParserPerf.Parse<ParserTestRoot>(_json);
}
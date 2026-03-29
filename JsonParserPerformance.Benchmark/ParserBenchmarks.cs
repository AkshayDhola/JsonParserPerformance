using BenchmarkDotNet.Attributes;
using Google.Protobuf.Reflection;
using JsonParserPerformance.UnitTests.TestData;
using Parser.Test;
using JsonParserPerf = JsonParserPerformance.JsonParser;
using JsonParserGoogle = Google.Protobuf.JsonParser;

namespace JsonParserPerformance.Benchmark;

[MemoryDiagnoser]
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
        _json = Sample.Json;
    }

    [Benchmark(Baseline = true)]
    public ParserTestRoot Google_ParseParserTestRoot()
        => GoogleParser.Parse<ParserTestRoot>(_json);

    [Benchmark]
    public ParserTestRoot Custom_ParseParserTestRoot()
        => JsonParserPerf.Parse<ParserTestRoot>(_json);
}
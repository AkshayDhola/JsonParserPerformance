using Google.Protobuf;
using Google.Protobuf.Reflection;
using Parser.Test;
using JsonParserGoogle = Google.Protobuf.JsonParser;
using JsonParserPerf = JsonParserPerformance.JsonParser;

namespace JsonParserPerformance.UnitTests.Fixtures;

public abstract class ParserFixture
{
    protected static readonly TypeRegistry FullRegistry = TypeRegistry.FromMessages(
        AllScalars.Descriptor,
        AllRepeated.Descriptor,
        AllMaps.Descriptor,
        AllOneofs.Descriptor,
        Outer.Descriptor,
        AllWellKnown.Descriptor,
        WithFieldOptions.Descriptor,
        WithReserved.Descriptor,
        ParserTestRoot.Descriptor,
        FloatTest.Descriptor
    );

    protected static readonly JsonFormatter Formatter =
        new(new JsonFormatter.Settings(formatDefaultValues: true, typeRegistry: FullRegistry));

    protected static JsonParserGoogle Google =>
        new(JsonParserGoogle.Settings.Default.WithTypeRegistry(FullRegistry));

    protected static (T Expected, T Actual) RoundTripAndCompare<T>(T message)
        where T : IMessage<T>, new()
    {
        var json = Formatter.Format(message);
        return ParseWithBothParsers<T>(json);
    }

    protected static (T Expected, T Actual) ParseWithBothParsers<T>(string json)
        where T : IMessage<T>, new()
    {
        var expected = Google.Parse<T>(json);
        var actual = JsonParserPerf.Parse<T>(json);
        return (expected, actual);
    }
}

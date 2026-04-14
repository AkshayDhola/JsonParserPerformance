using Google.Protobuf;
using Google.Protobuf.Reflection;
using JsonParserPerformance.UnitTests.Helpers;
using Parser.Test;

namespace JsonParserPerformance.UnitTests.TestData;

public static class JsonTestData
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
        ParserTestRoot.Descriptor,
        FloatTest.Descriptor
    );

    private static readonly JsonFormatter Formatter =
        new(new JsonFormatter.Settings(formatDefaultValues: true, typeRegistry: Registry));

    public static readonly string FullRoot = Formatter.Format(MessageBuilders.ParserTestRoot_Full());

    public static string Json => FullRoot;
}
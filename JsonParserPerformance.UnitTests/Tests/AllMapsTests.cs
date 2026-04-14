using FluentAssertions;
using Google.Protobuf;
using JsonParserPerformance.UnitTests.Fixtures;
using JsonParserPerformance.UnitTests.Helpers;
using Parser.Test;

namespace JsonParserPerformance.UnitTests.Tests;

public sealed class AllMapsTests : ParserFixture
{
    [Fact]
    public void Parse_WithStringKeyedMaps_AllValueTypes_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllMaps_StringKeyed());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithIntegerKeyedMaps_AllWidths_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllMaps_IntegerKeyed());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithEmptyMessage_AllMapsEmpty_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new AllMaps());
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(500)]
    public void Parse_WithLargeStringToStringMap_ReturnsEquivalentToGoogle(int count)
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllMaps_Large(count));
        actual.Should().BeEquivalentTo(expected, because: $"count={count}");
    }

    [Fact]
    public void Parse_WithMapStringToNestedMessage_ReturnsEquivalentToGoogle()
    {
        var msg = new AllMaps
        {
            MStringMsg =
            {
                ["first"]  = new AllScalars { FInt32 = 1, FString = "one"   },
                ["second"] = new AllScalars { FInt32 = 2, FString = "two"   },
                ["empty"]  = new AllScalars()
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithMapStringToEnum_AllColorValues_ReturnsEquivalentToGoogle()
    {
        var msg = new AllMaps
        {
            MStringEnum =
            {
                ["unspecified"] = Color.Unspecified,
                ["red"]        = Color.Red,
                ["green"]      = Color.Green,
                ["blue"]       = Color.Blue
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithMapStringToBytes_IncludingEmpty_ReturnsEquivalentToGoogle()
    {
        var msg = new AllMaps
        {
            MStringBytes =
            {
                ["data"]  = ByteString.CopyFromUtf8("hello bytes"),
                ["empty"] = ByteString.Empty
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithBoolKeyedMap_BothKeys_ReturnsEquivalentToGoogle()
    {
        var msg = new AllMaps
        {
            MBoolString = { [true] = "yes", [false] = "no" }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithMapUint64Keys_BoundaryValues_ReturnsEquivalentToGoogle()
    {
        var msg = new AllMaps
        {
            MUint64String = { [0UL] = "zero", [ulong.MaxValue] = "max" }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithSingleEntryPerMap_ReturnsEquivalentToGoogle()
    {
        var msg = new AllMaps
        {
            MStringString = { ["only"] = "entry" },
            MInt32String = { [42] = "forty-two" },
            MBoolString = { [true] = "truth" }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }
}
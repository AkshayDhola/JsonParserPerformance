using FluentAssertions;
using Google.Protobuf;
using JsonParserPerformance.UnitTests.Fixtures;
using JsonParserPerformance.UnitTests.Helpers;
using Parser.Test;

namespace JsonParserPerformance.UnitTests.Tests;

public sealed class AllRepeatedTests : ParserFixture
{
    [Fact]
    public void Parse_WithFullyPopulatedLists_IncludingSpecialFloats_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllRepeated_Full());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithEmptyMessage_AllListsEmpty_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new AllRepeated());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithSingleElementPerList_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllRepeated_SingleElement());
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1_000)]
    [InlineData(5_000)]
    public void Parse_WithLargeInt32List_ReturnsEquivalentToGoogle(int count)
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllRepeated_Large(count));
        actual.Should().BeEquivalentTo(expected, because: $"count={count}");
    }

    [Fact]
    public void Parse_WithAllColorEnumValues_ReturnsEquivalentToGoogle()
    {
        var msg = new AllRepeated
        {
            REnum = { Color.Unspecified, Color.Red, Color.Green, Color.Blue }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithRepeatedBytes_IncludingEmpty_ReturnsEquivalentToGoogle()
    {
        var msg = new AllRepeated
        {
            RBytes =
            {
                ByteString.CopyFromUtf8("hello"),
                ByteString.CopyFromUtf8("world"),
                ByteString.Empty,
                ByteString.CopyFrom(new byte[] { 0x00, 0xFF, 0x7F })
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithRepeatedStrings_IncludingEmptyAndUnicode_ReturnsEquivalentToGoogle()
    {
        var msg = new AllRepeated
        {
            RString = { "alpha", "", "ε∑√∞", "日本語", "\t\n\r", "\"quoted\"" }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithRepeatedNestedMessages_ReturnsEquivalentToGoogle()
    {
        var msg = new AllRepeated
        {
            RMessage =
            {
                new AllScalars { FInt32 = 1, FString = "first",  FBool = true  },
                new AllScalars { FInt32 = 2, FDouble  = 3.14                   },
                new AllScalars()
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithRepeatedSpecialFloats_NaNAndInfinities_ReturnsEquivalentToGoogle()
    {
        var msg = new AllRepeated
        {
            RFloat = { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 0f },
            RDouble = { double.NaN, double.PositiveInfinity, double.NegativeInfinity, 0.0 }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithRepeatedBooleans_AllCombinations_ReturnsEquivalentToGoogle()
    {
        var msg = new AllRepeated { RBool = { true, false, true, false } };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }
}
using FluentAssertions;
using JsonParserPerformance.UnitTests.Fixtures;
using JsonParserPerformance.UnitTests.Helpers;
using Parser.Test;

namespace JsonParserPerformance.UnitTests.Tests;

public sealed class AllScalarsTests : ParserFixture
{
    [Fact]
    public void Parse_WithExtremeValues_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllScalars_Extreme());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithZeroAndDefaultValues_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllScalars_Zero());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithEmptyMessage_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new AllScalars());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithSpecialFloatValues_NaNAndNegInfinity_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllScalars_SpecialFloats());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithEscapedStringCharacters_TabNewlineQuoteBackslashNull_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllScalars_EscapedString());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithUnicodeString_MultiScriptEmoji_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllScalars_Unicode());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithLargeBase64Bytes_4096Bytes_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllScalars_LargeBytes(4096));
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0f, 0.0)]
    [InlineData(1.5f, 1.5)]
    [InlineData(-3.14f, -3.14)]
    [InlineData(1e10f, 1e10)]
    public void Parse_WithTypicalFloatDoublePairs_ReturnsEquivalentToGoogle(float f, double d)
    {
        var (expected, actual) = RoundTripAndCompare(new AllScalars { FFloat = f, FDouble = d });
        actual.Should().BeEquivalentTo(expected, because: $"FFloat={f}, FDouble={d}");
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void Parse_WithInt32BoundaryValues_ReturnsEquivalentToGoogle(int value)
    {
        var (expected, actual) = RoundTripAndCompare(new AllScalars { FInt32 = value });
        actual.Should().BeEquivalentTo(expected, because: $"FInt32={value}");
    }

    [Theory]
    [InlineData(long.MinValue)]
    [InlineData(-1L)]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(long.MaxValue)]
    public void Parse_WithInt64BoundaryValues_ReturnsEquivalentToGoogle(long value)
    {
        var (expected, actual) = RoundTripAndCompare(new AllScalars { FInt64 = value });
        actual.Should().BeEquivalentTo(expected, because: $"FInt64={value}");
    }

    [Fact]
    public void Parse_WithMaxUint64_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new AllScalars { FUint64 = ulong.MaxValue });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithBoolTrue_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new AllScalars { FBool = true });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithBoolFalse_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new AllScalars { FBool = false });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithEmptyString_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new AllScalars { FString = string.Empty });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithVeryLongString_1000Chars_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new AllScalars { FString = new string('a', 1000) });
        actual.Should().BeEquivalentTo(expected);
    }
}
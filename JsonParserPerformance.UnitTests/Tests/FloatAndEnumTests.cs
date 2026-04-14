using FluentAssertions;
using JsonParserPerformance.UnitTests.Fixtures;
using JsonParserPerformance.UnitTests.Helpers;
using Parser.Test;

namespace JsonParserPerformance.UnitTests.Tests;

public sealed class FloatEdgeCaseTests : ParserFixture
{
    [Fact]
    public void Parse_WithNaN_BothFields_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.FloatTest_NaN());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithPositiveInfinity_BothFields_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.FloatTest_PosInfinity());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithNegativeInfinity_BothFields_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.FloatTest_NegInfinity());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithMixedSpecialValues_RawJsonStrings_ReturnsEquivalentToGoogle()
    {
        const string json = """{"fFloat":"NaN","fDouble":"Infinity"}""";
        var (expected, actual) = ParseWithBothParsers<FloatTest>(json);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithNegativeInfinityJsonString_ReturnsEquivalentToGoogle()
    {
        const string json = """{"fFloat":"-Infinity","fDouble":"-Infinity"}""";
        var (expected, actual) = ParseWithBothParsers<FloatTest>(json);
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0f, 0.0)]
    [InlineData(1f, 1.0)]
    [InlineData(-1f, -1.0)]
    [InlineData(3.14f, 3.14)]
    public void Parse_WithNormalValues_ReturnsEquivalentToGoogle(float f, double d)
    {
        var (expected, actual) = RoundTripAndCompare(new FloatTest { FFloat = f, FDouble = d });
        actual.Should().BeEquivalentTo(expected, because: $"f={f} d={d}");
    }
}

public sealed class EnumTests : ParserFixture
{
    [Theory]
    [InlineData(Color.Unspecified)]
    [InlineData(Color.Red)]
    [InlineData(Color.Green)]
    [InlineData(Color.Blue)]
    public void Parse_WithColorEnum_AllValues_ReturnsEquivalentToGoogle(Color color)
    {
        var (expected, actual) = RoundTripAndCompare(new ParserTestRoot { Color = color });
        actual.Should().BeEquivalentTo(expected, because: $"Color = {color}");
    }

    [Theory]
    [InlineData(Direction.Unspecified)]
    [InlineData(Direction.North)]
    [InlineData(Direction.South)]
    [InlineData(Direction.East)]
    [InlineData(Direction.West)]
    public void Parse_WithDirectionEnum_AllValues_ReturnsEquivalentToGoogle(Direction direction)
    {
        var (expected, actual) = RoundTripAndCompare(new ParserTestRoot { Direction = direction });
        actual.Should().BeEquivalentTo(expected, because: $"Direction = {direction}");
    }
}
using FluentAssertions;
using JsonParserPerformance.UnitTests.Fixtures;
using JsonParserPerformance.UnitTests.Helpers;
using Parser.Test;
using static Parser.Test.Outer.Types;
using static Parser.Test.Outer.Types.Inner.Types;

namespace JsonParserPerformance.UnitTests.Tests;

public sealed class NestedMessageTests : ParserFixture
{
    [Fact]
    public void Parse_FullyNestedMessage_ThreeLevelsDeep_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.Outer_FullyNested());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_EmptyOuter_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new Outer());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_OuterWithInnerOnly_NoDeep_ReturnsEquivalentToGoogle()
    {
        var msg = new Outer
        {
            OuterField = "only outer and inner",
            Inner = new Inner { InnerField = "inner only" }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_OuterWithDeepInnerOnly_NoInnerField_ReturnsEquivalentToGoogle()
    {
        var msg = new Outer
        {
            Inner = new Inner
            {
                Deep = new DeepInner { DeepField = int.MaxValue }
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(InnerEnum.Unspecified)]
    [InlineData(InnerEnum.A)]
    [InlineData(InnerEnum.B)]
    public void Parse_WithInnerEnumValue_ReturnsEquivalentToGoogle(InnerEnum innerEnum)
    {
        var (expected, actual) = RoundTripAndCompare(new Outer { InnerEnum = innerEnum });
        actual.Should().BeEquivalentTo(expected, because: $"InnerEnum = {innerEnum}");
    }

    [Fact]
    public void Parse_OuterWithAllFieldsSet_ReturnsEquivalentToGoogle()
    {
        var msg = new Outer
        {
            OuterField = "outer text",
            InnerEnum = InnerEnum.B,
            Inner = new Inner
            {
                InnerField = "inner text",
                Deep = new DeepInner { DeepField = -1 }
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_OuterWithEmptyStringFields_ReturnsEquivalentToGoogle()
    {
        var msg = new Outer
        {
            OuterField = string.Empty,
            Inner = new Inner
            {
                InnerField = string.Empty,
                Deep = new DeepInner { DeepField = 0 }
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }
}
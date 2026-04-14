using FluentAssertions;
using Google.Protobuf;
using JsonParserPerformance.UnitTests.Fixtures;
using JsonParserPerformance.UnitTests.Helpers;
using Parser.Test;

namespace JsonParserPerformance.UnitTests.Tests;

public sealed class AllOneofTests : ParserFixture
{
    public static IEnumerable<object[]> AllVariants() =>
        MessageBuilders.AllOneofs_AllVariants().Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(AllVariants))]
    public void Parse_EachOneoFVariant_ReturnsEquivalentToGoogle(AllOneofs msg)
    {
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected,
            because: $"active oneof case = {msg.ScalarChoiceCase}/{msg.TypedChoiceCase}");
    }

    [Fact]
    public void Parse_ChoiceInt32_MinValue_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(
            new AllOneofs { SharedField = "s", ChoiceInt32 = int.MinValue });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_ChoiceDouble_MaxValue_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(
            new AllOneofs { SharedField = "s", ChoiceDouble = double.MaxValue });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_ChoiceBool_True_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(
            new AllOneofs { SharedField = "s", ChoiceBool = true });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_ChoiceString_NonEmpty_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(
            new AllOneofs { SharedField = "s", ChoiceString = "a string choice" });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_ChoiceBytes_UTF8Content_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(
            new AllOneofs { SharedField = "s", ChoiceBytes = ByteString.CopyFromUtf8("bytes choice") });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_ChoiceEnum_Blue_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(
            new AllOneofs { SharedField = "s", ChoiceEnum = Color.Blue });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_ChoiceMessage_NestedAllScalars_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(
            new AllOneofs
            {
                SharedField = "s",
                ChoiceMessage = new AllScalars { FInt32 = 99, FBool = true }
            });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_OnlySharedField_NoneChosen_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(
            new AllOneofs { SharedField = "only shared" });
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_EmptyMessage_AllFieldsDefault_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new AllOneofs());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_SharedFieldEmpty_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(
            new AllOneofs { SharedField = string.Empty, ChoiceInt32 = 0 });
        actual.Should().BeEquivalentTo(expected);
    }
}
using FluentAssertions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerformance.UnitTests.Fixtures;
using JsonParserPerformance.UnitTests.TestData;
using Parser.Test;

namespace JsonParserPerformance.UnitTests.Tests;

public sealed class ParserTestRootTests : ParserFixture
{
    [Fact]
    public void Parse_FullSample_MatchesGoogleParser()
    {
        var (expected, actual) = ParseWithBothParsers<ParserTestRoot>(JsonTestData.FullRoot);
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_EmptyRoot_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new ParserTestRoot());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithRepeatedScalarList_ThreeEntries_ReturnsEquivalentToGoogle()
    {
        var msg = new ParserTestRoot
        {
            ScalarList =
            {
                new AllScalars { FInt32 = 1, FString = "one" },
                new AllScalars { FInt32 = 2, FDouble = 3.14  },
                new AllScalars()
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithScalarIndexMap_TwoEntries_ReturnsEquivalentToGoogle()
    {
        var msg = new ParserTestRoot
        {
            ScalarIndex =
            {
                ["id-1"] = new AllScalars { FString = "first",  FBool  = true },
                ["id-2"] = new AllScalars { FString = "second", FInt32 = 99   }
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithOneofIndexMap_MixedVariants_ReturnsEquivalentToGoogle()
    {
        var msg = new ParserTestRoot
        {
            OneofIndex =
            {
                ["a"] = new AllOneofs { SharedField = "s1", ChoiceString = "str"      },
                ["b"] = new AllOneofs { SharedField = "s2", ChoiceEnum   = Color.Blue }
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithCreatedAtTimestamp_ReturnsEquivalentToGoogle()
    {
        var msg = new ParserTestRoot
        {
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow.Date)
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithUpdateMaskMultiplePaths_ReturnsEquivalentToGoogle()
    {
        var msg = new ParserTestRoot
        {
            UpdateMask = FieldMask.FromString("scalars.f_string,color,label")
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithExtraAnyField_PackedAllScalars_ReturnsEquivalentToGoogle()
    {
        var msg = new ParserTestRoot
        {
            Extra = Any.Pack(new AllScalars { FString = "packed inside Any", FInt32 = 42 })
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithFieldOptions_AllFieldsSet_ReturnsEquivalentToGoogle()
    {
        var msg = new WithFieldOptions
        {
            ActiveField = "active",
            OldField = "deprecated but parseable",
            PackedInts = { -100, -1, 0, 1, 100, int.MaxValue }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithFieldOptions_EmptyPackedInts_ReturnsEquivalentToGoogle()
    {
        var msg = new WithFieldOptions { ActiveField = "only active" };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithReserved_CurrentFieldOnly_ReturnsEquivalentToGoogle()
    {
        var msg = new WithReserved { CurrentField = "only valid field" };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_CalledTwiceOnSameJson_ReturnsSameResult()
    {
        var json = JsonTestData.FullRoot;
        var first = JsonParserPerformance.JsonParser.Parse<ParserTestRoot>(json);
        var second = JsonParserPerformance.JsonParser.Parse<ParserTestRoot>(json);
        first.Should().BeEquivalentTo(second);
    }
}
using FluentAssertions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerformance.UnitTests.Fixtures;
using JsonParserPerformance.UnitTests.Helpers;
using Parser.Test;
using System.Globalization;
using System.Text;
using WellKnownValue = Google.Protobuf.WellKnownTypes.Value;

namespace JsonParserPerformance.UnitTests.Tests;

public sealed class AllWellKnownTests : ParserFixture
{
    [Fact]
    public void Parse_WithAllFieldsPopulated_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllWellKnown_Full());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithEmptyMessage_AllWrappersUnset_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(new AllWellKnown());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithEpochTimestamp_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllWellKnown_EpochTimestamp());
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("2024-03-15T08:30:00.500Z")]
    [InlineData("2000-01-01T00:00:00Z")]
    [InlineData("1970-01-01T00:00:00Z")]
    [InlineData("9999-12-31T23:59:59.999Z")]
    public void Parse_WithVariousTimestamps_ReturnsEquivalentToGoogle(string iso)
    {
        var msg = new AllWellKnown
        {
            Timestamp = Timestamp.FromDateTime(
                DateTime.Parse(iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal).ToUniversalTime())
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected, because: $"timestamp = {iso}");
    }

    [Fact]
    public void Parse_WithZeroDuration_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllWellKnown_ZeroDuration());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithNegativeDuration_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllWellKnown_NegativeDuration());
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3600)]
    [InlineData(-90)]
    [InlineData(86400 * 365)]
    public void Parse_WithDurationSeconds_ReturnsEquivalentToGoogle(int seconds)
    {
        var msg = new AllWellKnown
        {
            Duration = Duration.FromTimeSpan(TimeSpan.FromSeconds(seconds))
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected, because: $"seconds={seconds}");
    }

    [Fact]
    public void Parse_WithAllWrappersSet_ReturnsEquivalentToGoogle()
    {
        var msg = new AllWellKnown
        {
            WrapBool = true,
            WrapInt32 = int.MinValue,
            WrapInt64 = long.MaxValue,
            WrapUint32 = uint.MaxValue,
            WrapUint64 = ulong.MaxValue,
            WrapFloat = float.MaxValue,
            WrapDouble = double.Epsilon,
            WrapString = "wrapped string \u0000 with null",
            WrapBytes = ByteString.CopyFrom("wrapped", Encoding.UTF8)
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithNullableWrappersExplicitlyNull_ReturnsEquivalentToGoogle()
    {
        // Wrappers not set → null in proto3 JSON
        var (expected, actual) = RoundTripAndCompare(new AllWellKnown());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithAnyPackingScalars_ReturnsEquivalentToGoogle()
    {
        var msg = new AllWellKnown
        {
            AnyField = Any.Pack(new AllScalars { FString = "in any", FInt32 = 7, FBool = true })
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithAnyPackingRepeatedMessage_ReturnsEquivalentToGoogle()
    {
        var (expected, actual) = RoundTripAndCompare(MessageBuilders.AllWellKnown_AnyWithRepeated());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithStructContainingAllValueKinds_ReturnsEquivalentToGoogle()
    {
        var msg = new AllWellKnown
        {
            StructField = new Struct
            {
                Fields =
                {
                    ["strKey"]  = WellKnownValue.ForString("strValue"),
                    ["numKey"]  = WellKnownValue.ForNumber(123.45),
                    ["boolKey"] = WellKnownValue.ForBool(true),
                    ["nullKey"] = WellKnownValue.ForNull(),
                    ["listKey"] = WellKnownValue.ForList(WellKnownValue.ForNumber(1), WellKnownValue.ForString("two")),
                    ["objKey"]  = WellKnownValue.ForStruct(new Struct
                    {
                        Fields = { ["nested"] = WellKnownValue.ForBool(false) }
                    })
                }
            }
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithValueList_MixedTypes_ReturnsEquivalentToGoogle()
    {
        var msg = new AllWellKnown
        {
            ValueField = WellKnownValue.ForList(
                WellKnownValue.ForString("hello"),
                WellKnownValue.ForNumber(42),
                WellKnownValue.ForBool(true),
                WellKnownValue.ForNull()
            )
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("field_a")]
    [InlineData("field_a,field_b.sub_field")]
    [InlineData("well_known.timestamp,well_known.wrap_bool,scalars.f_string")]
    public void Parse_WithFieldMask_ReturnsEquivalentToGoogle(string paths)
    {
        var msg = new AllWellKnown
        {
            FieldMask = paths.Length == 0 ? new FieldMask() : FieldMask.FromString(paths)
        };
        var (expected, actual) = RoundTripAndCompare(msg);
        actual.Should().BeEquivalentTo(expected, because: $"paths='{paths}'");
    }
}
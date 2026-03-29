using FluentAssertions;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerformance.UnitTests.TestData;
using Parser.Test;
using System.Globalization;
using System.Text;
using static Parser.Test.Outer.Types;
using static Parser.Test.Outer.Types.Inner.Types;
using JsonParserGoogle = Google.Protobuf.JsonParser;
using JsonParserPerf = JsonParserPerformance.JsonParser;
using WellKnownValue = Google.Protobuf.WellKnownTypes.Value;

namespace JsonParserPerformance.UnitTests;

public class JsonParserTests
{
    private static readonly TypeRegistry FullRegistry = TypeRegistry.FromMessages(
        AllScalars.Descriptor,
        AllRepeated.Descriptor,
        AllMaps.Descriptor,
        AllOneofs.Descriptor,
        Outer.Descriptor,
        AllWellKnown.Descriptor,
        WithFieldOptions.Descriptor,
        WithReserved.Descriptor,
        ParserTestRoot.Descriptor
    );

    private static JsonParserGoogle GoogleParser(TypeRegistry? registry = null) =>
        new (JsonParserGoogle.Settings.Default.WithTypeRegistry(registry ?? FullRegistry));

    private static readonly JsonFormatter Formatter =
        new (new JsonFormatter.Settings(formatDefaultValues: true, typeRegistry: FullRegistry));

    #region ParserTestRoot

    [Fact]
    public void Parse_ParserTestRootFullSample_ReturnsEquivalent()
    {
        // Arrange
        string json = Sample.Json;
        var expected = GoogleParser().Parse<ParserTestRoot>(json);

        // Act
        var result = JsonParserPerf.Parse<ParserTestRoot>(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_ParserTestRootEmpty_ReturnsEquivalent()
    {
        // Arrange
        var json = Formatter.Format(new ParserTestRoot());
        var expected = GoogleParser().Parse<ParserTestRoot>(json);

        // Act
        var result = JsonParserPerf.Parse<ParserTestRoot>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_ParserTestRootWithRepeatedScalarList_ReturnsEquivalent()
    {
        // Arrange
        var msg = new ParserTestRoot
        {
            ScalarList =
            {
                new AllScalars { FInt32 = 1, FString = "one" },
                new AllScalars { FInt32 = 2, FDouble = 3.14  },
                new AllScalars()
            }
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<ParserTestRoot>(json);

        // Act
        var result = JsonParserPerf.Parse<ParserTestRoot>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_ParserTestRootWithScalarIndexMap_ReturnsEquivalent()
    {
        // Arrange
        var msg = new ParserTestRoot
        {
            ScalarIndex =
            {
                ["id-1"] = new AllScalars { FString = "first",  FBool  = true },
                ["id-2"] = new AllScalars { FString = "second", FInt32 = 99   }
            }
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<ParserTestRoot>(json);

        // Act
        var result = JsonParserPerf.Parse<ParserTestRoot>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_ParserTestRootWithOneofIndexMap_ReturnsEquivalent()
    {
        // Arrange
        var msg = new ParserTestRoot
        {
            OneofIndex =
            {
                ["a"] = new AllOneofs { SharedField = "s1", ChoiceString = "str"      },
                ["b"] = new AllOneofs { SharedField = "s2", ChoiceEnum   = Color.Blue }
            }
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<ParserTestRoot>(json);

        // Act
        var result = JsonParserPerf.Parse<ParserTestRoot>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(Color.Unspecified)]
    [InlineData(Color.Red)]
    [InlineData(Color.Green)]
    [InlineData(Color.Blue)]
    public void Parse_ParserTestRootWithColor_ReturnsEquivalent(Color color)
    {
        // Arrange
        var json = Formatter.Format(new ParserTestRoot { Color = color });
        var expected = GoogleParser().Parse<ParserTestRoot>(json);

        // Act
        var result = JsonParserPerf.Parse<ParserTestRoot>(json);

        // Assert
        result.Should().BeEquivalentTo(expected, because: $"Color = {color}");
    }

    [Theory]
    [InlineData(Direction.Unspecified)]
    [InlineData(Direction.North)]
    [InlineData(Direction.South)]
    [InlineData(Direction.East)]
    [InlineData(Direction.West)]
    public void Parse_ParserTestRootWithDirection_ReturnsEquivalent(Direction direction)
    {
        // Arrange
        var json = Formatter.Format(new ParserTestRoot { Direction = direction });
        var expected = GoogleParser().Parse<ParserTestRoot>(json);

        // Act
        var result = JsonParserPerf.Parse<ParserTestRoot>(json);

        // Assert
        result.Should().BeEquivalentTo(expected, because: $"Direction = {direction}");
    }

    #endregion

    #region AllScalars

    [Fact]
    public void Parse_AllScalarsWithExtremeValues_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllScalars
        {
            FInt32 = int.MinValue,
            FInt64 = long.MinValue,
            FUint32 = uint.MaxValue,
            FUint64 = ulong.MaxValue,
            FSint32 = int.MinValue,
            FSint64 = long.MinValue,
            FFixed32 = uint.MaxValue,
            FFixed64 = ulong.MaxValue,
            FSfixed32 = int.MinValue,
            FSfixed64 = long.MinValue,
            FFloat = float.MaxValue,
            FDouble = double.MaxValue,
            FBool = true,
            FString = "hello protobuf \u00e9\u00e0\u00fc",
            FBytes = ByteString.CopyFromUtf8("hello world")
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllScalars>(json);

        // Act
        var result = JsonParserPerf.Parse<AllScalars>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllScalarsWithZeroAndDefaultValues_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllScalars
        {
            FInt32 = 0,
            FInt64 = 0,
            FUint32 = 0,
            FUint64 = 0,
            FSint32 = 0,
            FSint64 = 0,
            FFixed32 = 0,
            FFixed64 = 0,
            FSfixed32 = 0,
            FSfixed64 = 0,
            FFloat = 0f,
            FDouble = 0.0,
            FBool = false,
            FString = "",
            FBytes = ByteString.Empty
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllScalars>(json);

        // Act
        var result = JsonParserPerf.Parse<AllScalars>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllScalarsWithSpecialFloatValues_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllScalars
        {
            FFloat = float.NaN,
            FDouble = double.NegativeInfinity
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllScalars>(json);

        // Act
        var result = JsonParserPerf.Parse<AllScalars>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllScalarsWithEscapedStringCharacters_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllScalars
        {
            FString = "tab:\there\nnewline\r\"quoted\" \\backslash\\ \u0000null\u001Fcontrol"
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllScalars>(json);

        // Act
        var result = JsonParserPerf.Parse<AllScalars>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllScalarsWithLargeBase64Bytes_ReturnsEquivalent()
    {
        // Arrange
        var buf = new byte[4096];
        new Random(42).NextBytes(buf);

        var msg = new AllScalars { FBytes = ByteString.CopyFrom(buf) };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllScalars>(json);

        // Act
        var result = JsonParserPerf.Parse<AllScalars>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    #endregion

    #region AllRepeated

    [Fact]
    public void Parse_AllRepeatedWithPopulatedLists_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllRepeated
        {
            RInt32 = { int.MinValue, -1, 0, 1, int.MaxValue },
            RInt64 = { long.MinValue, 0, long.MaxValue },
            RUint32 = { 0, 1, uint.MaxValue },
            RUint64 = { 0, 1, ulong.MaxValue },
            RSint32 = { -100, 0, 100 },
            RSint64 = { -100, 0, 100 },
            RFixed32 = { 0, 1, uint.MaxValue },
            RFixed64 = { 0, 1, ulong.MaxValue },
            RFloat = { float.NaN, 0f, 1.5f, float.PositiveInfinity },
            RDouble = { 0.0, 1.23456789, -9.99e100 },
            RBool = { true, false, true },
            RString = { "alpha", "beta", "gamma", "", "ε∑√∞" },
            RBytes =
            {
                ByteString.CopyFromUtf8("hello"),
                ByteString.CopyFromUtf8("world"),
                ByteString.Empty
            },
            REnum = { Color.Red, Color.Green, Color.Blue, Color.Unspecified },
            RMessage =
            {
                new AllScalars { FInt32 = 1, FString = "first"  },
                new AllScalars { FInt32 = 2, FString = "second" }
            }
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllRepeated>(json);

        // Act
        var result = JsonParserPerf.Parse<AllRepeated>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllRepeatedWithEmptyLists_ReturnsEquivalent()
    {
        // Arrange
        var json = Formatter.Format(new AllRepeated());
        var expected = GoogleParser().Parse<AllRepeated>(json);

        // Act
        var result = JsonParserPerf.Parse<AllRepeated>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllRepeatedWithSingleElementLists_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllRepeated
        {
            RInt32 = { 42 },
            RString = { "solo" },
            REnum = { Color.Blue }
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllRepeated>(json);

        // Act
        var result = JsonParserPerf.Parse<AllRepeated>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllRepeatedWithLargeList_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllRepeated();
        for (int i = 0; i < 1000; i++)
        {
            msg.RInt32.Add(i - 500);
        }

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllRepeated>(json);

        // Act
        var result = JsonParserPerf.Parse<AllRepeated>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    #endregion

    #region AllMaps

    [Fact]
    public void Parse_AllMapsWithStringKeyedMaps_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllMaps
        {
            MStringString = { ["a"] = "apple", ["b"] = "banana", ["empty"] = "" },
            MStringInt32 = { ["min"] = int.MinValue, ["max"] = int.MaxValue },
            MStringInt64 = { ["x"] = long.MinValue },
            MStringUint32 = { ["u"] = uint.MaxValue },
            MStringFloat = { ["pi"] = 3.14f },
            MStringDouble = { ["e"] = 2.718281828 },
            MStringBool = { ["t"] = true, ["f"] = false },
            MStringBytes = { ["b"] = ByteString.CopyFromUtf8("bytes!") },
            MStringEnum = { ["r"] = Color.Red, ["g"] = Color.Green },
            MStringMsg = { ["s"] = new AllScalars { FInt32 = 7, FString = "nested in map" } }
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllMaps>(json);

        // Act
        var result = JsonParserPerf.Parse<AllMaps>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllMapsWithIntegerKeyedMaps_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllMaps
        {
            MInt32String = { [-1] = "neg", [0] = "zero", [1] = "pos" },
            MInt64String = { [long.MinValue] = "min64" },
            MUint32String = { [uint.MaxValue] = "maxu32" },
            MUint64String = { [ulong.MaxValue] = "maxu64" },
            MSint32String = { [-100] = "s32" },
            MSint64String = { [-100] = "s64" },
            MFixed32String = { [uint.MaxValue] = "f32" },
            MFixed64String = { [ulong.MaxValue] = "f64" },
            MBoolString = { [true] = "yes", [false] = "no" }
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllMaps>(json);

        // Act
        var result = JsonParserPerf.Parse<AllMaps>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllMapsWithEmptyMap_ReturnsEquivalent()
    {
        // Arrange
        var json = Formatter.Format(new AllMaps());
        var expected = GoogleParser().Parse<AllMaps>(json);

        // Act
        var result = JsonParserPerf.Parse<AllMaps>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllMapsWithLargeStringMap_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllMaps();
        for (int i = 0; i < 500; i++)
        {
            msg.MStringString[$"key_{i:D4}"] = $"value_{i}";
        }

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllMaps>(json);

        // Act
        var result = JsonParserPerf.Parse<AllMaps>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    #endregion

    #region AllOneofs

    [Theory]
    [MemberData(nameof(OneofsVariants))]
    public void Parse_AllOneofs_EachVariant_ReturnsEquivalent(AllOneofs msg)
    {
        // Arrange
        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllOneofs>(json);

        // Act
        var result = JsonParserPerf.Parse<AllOneofs>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    public static IEnumerable<object[]> OneofsVariants()
    {
        yield return [new AllOneofs { SharedField = "shared", ChoiceInt32 = int.MinValue }];
        yield return [new AllOneofs { SharedField = "shared", ChoiceDouble = double.MaxValue }];
        yield return [new AllOneofs { SharedField = "shared", ChoiceBool = true }];
        yield return [new AllOneofs { SharedField = "shared", ChoiceString = "a string choice" }];
        yield return [new AllOneofs { SharedField = "shared", ChoiceBytes = ByteString.CopyFromUtf8("bytes choice") }];
        yield return [new AllOneofs { SharedField = "shared", ChoiceEnum = Color.Blue }];
        yield return [new AllOneofs { SharedField = "shared", ChoiceMessage = new AllScalars { FInt32 = 99, FBool = true } }];
        yield return [new AllOneofs { SharedField = "only shared" }];
    }

    #endregion

    #region Outer (Nested Messages)

    [Fact]
    public void Parse_OuterWithFullyNestedMessage_ReturnsEquivalent()
    {
        // Arrange
        var msg = new Outer
        {
            OuterField = "outer",
            InnerEnum = InnerEnum.A,
            Inner = new Inner
            {
                InnerField = "inner",
                Deep = new DeepInner { DeepField = 999 }
            }
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<Outer>(json);

        // Act
        var result = JsonParserPerf.Parse<Outer>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(InnerEnum.Unspecified)]
    [InlineData(InnerEnum.A)]
    [InlineData(InnerEnum.B)]
    public void Parse_OuterWithInnerEnumValue_ReturnsEquivalent(InnerEnum innerEnum)
    {
        // Arrange
        var json = Formatter.Format(new Outer { InnerEnum = innerEnum });
        var expected = GoogleParser().Parse<Outer>(json);

        // Act
        var result = JsonParserPerf.Parse<Outer>(json);

        // Assert
        result.Should().BeEquivalentTo(expected, because: $"InnerEnum = {innerEnum}");
    }

    #endregion

    #region AllWellKnown

    [Fact]
    public void Parse_AllWellKnownWithAllFieldsPopulated_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllWellKnown
        {
            Timestamp = Timestamp.FromDateTime(DateTime.Parse("2024-03-15T08:30:00.500Z", CultureInfo.InvariantCulture).ToUniversalTime()),
            Duration = Duration.FromTimeSpan(TimeSpan.FromSeconds(3600.5)),

            WrapBool = true,
            WrapInt32 = -42,
            WrapInt64 = 9007199254740992L,
            WrapUint32 = 4000000000,
            WrapUint64 = 18000000000000000000UL,
            WrapFloat = 2.71f,
            WrapDouble = 1.4142135623730951,
            WrapString = "nullable string",
            WrapBytes = ByteString.CopyFrom("wrapped", Encoding.UTF8),

            AnyField = Any.Pack(new AllScalars { FString = "in any", FInt32 = 7, FBool = true }),

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
            },

            ValueField = WellKnownValue.ForList(
                WellKnownValue.ForString("hello"),
                WellKnownValue.ForNumber(42),
                WellKnownValue.ForBool(true),
                WellKnownValue.ForNull()
            ),

            FieldMask = FieldMask.FromString("well_known.timestamp,well_known.wrap_bool,scalars.f_string")
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllWellKnown>(json);

        // Act
        var result = JsonParserPerf.Parse<AllWellKnown>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllWellKnownWithNullableWrappersUnset_ReturnsEquivalent()
    {
        // Arrange
        var json = Formatter.Format(new AllWellKnown());
        var expected = GoogleParser().Parse<AllWellKnown>(json);

        // Act
        var result = JsonParserPerf.Parse<AllWellKnown>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllWellKnownWithZeroDuration_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllWellKnown { Duration = Duration.FromTimeSpan(TimeSpan.Zero) };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllWellKnown>(json);

        // Act
        var result = JsonParserPerf.Parse<AllWellKnown>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllWellKnownWithNegativeDuration_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllWellKnown { Duration = Duration.FromTimeSpan(TimeSpan.FromSeconds(-90.5)) };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllWellKnown>(json);

        // Act
        var result = JsonParserPerf.Parse<AllWellKnown>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllWellKnownWithEpochTimestamp_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllWellKnown { Timestamp = Timestamp.FromDateTime(DateTime.UnixEpoch.ToUniversalTime()) };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllWellKnown>(json);

        // Act
        var result = JsonParserPerf.Parse<AllWellKnown>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_AllWellKnownWithAnyPackingRepeatedMessage_ReturnsEquivalent()
    {
        // Arrange
        var msg = new AllWellKnown
        {
            AnyField = Any.Pack(new AllRepeated
            {
                RString = { "x", "y", "z" },
                REnum = { Color.Red, Color.Blue }
            })
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<AllWellKnown>(json);

        // Act
        var result = JsonParserPerf.Parse<AllWellKnown>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    #endregion

    #region WithFieldOptions

    [Fact]
    public void Parse_WithFieldOptionsWithAllFieldsSet_ReturnsEquivalent()
    {
        // Arrange
        var msg = new WithFieldOptions
        {
            ActiveField = "active",
            OldField = "deprecated but parseable",
            PackedInts = { -100, -1, 0, 1, 100, int.MaxValue }
        };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<WithFieldOptions>(json);

        // Act
        var result = JsonParserPerf.Parse<WithFieldOptions>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithFieldOptionsWithEmptyPackedInts_ReturnsEquivalent()
    {
        // Arrange
        var msg = new WithFieldOptions { ActiveField = "only active" };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<WithFieldOptions>(json);

        // Act
        var result = JsonParserPerf.Parse<WithFieldOptions>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    #endregion

    #region WithReserved

    [Fact]
    public void Parse_WithReservedWithCurrentField_ReturnsEquivalent()
    {
        // Arrange
        var msg = new WithReserved { CurrentField = "only valid field" };

        var json = Formatter.Format(msg);
        var expected = GoogleParser().Parse<WithReserved>(json);

        // Act
        var result = JsonParserPerf.Parse<WithReserved>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    #endregion

    #region FloatTest
    [Fact]
    public void Parse_WithSpecialFloatValues_ReturnsEquivalent()
    {
        // Arrange
        var floatTest = new FloatTest
        {
            FFloat = float.NaN,
            FDouble = double.NaN
        };

        var json = Formatter.Format(floatTest);
        var expected = GoogleParser().Parse<FloatTest>(json);

        // Act
        var result = JsonParserPerf.Parse<FloatTest>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithInfinityValues_ReturnsEquivalent()
    {
        // Arrange
        var floatTest = new FloatTest
        {
            FFloat = float.PositiveInfinity,
            FDouble = double.PositiveInfinity
        };

        var json = Formatter.Format(floatTest);
        var expected = GoogleParser().Parse<FloatTest>(json);

        // Act
        var result = JsonParserPerf.Parse<FloatTest>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithNegativeInfinityValues_ReturnsEquivalent()
    {
        // Arrange
        var floatTest = new FloatTest
        {
            FFloat = float.NegativeInfinity,
            FDouble = double.NegativeInfinity
        };

        var json = Formatter.Format(floatTest);
        var expected = GoogleParser().Parse<FloatTest>(json);

        // Act
        var result = JsonParserPerf.Parse<FloatTest>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_WithMixedSpecialValues_ReturnsEquivalent()
    {
        // Arrange
        var json = @"{
            ""fFloat"": ""NaN"",
            ""fDouble"": ""Infinity""
        }";

        var expected = GoogleParser().Parse<FloatTest>(json);

        // Act
        var result = JsonParserPerf.Parse<FloatTest>(json);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
    #endregion
}
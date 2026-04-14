using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Parser.Test;
using System.Globalization;
using System.Text;
using static Parser.Test.Outer.Types;
using static Parser.Test.Outer.Types.Inner.Types;

namespace JsonParserPerformance.UnitTests.Helpers;

public static class MessageBuilders
{
    #region AllScalars
    public static AllScalars AllScalars_Extreme() => new ()
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

    public static AllScalars AllScalars_Zero() => new ()
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

    public static AllScalars AllScalars_SpecialFloats() => new ()
    {
        FFloat = float.NaN,
        FDouble = double.NegativeInfinity
    };

    public static AllScalars AllScalars_EscapedString() => new ()
    {
        FString = "tab:\there\nnewline\r\"quoted\" \\backslash\\ \u0000null\u001Fcontrol"
    };

    public static AllScalars AllScalars_LargeBytes(int size = 4096, int seed = 42)
    {
        var buf = new byte[size];
        new Random(seed).NextBytes(buf);
        return new AllScalars { FBytes = ByteString.CopyFrom(buf) };
    }

    public static AllScalars AllScalars_Unicode() => new ()
    {
        FString = "日本語 Ελληνικά 🎉🔥💯"
    };
    #endregion

    #region AllRepeated
    public static AllRepeated AllRepeated_Full() => new ()
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

    public static AllRepeated AllRepeated_Large(int count = 1000)
    {
        var msg = new AllRepeated();
        for (int i = 0; i < count; i++)
            msg.RInt32.Add(i - count / 2);
        return msg;
    }

    public static AllRepeated AllRepeated_SingleElement() => new ()
    {
        RInt32 = { 42 },
        RString = { "solo" },
        REnum = { Color.Blue }
    };
    #endregion

    #region AllMaps
    public static AllMaps AllMaps_StringKeyed() => new ()
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

    public static AllMaps AllMaps_IntegerKeyed() => new ()
    {
        MInt32String = { [-1] = "neg", [0] = "zero", [1] = "pos" },
        MInt64String = { [long.MinValue] = "min64" },
        MUint32String = { [uint.MaxValue] = "maxu32" },
        MUint64String = { [ulong.MaxValue] = "maxu64" },
        MSint32String = { [-100] = "s32" },
        MSint64String = { [-100L] = "s64" },
        MFixed32String = { [uint.MaxValue] = "f32" },
        MFixed64String = { [ulong.MaxValue] = "f64" },
        MBoolString = { [true] = "yes", [false] = "no" }
    };

    public static AllMaps AllMaps_Large(int count = 500)
    {
        var msg = new AllMaps();
        for (int i = 0; i < count; i++)
            msg.MStringString[$"key_{i:D4}"] = $"value_{i}";
        return msg;
    }
    #endregion

    #region AllWellKnown
    public static AllWellKnown AllWellKnown_Full() => new ()
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
                ["strKey"]  = Value.ForString("strValue"),
                ["numKey"]  = Value.ForNumber(123.45),
                ["boolKey"] = Value.ForBool(true),
                ["nullKey"] = Value.ForNull(),
                ["listKey"] = Value.ForList(Value.ForNumber(1), Value.ForString("two")),
                ["objKey"]  = Value.ForStruct(new Struct
                {
                    Fields = { ["nested"] = Value.ForBool(false) }
                })
            }
        },
        ValueField = Value.ForList(
            Value.ForString("hello"),
            Value.ForNumber(42),
            Value.ForBool(true),
            Value.ForNull()
        ),
        FieldMask = FieldMask.FromString("well_known.timestamp,well_known.wrap_bool,scalars.f_string")
    };

    public static AllWellKnown AllWellKnown_ZeroDuration() => new ()
    {
        Duration = Duration.FromTimeSpan(TimeSpan.Zero)
    };

    public static AllWellKnown AllWellKnown_NegativeDuration() => new ()
    {
        Duration = Duration.FromTimeSpan(TimeSpan.FromSeconds(-90.5))
    };

    public static AllWellKnown AllWellKnown_EpochTimestamp() => new ()
    {
        Timestamp = Timestamp.FromDateTime(DateTime.UnixEpoch.ToUniversalTime())
    };

    public static AllWellKnown AllWellKnown_AnyWithRepeated() => new ()
    {
        AnyField = Any.Pack(new AllRepeated
        {
            RString = { "x", "y", "z" },
            REnum = { Color.Red, Color.Blue }
        })
    };
    #endregion

    #region FloatTest
    public static FloatTest FloatTest_NaN() => new () { FFloat = float.NaN, FDouble = double.NaN };
    public static FloatTest FloatTest_PosInfinity() => new () { FFloat = float.PositiveInfinity, FDouble = double.PositiveInfinity };
    public static FloatTest FloatTest_NegInfinity() => new () { FFloat = float.NegativeInfinity, FDouble = double.NegativeInfinity };
    #endregion

    public static IEnumerable<AllOneofs> AllOneofs_AllVariants()
    {
        yield return new AllOneofs { SharedField = "shared", ChoiceInt32 = int.MinValue };
        yield return new AllOneofs { SharedField = "shared", ChoiceDouble = double.MaxValue };
        yield return new AllOneofs { SharedField = "shared", ChoiceBool = true };
        yield return new AllOneofs { SharedField = "shared", ChoiceString = "a string choice" };
        yield return new AllOneofs { SharedField = "shared", ChoiceBytes = ByteString.CopyFromUtf8("bytes choice") };
        yield return new AllOneofs { SharedField = "shared", ChoiceEnum = Color.Blue };
        yield return new AllOneofs { SharedField = "shared", ChoiceMessage = new AllScalars { FInt32 = 99, FBool = true } };
        yield return new AllOneofs { SharedField = "only shared" };
    }

    public static Outer Outer_FullyNested() => new ()
    {
        OuterField = "outer",
        InnerEnum = InnerEnum.A,
        Inner = new Inner
        {
            InnerField = "inner",
            Deep = new DeepInner { DeepField = 999 }
        }
    };

    public static ParserTestRoot ParserTestRoot_Full() => new ()
    {
        Id = "test-parser-id",
        Label = "Test Parser",
        Color = Color.Red,
        Direction = Direction.North,
        CreatedAt = Timestamp.FromDateTime(
            DateTime.Parse("2024-06-01T12:00:00Z", CultureInfo.InvariantCulture).ToUniversalTime()),
        UpdateMask = FieldMask.FromString("scalars.f_string,color,label"),
        Extra = Any.Pack(new AllScalars { FString = "packed inside Any", FInt32 = 42 }),
        Scalars = AllScalars_Extreme(),
        RepeatedFields = AllRepeated_Full(),
        Maps = AllMaps_StringKeyed(),
        Oneofs = new AllOneofs
        {
            SharedField = "I am always present",
            ChoiceString = "scalar_choice is set to string variant",
            ChoiceEnum = Color.Green
        },
        Nested = Outer_FullyNested(),
        WellKnown = AllWellKnown_Full(),
        FieldOptions = new WithFieldOptions
        {
            ActiveField = "I am active",
            OldField = "I am deprecated but still parseable",
            PackedInts = { -100, -1, 0, 1, 100, int.MaxValue }
        },
        ReservedDemo = new WithReserved { CurrentField = "only non-reserved field" }
    };
}
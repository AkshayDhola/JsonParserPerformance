using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Parser.Test;
using System.Globalization;
using System.Text;
using static Parser.Test.Outer.Types;
using static Parser.Test.Outer.Types.Inner.Types;

namespace JsonParserPerformance.UnitTests.TestData
{
    public static class Sample
    {

        private static readonly TypeRegistry Registry = TypeRegistry.FromMessages(AllScalars.Descriptor, ParserTestRoot.Descriptor);
        private static readonly JsonFormatter Formatter = new (new JsonFormatter.Settings(formatDefaultValues: true, typeRegistry: Registry));

        private static readonly ParserTestRoot _testee = new ()
        {
            Id = "test-parser-id",
            Label = "Test Parser",
            Color = Color.Red,
            Direction = Direction.North,

            CreatedAt = Timestamp.FromDateTime(
                DateTime.Parse("2024-06-01T12:00:00Z", CultureInfo.InvariantCulture).ToUniversalTime()
            ),

            UpdateMask = FieldMask.FromString("scalars.f_string,color,label"),

            Extra = Any.Pack(new AllScalars
            {
                FString = "packed inside Any",
                FInt32 = 42
            }),

            Scalars = new AllScalars
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
                FFloat = 3.14f,
                FDouble = 2.718281828459045,
                FBool = true,
                FString = "hello protobuf",
                FBytes = ByteString.CopyFromUtf8("hello world")
            },

            RepeatedFields = new AllRepeated
            {
                RInt32 = { -1, 0, 1, int.MaxValue },
                RInt64 = { long.MinValue, 0, long.MaxValue },
                RUint32 = { 0, 1, uint.MaxValue },
                RUint64 = { 0, 1, ulong.MaxValue },
                RSint32 = { -100, 0, 100 },
                RSint64 = { -100, 0, 100 },
                RFixed32 = { 0, 1, uint.MaxValue },
                RFixed64 = { 0, 1, ulong.MaxValue },
                RFloat = { 0f, 1.5f, -3.14f, 1e10f },
                RDouble = { 0.0, 1.23456789, -9.99e100 },
                RBool = { true, false, true },
                RString = { "alpha", "beta", "gamma", "" },
                RBytes =
            {
                ByteString.CopyFromUtf8("hello"),
                ByteString.CopyFromUtf8("world"),
                ByteString.Empty
            },
                REnum = { Color.Red, Color.Green, Color.Blue, Color.Unspecified }
            },

            Maps = new AllMaps
            {
                MStringString = { ["key_a"] = "value_a", ["key_b"] = "value_b", ["empty"] = "" }
            },

            Oneofs = new AllOneofs
            {
                SharedField = "I am always present",
                ChoiceString = "scalar_choice is set to string variant",
                ChoiceEnum = Color.Green
            },

            Nested = new Outer { OuterField = "outer value", InnerEnum = InnerEnum.A, Inner = new Inner() { InnerField = "inner value", Deep = new DeepInner() { DeepField = 99 } } },

            WellKnown = new AllWellKnown
            {
                Timestamp = Timestamp.FromDateTime(
                    DateTime.Parse("2024-03-15T08:30:00.500Z", CultureInfo.InvariantCulture).ToUniversalTime()
                ),
                Duration = Duration.FromTimeSpan(TimeSpan.FromSeconds(3600.5)),

                WrapBool = true,
                WrapInt32 = -42,
                WrapInt64 = 9007199254740992,
                WrapUint32 = 4000000000,
                WrapUint64 = 18000000000000000000,
                WrapFloat = 2.71f,
                WrapDouble = 1.4142135623730951,
                WrapString = "nullable string value",
                WrapBytes = ByteString.CopyFrom("wrapped", Encoding.UTF8),

                AnyField = Any.Pack(new AllScalars
                {
                    FString = "encoded inside Any",
                    FInt32 = 7,
                    FBool = true
                }),

                StructField = new Struct
                {
                    Fields =
                {
                    ["strKey"] = Value.ForString("strValue"),
                    ["numKey"] = Value.ForNumber(123.45),
                    ["boolKey"] = Value.ForBool(true),
                    ["nullKey"] = Value.ForNull()
                }
                },

                ValueField = Value.ForList(
                    Value.ForString("hello"),
                    Value.ForNumber(42),
                    Value.ForBool(true),
                    Value.ForNull()
                ),

                FieldMask = FieldMask.FromString(
                    "well_known.timestamp,well_known.wrap_bool,scalars.f_string"
                )
            },

            FieldOptions = new WithFieldOptions
            {
                ActiveField = "I am active",
                OldField = "I am deprecated but still parseable",
                PackedInts = { -100, -1, 0, 1, 100, int.MaxValue }
            },

            ReservedDemo = new WithReserved
            {
                CurrentField = "only non-reserved field"
            }
        };

        public static readonly string Json = Formatter.Format(_testee);
    }
}

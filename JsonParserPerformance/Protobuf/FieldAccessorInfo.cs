using Google.Protobuf;
using Google.Protobuf.Reflection;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace JsonParserPerformance.Protobuf;

internal enum WellKnownType : byte
{
    None, Timestamp, Duration,
    BoolValue, Int32Value, Int64Value, UInt32Value, UInt64Value,
    FloatValue, DoubleValue, StringValue, BytesValue,
    FieldMask, Empty, Any, Struct, Value, ListValue,
}

internal sealed class FieldAccessorInfo
{
    private static readonly FrozenDictionary<string, WellKnownType> _wellKnownTypes =
    new Dictionary<string, WellKnownType>
    {
        ["google.protobuf.Timestamp"] = WellKnownType.Timestamp,
        ["google.protobuf.Duration"] = WellKnownType.Duration,
        ["google.protobuf.BoolValue"] = WellKnownType.BoolValue,
        ["google.protobuf.Int32Value"] = WellKnownType.Int32Value,
        ["google.protobuf.Int64Value"] = WellKnownType.Int64Value,
        ["google.protobuf.UInt32Value"] = WellKnownType.UInt32Value,
        ["google.protobuf.UInt64Value"] = WellKnownType.UInt64Value,
        ["google.protobuf.FloatValue"] = WellKnownType.FloatValue,
        ["google.protobuf.DoubleValue"] = WellKnownType.DoubleValue,
        ["google.protobuf.StringValue"] = WellKnownType.StringValue,
        ["google.protobuf.BytesValue"] = WellKnownType.BytesValue,
        ["google.protobuf.FieldMask"] = WellKnownType.FieldMask,
        ["google.protobuf.Empty"] = WellKnownType.Empty,
        ["google.protobuf.Any"] = WellKnownType.Any,
        ["google.protobuf.Struct"] = WellKnownType.Struct,
        ["google.protobuf.Value"] = WellKnownType.Value,
        ["google.protobuf.ListValue"] = WellKnownType.ListValue,
    }.ToFrozenDictionary();

    public readonly FieldDescriptor Field;
    public readonly IFieldAccessor Accessor;
    public readonly FieldType FieldType;
    public readonly bool IsMap;
    public readonly bool IsRepeated;
    public readonly WellKnownType Wkt;
    public readonly Type? ClrType;

    public FieldDescriptor? MapKeyField;
    public FieldAccessorInfo? MapValueMeta;

    public FieldAccessorInfo(FieldDescriptor f)
    {
        Field = f;
        Accessor = f.Accessor;
        FieldType = f.FieldType;
        IsMap = f.IsMap;
        IsRepeated = f.IsRepeated;

        if (f.FieldType == FieldType.Message && !_wellKnownTypes.TryGetValue(f.MessageType.FullName, out Wkt))
        {
            ClrType = f.MessageType.ClrType;
        }

        if (IsMap)
        {
            MapKeyField = f.MessageType.FindFieldByNumber(1);
            var valueField = f.MessageType.FindFieldByNumber(2);
            MapValueMeta = new FieldAccessorInfo(valueField);
        }
    }

    /// <summary>
    /// Sets the value of the specified field on the given message instance.
    /// </summary>
    /// <param name="message">The message object whose field value is to be set. Cannot be null.</param>
    /// <param name="value">The value to assign to the field. The type and constraints depend on the field being set; may be null for
    /// reference or nullable types.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetValue(IMessage message, object? value) => Accessor.SetValue(message, value);

    /// <summary>
    /// Gets the value of the field from the specified message.
    /// </summary>
    /// <param name="message">The message instance from which to retrieve the field value. Cannot be null.</param>
    /// <returns>The value of the field as an object, or null if the field is not set in the message.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetValue(IMessage message) => Accessor.GetValue(message);
}

using Google.Protobuf;
using Google.Protobuf.Reflection;
using JsonParserPerformance.Protobuf;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace JsonParserPerformance.Helpers;

internal static class MessageParser
{
    /// <summary>
    /// Parses a JSON object from the specified reader and populates the fields of the given message using the provided
    /// field lookup.
    /// </summary>
    /// <param name="message">The message instance to populate with values parsed from the JSON object.</param>
    /// <param name="fieldLookup">The field lookup used to resolve field accessors for mapping JSON property names to message fields.</param>
    /// <param name="reader">A reference to the JSON reader positioned at the start of the object to parse. The reader is advanced as fields
    /// are read.</param>
    /// <exception cref="JsonException">Thrown if the JSON structure is invalid, such as when the reader is not positioned at a start object or a
    /// property name is expected but not found.</exception>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void ParseObject(IMessage message, FieldLookup fieldLookup, ref Utf8JsonReader reader)
    {
        // Reader should already be at StartObject (positioned by caller)
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject, got {reader.TokenType}");
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName");
            }

            var fieldAccessorInfo = fieldLookup.Find(reader.ValueSpan);

            if (fieldAccessorInfo == null)
            {
                reader.Skip();
                continue;
            }

            reader.Read();

            if (fieldAccessorInfo.IsMap)
            {
                ParseMap(message, fieldAccessorInfo, ref reader);
            }
            else if (fieldAccessorInfo.IsRepeated)
            {
                ParseRepeated(message, fieldAccessorInfo, ref reader);
            }
            else
            {
                fieldAccessorInfo.SetValue(message, ParseValue(fieldAccessorInfo, ref reader));
            }
        }
    }

    /// <summary>
    /// Parses a value from the provided JSON reader according to the specified field accessor information.
    /// </summary>
    /// <param name="fieldAccessorInfo">The metadata describing the field to parse, including its type and any relevant field information.</param>
    /// <param name="reader">A reference to the JSON reader positioned at the value to parse. The reader will be advanced past the parsed
    /// value.</param>
    /// <returns>An object representing the parsed value, typed according to the field information. Returns null if the value is
    /// null in the JSON input.</returns>
    /// <exception cref="NotSupportedException">Thrown if the field type specified in fieldAccessorInfo is not supported for parsing.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object? ParseValue(FieldAccessorInfo fieldAccessorInfo, ref Utf8JsonReader reader)
    {
        return fieldAccessorInfo.FieldType switch
        {
            FieldType.Int32 or FieldType.SInt32 or FieldType.SFixed32 => PrimitiveParser.GetNumeric<int>(ref reader),
            FieldType.Int64 or FieldType.SInt64 or FieldType.SFixed64 => PrimitiveParser.GetNumeric<long>(ref reader),
            FieldType.UInt32 or FieldType.Fixed32 => PrimitiveParser.GetNumeric<uint>(ref reader),
            FieldType.UInt64 or FieldType.Fixed64 => PrimitiveParser.GetNumeric<ulong>(ref reader),
            FieldType.Float => PrimitiveParser.GetNumeric<float>(ref reader),
            FieldType.Double => PrimitiveParser.GetNumeric<double>(ref reader),
            FieldType.Bool => PrimitiveParser.GetBool(ref reader),
            FieldType.String => reader.GetString(),
            FieldType.Bytes => PrimitiveParser.ParseBytes(ref reader),
            FieldType.Enum => PrimitiveParser.ParseEnum(fieldAccessorInfo.Field, ref reader),
            FieldType.Message => ParseMessage(fieldAccessorInfo, ref reader),
            _ => throw new NotSupportedException($"Unsupported: {fieldAccessorInfo.FieldType}")
        };
    }

    /// <summary>
    /// Parses a repeated field from the current JSON array in the reader and adds the parsed values to the specified
    /// message instance.
    /// </summary>
    /// <param name="message">The message instance to which the parsed repeated field values will be added.</param>
    /// <param name="fieldAccessorInfo">The field accessor information describing the repeated field to parse and set.</param>
    /// <param name="reader">The JSON reader positioned at the start of the array to parse. The reader is advanced as values are read.</param>
    /// <exception cref="JsonException">Thrown if the current token in the reader is not the start of a JSON array.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseRepeated(IMessage message, FieldAccessorInfo fieldAccessorInfo, ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected array");
        }

        var list = (IList)fieldAccessorInfo.GetValue(message);
        if (fieldAccessorInfo.FieldType == FieldType.Message)
        {
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                list.Add(ParseMessage(fieldAccessorInfo, ref reader));
    }
        }
        else
        {
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                list.Add(ParseValue(fieldAccessorInfo, ref reader));
            }
        }
    }

    /// <summary>
    /// Parses a JSON object from the reader and populates the map field of the specified message with the parsed
    /// key-value pairs.
    /// </summary>
    /// <param name="message">The message instance whose map field will be populated.</param>
    /// <param name="fieldAccessorInfo">The accessor information for the map field to be populated, including metadata about the key and value types.</param>
    /// <param name="reader">The JSON reader positioned at the start of the object to parse. The reader is advanced as the map is read.</param>
    /// <exception cref="JsonException">Thrown if the JSON does not represent a valid object or if a property name is not found where expected.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseMap(IMessage message, FieldAccessorInfo fieldAccessorInfo, ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected object for map");
        }

        var map = fieldAccessorInfo.GetValue(message);
        var adder = ParserCache.GetOrAddMapAdder(map.GetType());

        var keyField = fieldAccessorInfo.MapKeyField!;
        var valueMeta = fieldAccessorInfo.MapValueMeta!;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name");
    }

            var key = keyField.FieldType == FieldType.String ?
                        reader.GetString() :
                        ParseMapKey(keyField.FieldType, ref reader);

            reader.Read();
            var value = ParseValue(valueMeta, ref reader);

            adder(map, key, value);
        }
    }

    /// <summary>
    /// Parses a string representation of a map key into the appropriate .NET type based on the specified field
    /// descriptor.
    /// </summary>
    /// <param name="keyField">The field descriptor that defines the type of the map key to parse. Must specify a supported key type.</param>
    /// <param name="key">The string value representing the map key to be parsed.</param>
    /// <returns>An object containing the parsed key value, converted to the type specified by the field descriptor.</returns>
    /// <exception cref="NotSupportedException">Thrown if the field descriptor specifies a map key type that is not supported.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object ParseMapKey(FieldType fieldType, ref Utf8JsonReader reader) =>
        fieldType switch
    {
            FieldType.Int32 or FieldType.SInt32 or FieldType.SFixed32
                => PrimitiveParser.GetNumeric<int>(ref reader),

            FieldType.Int64 or FieldType.SInt64 or FieldType.SFixed64
                => PrimitiveParser.GetNumeric<long>(ref reader),

            FieldType.UInt32 or FieldType.Fixed32
                => PrimitiveParser.GetNumeric<uint>(ref reader),

            FieldType.UInt64 or FieldType.Fixed64
                => PrimitiveParser.GetNumeric<ulong>(ref reader),

            FieldType.Bool
                => PrimitiveParser.GetBool(ref reader),

            _ => throw new NotSupportedException($"Invalid map key type: {fieldType}")
        };

    /// <summary>
    /// Parses a JSON value from the reader as a well-known Protobuf type or a nested message, based on the provided
    /// field accessor information.
    /// </summary>
    /// <param name="fieldAccessorInfo">The field accessor information that specifies the expected Protobuf well-known type or message type to parse.</param>
    /// <param name="reader">The JSON reader positioned at the value to parse. The reader is advanced past the parsed value.</param>
    /// <returns>An object representing the parsed value, which may be a well-known Protobuf type, a primitive value, or a nested
    /// message. Returns null if the JSON token is null.</returns>
    /// <exception cref="JsonException">Thrown if the JSON token is not valid for the expected message type, such as when a nested message does not
    /// start with a JSON object.</exception>
    private static object? ParseMessage(FieldAccessorInfo fieldAccessorInfo, ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        switch (fieldAccessorInfo.Wkt)
        {
            case WellKnownType.Timestamp: return WellKnownParser.ParseTimestamp(ref reader);
            case WellKnownType.Duration: return WellKnownParser.ParseDuration(ref reader);
            case WellKnownType.BoolValue: return PrimitiveParser.GetBool(ref reader);
            case WellKnownType.Int32Value: return PrimitiveParser.GetNumeric<int>(ref reader);
            case WellKnownType.Int64Value: return PrimitiveParser.GetNumeric<long>(ref reader);
            case WellKnownType.UInt32Value: return PrimitiveParser.GetNumeric<uint>(ref reader);
            case WellKnownType.UInt64Value: return PrimitiveParser.GetNumeric<ulong>(ref reader);
            case WellKnownType.FloatValue: return PrimitiveParser.GetNumeric<float>(ref reader);
            case WellKnownType.DoubleValue: return PrimitiveParser.GetNumeric<double>(ref reader);
            case WellKnownType.StringValue: return reader.GetString();
            case WellKnownType.BytesValue: return ByteString.FromBase64(reader.GetString());
            case WellKnownType.FieldMask: return WellKnownParser.ParseFieldMask(ref reader);
            case WellKnownType.Empty:
                reader.Skip();
                return new Google.Protobuf.WellKnownTypes.Empty();
            case WellKnownType.Any: return ParseAny(ref reader);
            case WellKnownType.Struct: return WellKnownParser.ParseStruct(ref reader);
            case WellKnownType.Value: return WellKnownParser.ParseValue(ref reader);
            case WellKnownType.ListValue: return WellKnownParser.ParseListValue(ref reader);
        }

        // Regular nested message - reader is already at StartObject
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject for nested message, got {reader.TokenType}");
        }

        var msg = (IMessage)Activator.CreateInstance(fieldAccessorInfo.ClrType!)!;
        var msgMeta = ParserCache.GetOrCreateFieldLookup(msg);

        ParseObject(msg, msgMeta, ref reader);
        return msg;
    }

    /// <summary>
    /// Parses a JSON representation of a Protobuf Any message from the provided JSON reader.
    /// </summary>
    /// <param name="reader">The JSON reader positioned at the start of the Any message to parse. The reader is advanced to the end of the
    /// Any message upon completion.</param>
    /// <returns>A Google.Protobuf.WellKnownTypes.Any instance containing the parsed type URL and value from the JSON input.</returns>
    /// <exception cref="JsonException">Thrown if the JSON does not contain a required '@type' property or if the type cannot be resolved.</exception>
    private static Google.Protobuf.WellKnownTypes.Any ParseAny(ref Utf8JsonReader reader)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("@type", out var typeProp))
        {
            throw new JsonException("Missing @type in Any");
        }

        var typeUrl = typeProp.GetString() ?? "";
        var any = new Google.Protobuf.WellKnownTypes.Any { TypeUrl = typeUrl };

        if (root.TryGetProperty("value", out var vp) && vp.ValueKind == JsonValueKind.String)
        {
            any.Value = ByteString.FromBase64(vp.GetString());
            return any;
        }

        var slash = typeUrl.LastIndexOf('/');
        var typeName = slash >= 0 ? typeUrl[(slash + 1)..] : typeUrl;
        var clrType = ParserCache.GetOrAddAnyType(typeName) ?? throw new JsonException($"Cannot resolve: {typeName}");

        var innerMsg = (IMessage)Activator.CreateInstance(clrType)!;
        var innerMeta = ParserCache.GetOrCreateFieldLookup(innerMsg);

        var buf = ArrayBufferWriterPool.Rent();
        try
        {
            using (var wtr = new Utf8JsonWriter(buf))
            {
                wtr.WriteStartObject();
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.NameEquals("@type"u8))
                    {
                        continue;
                    }

                    prop.WriteTo(wtr);
                }

                wtr.WriteEndObject();
            }

            var innerReader = new Utf8JsonReader(buf.WrittenSpan);
            innerReader.Read();
            ParseObject(innerMsg, innerMeta, ref innerReader);
        }
        finally
        {
            ArrayBufferWriterPool.Return(buf);
        }

        any.Value = innerMsg.ToByteString();
        return any;
    }
}

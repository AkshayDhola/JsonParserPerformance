using System.Globalization;
using System.Text.Json;

namespace JsonParserPerformance.Helpers;

internal static class WellKnownParser
{
    /// <summary>
    /// Parses an ISO 8601 timestamp string from the current JSON token and returns a corresponding Timestamp value.
    /// </summary>
    /// <param name="reader">The JSON reader positioned at a string token containing an ISO 8601 timestamp.</param>
    /// <returns>A Timestamp representing the parsed date and time in UTC.</returns>
    /// <exception cref="JsonException">Thrown if the current JSON token is null, empty, or not a valid ISO 8601 timestamp string.</exception>
    public static Google.Protobuf.WellKnownTypes.Timestamp ParseTimestamp(ref Utf8JsonReader reader)
    {
        string? dateStr = reader.GetString();

        if (string.IsNullOrWhiteSpace(dateStr))
        {
            throw new JsonException("Expected a non-empty ISO 8601 timestamp string.");
        }

        var dt = DateTime.Parse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        return Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dt.ToUniversalTime());
    }

    /// <summary>
    /// Parses a JSON string representing a duration in seconds and returns a corresponding Duration object.
    /// </summary>
    /// <param name="reader">The JSON reader positioned at a string value representing the duration in seconds, with an 's' suffix (e.g.,
    /// "123.45s").</param>
    /// <returns>A Duration object representing the parsed duration value.</returns>
    /// <exception cref="JsonException">Thrown if the JSON value is null, empty, or not a valid duration string.</exception>
    public static Google.Protobuf.WellKnownTypes.Duration ParseDuration(ref Utf8JsonReader reader)
    {
        var str = reader.GetString();

        if (string.IsNullOrWhiteSpace(str))
        {
            throw new JsonException("Expected a non-empty duration string.");
        }

        var secs = double.Parse(str.AsSpan(0, str.Length - 1), CultureInfo.InvariantCulture);
        return Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(TimeSpan.FromSeconds(secs));
    }

    /// <summary>
    /// Parses a JSON string representing a field mask and returns a corresponding FieldMask instance.
    /// </summary>
    /// <param name="reader">A reference to the Utf8JsonReader positioned at a JSON string token containing the field mask paths,
    /// separated by commas.</param>
    /// <returns>A FieldMask instance containing the parsed field paths. If the input is not a string or is empty, an empty
    /// FieldMask is returned.</returns>
    public static Google.Protobuf.WellKnownTypes.FieldMask ParseFieldMask(ref Utf8JsonReader reader)
    {
        var mask = new Google.Protobuf.WellKnownTypes.FieldMask();
        if (reader.TokenType != JsonTokenType.String)
        {
            return mask;
        }

        var raw = reader.GetString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return mask;
        }

        Span<char> buffer = stackalloc char[256];
        foreach (var path in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            int pos = 0;
            bool first = true;
            foreach (var seg in path.Split('.'))
            {
                if (!first && pos < buffer.Length)
                {
                    buffer[pos++] = '.';
                }

                first = false;
                foreach (var c in seg)
                {
                    if (char.IsUpper(c) && pos > 0 && buffer[pos - 1] != '.')
                    {
                        buffer[pos++] = '_';
                    }

                    buffer[pos++] = char.ToLowerInvariant(c);
                }
            }
            mask.Paths.Add(new string(buffer[..pos]));
        }

        return mask;
    }

    /// <summary>
    /// Parses a JSON object from the specified reader and returns a corresponding
    /// Google.Protobuf.WellKnownTypes.Struct instance.
    /// </summary>
    /// <param name="reader">The reader positioned at the start of the JSON object to parse. The reader is advanced as the object is
    /// read.</param>
    /// <returns>A Struct representing the parsed JSON object. If the current token is not StartObject, returns an empty
    /// Struct.</returns>
    public static Google.Protobuf.WellKnownTypes.Struct ParseStruct(ref Utf8JsonReader reader)
    {
        var s = new Google.Protobuf.WellKnownTypes.Struct();
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return s;
        }

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var name = reader.GetString();
            reader.Read();
            s.Fields[name] = ParseValue(ref reader);
        }

        return s;
    }

    /// <summary>
    /// Parses the current JSON token from the specified reader and returns a corresponding
    /// Google.Protobuf.WellKnownTypes.Value instance.
    /// </summary>
    /// <param name="reader">A reference to the Utf8JsonReader positioned at the token to parse. The reader is advanced past the parsed
    /// value.</param>
    /// <returns>A Value instance representing the parsed JSON token. Returns a Value of kind NullValue if the token is not
    /// recognized.</returns>
    public static Google.Protobuf.WellKnownTypes.Value ParseValue(ref Utf8JsonReader reader)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => Google.Protobuf.WellKnownTypes.Value.ForNull(),
            JsonTokenType.True => Google.Protobuf.WellKnownTypes.Value.ForBool(true),
            JsonTokenType.False => Google.Protobuf.WellKnownTypes.Value.ForBool(false),
            JsonTokenType.Number => Google.Protobuf.WellKnownTypes.Value.ForNumber(reader.GetDouble()),
            JsonTokenType.String => Google.Protobuf.WellKnownTypes.Value.ForString(reader.GetString()),
            JsonTokenType.StartArray => ParseListValueAsValue(ref reader),
            JsonTokenType.StartObject => Google.Protobuf.WellKnownTypes.Value.ForStruct(ParseStruct(ref reader)),
            _ => Google.Protobuf.WellKnownTypes.Value.ForNull()
        };
    }

    /// <summary>
    /// Parses a JSON array from the specified reader and returns a Protobuf Value representing the array as a list.
    /// </summary>
    /// <param name="reader">A reference to the Utf8JsonReader positioned at the start of a JSON array. The reader is advanced to the end
    /// of the array after parsing.</param>
    /// <returns>A Value instance containing the parsed list of values from the JSON array.</returns>
    public static Google.Protobuf.WellKnownTypes.Value ParseListValueAsValue(ref Utf8JsonReader reader)
    {
        var values = new List<Google.Protobuf.WellKnownTypes.Value>(8);
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            values.Add(ParseValue(ref reader));
        }

        return Google.Protobuf.WellKnownTypes.Value.ForList([.. values]);
    }

    /// <summary>
    /// Parses a JSON array from the specified reader and returns a corresponding ListValue instance.
    /// </summary>
    /// <param name="reader">The reader positioned at the start of a JSON array to parse. The reader is advanced as the array is read.</param>
    /// <returns>A ListValue containing the elements parsed from the JSON array. If the current token is not StartArray,
    /// returns an empty ListValue.</returns>
    public static Google.Protobuf.WellKnownTypes.ListValue ParseListValue(ref Utf8JsonReader reader)
    {
        var lv = new Google.Protobuf.WellKnownTypes.ListValue();
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            return lv;
        }

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            lv.Values.Add(ParseValue(ref reader));
        }

        return lv;
    }
}

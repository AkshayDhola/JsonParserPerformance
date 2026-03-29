using Google.Protobuf;
using Google.Protobuf.Reflection;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace JsonParserPerformance.Helpers;

internal static class PrimitiveParser
{
    /// <summary>
    /// Reads a numeric value from the current JSON token and converts it to the specified numeric type.
    /// </summary>
    /// <typeparam name="T">The numeric type to convert the JSON value to. Must implement INumber<T>.</typeparam>
    /// <param name="reader">A reference to the Utf8JsonReader positioned at a JSON token representing a number or a string containing a
    /// numeric value.</param>
    /// <returns>The value of the current JSON token converted to the specified numeric type T.</returns>
    /// <exception cref="NotSupportedException">Thrown if the specified type T is not supported for conversion.</exception>
    /// <exception cref="JsonException">Thrown if the current JSON token is a string that is null or contains only whitespace.</exception>
    /// <exception cref="FormatException">Thrown if the string value cannot be parsed to the specified numeric type T.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetNumeric<T>(ref Utf8JsonReader reader) where T : INumber<T>
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return typeof(T) switch
            {
                Type t when t == typeof(int) => (T)(object)reader.GetInt32(),
                Type t when t == typeof(long) => (T)(object)reader.GetInt64(),
                Type t when t == typeof(uint) => (T)(object)reader.GetUInt32(),
                Type t when t == typeof(ulong) => (T)(object)reader.GetUInt64(),
                Type t when t == typeof(float) => (T)(object)reader.GetSingle(),
                Type t when t == typeof(double) => (T)(object)reader.GetDouble(),
                _ => throw new NotSupportedException($"Type {typeof(T)} is not supported.")
            };
        }

        string? str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str))
        {
            return T.Zero;
        }

        //if (typeof(T) == typeof(float))
        //{
        //    if (str == "NaN") return (T)(object)float.NaN;
        //    if (str == "Infinity") return (T)(object)float.PositiveInfinity;
        //    if (str == "-Infinity") return (T)(object)float.NegativeInfinity;
        //}

        //if (typeof(T) == typeof(double))
        //{
        //    if (str == "NaN") return (T)(object)double.NaN;
        //    if (str == "Infinity") return (T)(object)double.PositiveInfinity;
        //    if (str == "-Infinity") return (T)(object)double.NegativeInfinity;
        //}

        if (T.TryParse(str, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        throw new FormatException($"Invalid {typeof(T).Name} value: '{str}'.");
    }

    /// <summary>
    /// Reads the current JSON token from the specified reader and returns its boolean value.
    /// </summary>
    /// <param name="reader">A reference to the Utf8JsonReader positioned at a token representing a boolean value or a string that can be
    /// parsed as a boolean.</param>
    /// <returns>true if the current token is JsonTokenType.True or represents the string "true"; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBool(ref Utf8JsonReader reader)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            _ => bool.Parse(reader.GetString() ?? Boolean.FalseString)
        };
    }

    /// <summary>
    /// Parses a base64-encoded JSON string from the current token and returns its value as a ByteString.
    /// </summary>
    /// <param name="reader">A reference to the Utf8JsonReader positioned at a JSON string token containing base64-encoded data.</param>
    /// <returns>A ByteString representing the decoded bytes from the base64-encoded JSON string.</returns>
    /// <exception cref="JsonException">Thrown if the current JSON token is not a string or if the string is not valid base64.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ByteString ParseBytes(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected base64 string");
        }

        return ByteString.FromBase64(reader.GetString());
    }

    /// <summary>
    /// Parses an enum value from the current token in the JSON reader using the specified field descriptor.
    /// </summary>
    /// <param name="field">The field descriptor that provides metadata about the enum type to parse.</param>
    /// <param name="reader">A reference to the JSON reader positioned at the token to parse. The token should represent a string,
    /// number, or null value.</param>
    /// <returns>An object representing the parsed enum value. Returns 0 if the token is null or an empty string.</returns>
    /// <exception cref="Exception">Thrown if the token value does not correspond to a valid enum name or numeric value for the specified enum
    /// type.</exception>
    public static object ParseEnum(FieldDescriptor field, ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return 0;
        }

        if (reader.TokenType == JsonTokenType.Number) return reader.GetInt32();

        var str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str)) return 0;
        if (int.TryParse(str, out var num)) return num;

        var nameMap = ParserCache.GetOrAddEnum(field.EnumType);

        return nameMap.TryGetValue(str, out var found) ? found : throw new JsonException($"Unknown enum: {str}");
    }
}

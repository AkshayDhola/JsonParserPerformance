using Google.Protobuf;
using JsonParserPerformance.Helpers;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ProtoMessageParser = JsonParserPerformance.Helpers.MessageParser;

namespace JsonParserPerformance;

public static class JsonParser
{
    /// <summary>
    /// Parses a JSON string into a protocol buffer message of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of protocol buffer message to parse. Must implement IMessage<T> and have a parameterless
    /// constructor.</typeparam>
    /// <param name="json">The JSON string representing the protocol buffer message to parse. Cannot be null.</param>
    /// <returns>An instance of type T representing the parsed protocol buffer message.</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid, empty, or does not match the expected structure.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Parse<T>(string json) where T : IMessage<T>, new()
    {
        var message = new T();
        var meta = ParserCache.GetOrCreateFieldLookup(message);

        int maxBytes = System.Text.Encoding.UTF8.GetMaxByteCount(json.Length);
        byte[]? rented = null;

        Span<byte> utf8Json = maxBytes <= 1024 ? stackalloc byte[1024] : Rent();

        byte[] Rent()
        {
            rented = ArrayPool<byte>.Shared.Rent(maxBytes);
            return rented;
        }

        try
        {
            int written = System.Text.Encoding.UTF8.GetBytes(json, utf8Json);
            var reader = new Utf8JsonReader(utf8Json[..written]);

            // Read first token to position at StartObject
            if (!reader.Read())
            {
                throw new JsonException("Empty JSON");
            }

            ProtoMessageParser.ParseObject(message, meta, ref reader);
            return message;
        }
        finally
        {
            if (rented != null)
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }
}

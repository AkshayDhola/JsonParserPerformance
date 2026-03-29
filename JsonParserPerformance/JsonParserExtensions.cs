using Google.Protobuf;

namespace JsonParserPerformance
{
    public static class JsonParserExtensions
    {
        /// <summary>
        /// Parses a JSON string into a protocol buffer message of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of protocol buffer message to parse. Must implement IMessage<T> and have a parameterless
        /// constructor.</typeparam>
        /// <param name="_">The message parser instance. This parameter is not used.</param>
        /// <param name="json">The JSON string representing the protocol buffer message to parse. Cannot be null.</param>
        /// <returns>An instance of type T representing the parsed protocol buffer message.</returns>
        public static T ParseFrom<T>(this MessageParser<T> _, string json) where T : IMessage<T>, new()
        {
            return JsonParser.Parse<T>(json);
        }
    }
}

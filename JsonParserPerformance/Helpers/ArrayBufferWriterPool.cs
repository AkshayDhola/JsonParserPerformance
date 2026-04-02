using System.Buffers;

namespace JsonParserPerformance.Helpers
{
    /// <summary>
    /// Simple pool for ArrayBufferWriter to avoid allocations in ParseAny.
    /// </summary>
    internal static class ArrayBufferWriterPool
    {
        [ThreadStatic]
        private static ArrayBufferWriter<byte>? t_cached;

        public static ArrayBufferWriter<byte> Rent()
        {
            var writer = t_cached;
            if (writer != null)
            {
                t_cached = null;
                writer.ResetWrittenCount();
                return writer;
            }
            return new ArrayBufferWriter<byte>(512);
        }

        public static void Return(ArrayBufferWriter<byte> writer)
        {
            // Only cache if not too large (avoid holding huge buffers)
            if (writer.Capacity <= 65536)
            {
                t_cached = writer;
            }
        }
    }
}

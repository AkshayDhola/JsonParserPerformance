using Google.Protobuf.Reflection;
using System.Runtime.CompilerServices;

namespace JsonParserPerformance.Protobuf;

internal sealed class FieldLookup
{
    private readonly (byte[] Utf8Name, int Hash, FieldAccessorInfo Meta)[] _fields;

    public FieldLookup(MessageDescriptor descriptor)
    {
        var fields = descriptor.Fields.InFieldNumberOrder();
        var list = new List<(byte[], int, FieldAccessorInfo)>(fields.Count * 2);

        foreach (var f in fields)
        {
            var meta = new FieldAccessorInfo(f);

            var jsonNameBytes = System.Text.Encoding.UTF8.GetBytes(f.JsonName);
            list.Add((jsonNameBytes, ComputeHash(jsonNameBytes), meta));

            if (f.Name != f.JsonName)
            {
                var nameBytes = System.Text.Encoding.UTF8.GetBytes(f.Name);
                list.Add((nameBytes, ComputeHash(nameBytes), meta));
            }
        }

        _fields = [.. list];
    }

    // Finds the field accessor info for a given UTF-8 encoded field name, using a hash-based lookup for efficiency.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FieldAccessorInfo? Find(ReadOnlySpan<byte> utf8Name)
    {
        var hash = ComputeHash(utf8Name);
        var fields = _fields;

        for (int i = 0; i < fields.Length; i++)
        {
            ref var field = ref fields[i];
            if (field.Hash == hash && utf8Name.SequenceEqual(field.Utf8Name))
                return field.Meta;
        }
        return null;
    }

    // FNV-1a hash algorithm for case-sensitive hashing of UTF-8 byte sequences.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeHash(ReadOnlySpan<byte> bytes)
    {
        uint hash = 2166136261;
        for (int i = 0; i < bytes.Length; i++)
        {
            hash ^= bytes[i];
            hash *= 16777619;
        }
        return (int)hash;
    }
}

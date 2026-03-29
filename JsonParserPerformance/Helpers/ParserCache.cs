using Google.Protobuf;
using Google.Protobuf.Reflection;
using JsonParserPerformance.Protobuf;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace JsonParserPerformance.Helpers;

internal static class ParserCache
{
    private static readonly ConcurrentDictionary<Type, FieldLookup> _fieldLookupCache = new();
    private static readonly ConcurrentDictionary<string, Type?> _anyTypesCache = new();
    private static readonly ConcurrentDictionary<string, FrozenDictionary<string, int>> _enumsCache = new();
    private static readonly ConcurrentDictionary<Type, Action<object, object?, object?>> _mapAddersCache = new();

    /// <summary>
    /// Gets an existing field lookup for the specified message type, or creates and caches a new one if it does not
    /// exist.
    /// </summary>
    /// <typeparam name="T">The type of the protocol buffer message. Must implement <see cref="IMessage{T}"/>.</typeparam>
    /// <param name="message">An instance of the message type used to obtain its descriptor. Cannot be null.</param>
    /// <returns>A <see cref="FieldLookup"/> instance associated with the specified message type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FieldLookup GetOrCreateFieldLookup<T>(T message) where T : IMessage<T>
    {
        return _fieldLookupCache.GetOrAdd(typeof(T), static (_, m) => new FieldLookup(m.Descriptor), message);
    }

    /// <summary>
    /// Retrieves a cached field lookup for the specified message type, or creates and caches a new one if none
    /// exists.
    /// </summary>
    /// <param name="message">The message instance whose type is used to obtain or create the associated field lookup. Cannot be null.</param>
    /// <returns>A cached or newly created instance of FieldLookup for the type of the specified message.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FieldLookup GetOrCreateFieldLookup(IMessage message)
    {
        return _fieldLookupCache.GetOrAdd(message.GetType(), static (_, m) => new FieldLookup(m.Descriptor), message);
    }

    /// <summary>
    /// Gets the runtime type corresponding to the specified Protobuf type URL, adding it to the cache if not
    /// already present.
    /// </summary>
    /// <param name="typeUrl">The fully qualified Protobuf type URL to resolve to a .NET type. Cannot be null.</param>
    /// <returns>The resolved .NET type that implements IMessage and matches the specified type URL; or null if no matching
    /// type is found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Type? GetOrAddAnyType(string typeUrl)
    {
        return _anyTypesCache.GetOrAdd(typeUrl, static name =>
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch
                {
                    continue;
                }

                foreach (var t in types)
                {
                    if (!t.IsAbstract && typeof(IMessage).IsAssignableFrom(t))
                    {
                        try
                        {
                            if (Activator.CreateInstance(t) is IMessage m && m.Descriptor.FullName == name)
                            {
                                return t;
                            }
                        }
                        catch
                        {
                            // Ignore any exceptions from trying to create instances of types that may not have parameterless constructors or other issues.
                        }
                    }
                }
            }
            return null;
        });
    }

    /// <summary>
    /// Gets a cached, case-insensitive mapping of enum value names to their numeric values for the specified enum
    /// type, adding it to the cache if it does not already exist.
    /// </summary>
    /// <param name="enumType">The descriptor representing the enum type for which to retrieve or add the name-to-value mapping.</param>
    /// <returns>A frozen dictionary containing the mapping of enum value names to their corresponding numeric values for the
    /// specified enum type. The dictionary is case-insensitive and is shared for repeated requests of the same enum
    /// type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FrozenDictionary<string, int> GetOrAddEnum(EnumDescriptor enumType)
    {
        return _enumsCache.GetOrAdd(enumType.FullName, static (_, et) =>
        {
            var vals = et.Values;
            var map = new Dictionary<string, int>(vals.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var v in vals)
            {
                map.TryAdd(v.Name, v.Number);
            }

            return map.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        }, enumType);
    }

    /// <summary>
    /// Retrieves a cached delegate that adds a key-value pair to a map of the specified type, or creates and caches
    /// one if it does not exist.
    /// </summary>
    /// <param name="mapType">The type of the map for which to retrieve or create an adder delegate. Must have a public instance 'Add'
    /// method that accepts two parameters.</param>
    /// <returns>An action delegate that adds a key-value pair to an instance of the specified map type. The delegate accepts
    /// the map instance, the key, and the value as parameters.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Action<object, object?, object?> GetOrAddMapAdder(Type mapType)
    {
        return _mapAddersCache.GetOrAdd(mapType, static t =>
        {
            var method = t.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                          .First(m => m.Name == "Add" && m.GetParameters().Length == 2);

            var parms = method.GetParameters();
            var mapArg = Expression.Parameter(typeof(object), "map");
            var keyArg = Expression.Parameter(typeof(object), "key");
            var valArg = Expression.Parameter(typeof(object), "val");

            var call = Expression.Call(
                Expression.Convert(mapArg, t),
                method,
                Expression.Convert(keyArg, parms[0].ParameterType),
                Expression.Convert(valArg, parms[1].ParameterType));

            return Expression.Lambda<Action<object, object?, object?>>(call, mapArg, keyArg, valArg).Compile();
        });
    }
}

namespace System.Collections.Generic;

internal static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
    {
        foreach (var value in values)
            collection.Add(value);
    }
}

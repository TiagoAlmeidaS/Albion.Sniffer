using System.Collections.Concurrent;

namespace Albion.Sniffer.Core.Utility
{
    public static class DictionaryExtensions
    {
        public static void RemoveAll<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict,
            Func<KeyValuePair<TKey, TValue>, bool> removeIf)
        {
            foreach (var item in dict.Where(removeIf).ToList())
            {
                dict.TryRemove(item.Key, out TValue value);
            }
        }
    }
}

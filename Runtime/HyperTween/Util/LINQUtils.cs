using System.Collections.Generic;
using Unity.Collections;

namespace HyperTween.Util
{
    public static class LINQUtils
    {
        public static NativeArray<T> ToNativeArray<T>(this IEnumerable<T> enumerable, Allocator allocator) where T : unmanaged
        {
            var list = new NativeList<T>(enumerable is ICollection<T> collection ? collection.Count : 4, allocator);
            foreach (var elem in enumerable)
            {
                list.Add(elem);
            }

            return list.AsArray();
        }
        
        public static NativeList<T> ToNativeList<T>(this IEnumerable<T> enumerable, Allocator allocator) where T : unmanaged
        {
            var list = new NativeList<T>(enumerable is ICollection<T> collection ? collection.Count : 4, allocator);
            foreach (var elem in enumerable)
            {
                list.Add(elem);
            }

            return list;
        }
        
    }
}
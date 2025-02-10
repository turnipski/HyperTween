using System;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace HyperTween.ECS.Util
{
    public struct FieldEnumerable<TType, TFieldType> : IDisposable 
        where TType : unmanaged 
        where TFieldType : unmanaged
    {
        public unsafe struct Enumerator
        {
            private int _current;
            private readonly NativeArray<int> _fieldOffsets;
            private readonly byte* _basePointer;

            public Enumerator(NativeArray<int> fieldOffsets, ref TType instance)
            {
                _current = 0;
                _fieldOffsets = fieldOffsets;
                _basePointer = (byte*)UnsafeUtility.AddressOf(ref instance);
            }
            
            public bool Next(out TFieldType value)
            {
                if (_current >= _fieldOffsets.Length)
                {
                    value = default;
                    return false;
                }

                var offset = _fieldOffsets[_current++];
                value = *(TFieldType*)(_basePointer + offset);
                return true;
            }
        }
        
        private NativeArray<int> _fieldOffsets;

        public FieldEnumerable(Allocator allocator)
        {
            var fields = typeof(TType)
                .GetFields(BindingFlags.Instance | BindingFlags.Public);
            
            var fieldInfos = fields
                .Where(info => info.FieldType == typeof(DynamicTypeInfo));
            
            var fieldOffsets = fieldInfos.Select(UnsafeUtility.GetFieldOffset)
                .ToArray();

            _fieldOffsets = new NativeArray<int>(fieldOffsets.Length, allocator);
            _fieldOffsets.CopyFrom(fieldOffsets);
        }

        public Enumerator GetEnumerator(ref TType instance)
        {
            return new Enumerator(_fieldOffsets, ref instance);
        }

        public void Dispose()
        {
            _fieldOffsets.Dispose();
        }
    }
}
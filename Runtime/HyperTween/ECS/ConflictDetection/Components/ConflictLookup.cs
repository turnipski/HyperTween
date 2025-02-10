using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace HyperTween.ECS.ConflictDetection.Components
{
    public struct ConflictLookup : IComponentData, IDisposable
    {
        public readonly struct TransformInstanceIdKey : IEquatable<TransformInstanceIdKey>
        {
            private readonly int _instanceId;
            private readonly ComponentType _componentType;

            public TransformInstanceIdKey(int instanceId, ComponentType componentType)
            {
                _instanceId = instanceId;
                _componentType = componentType;
            }

            public bool Equals(TransformInstanceIdKey other)
            {
                return _instanceId == other._instanceId && _componentType == other._componentType;
            }

            public override int GetHashCode()
            {
                // Using bitwise operations and arithmetic to combine the hash codes
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + _instanceId;
                    hash = hash * 31 + _componentType.GetHashCode();
                    return hash;
                }            
            }
        }
        
        public readonly struct EntityTypeKey : IEquatable<EntityTypeKey>
        {
            private readonly Entity _targetEntity;
            private readonly ComponentType _componentType;

            public EntityTypeKey(Entity targetEntity, ComponentType componentType)
            {
                _targetEntity = targetEntity;
                _componentType = componentType;
            }

            public bool Equals(EntityTypeKey other)
            {
                return _targetEntity == other._targetEntity && _componentType == other._componentType;
            }

            public override int GetHashCode()
            {
                // Using bitwise operations and arithmetic to combine the hash codes
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + _targetEntity.GetHashCode();
                    hash = hash * 31 + _componentType.GetHashCode();
                    return hash;
                }            
            }
        }
        
        public NativeHashMap<EntityTypeKey, Entity> EntityTypeKeyToTweenMap;
        public NativeHashMap<TransformInstanceIdKey, Entity> GameObjectTypeKeyToTweenMap;

        public static ConflictLookup Allocate()
        {
            return new ConflictLookup()
            {
                EntityTypeKeyToTweenMap = new NativeHashMap<EntityTypeKey, Entity>(8, Allocator.Persistent),
                GameObjectTypeKeyToTweenMap = new NativeHashMap<TransformInstanceIdKey, Entity>(8, Allocator.Persistent)
            };
        }
        
        public void Dispose()
        {
            EntityTypeKeyToTweenMap.Dispose();
            GameObjectTypeKeyToTweenMap.Dispose();
        }
    }
}
using HyperTween.Auto.Systems;
using HyperTween.ECS.ConflictDetection.Components;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Structural.Systems;
using HyperTween.ECS.Update.Components;
using HyperTween.ECS.Util;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace HyperTween.ECS.ConflictDetection.Systems
{
    [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
    [BurstCompile]
    public partial struct AddToConflictLookupSystem : ISystem
    {
        [BurstCompile]
        private struct Job : IJobChunk
        {
            public NativeHashMap<ConflictLookup.EntityTypeKey, Entity> EntityTypeKeyToTweenMap;
            public NativeHashMap<ConflictLookup.TransformInstanceIdKey, Entity> GameObjectTypeKeyToTweenMap;
            
            public EntityCommandBuffer EntityCommandBuffer;

            public DetectConflictsJobData DetectConflictsJobData;
            public FieldEnumerable<DetectConflictsJobData, DetectConflictsJobData.ConflictTypeTuple> ConflictTypeTupleFieldEnumerable;
            
            public EntityTypeHandle EntityTypeHandle;
            
            [ReadOnly]
            public ComponentTypeHandle<TweenTarget> TweenTargetTypeHandle;

            public int IntSize;
            
            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            { 
                var entities = chunk.GetNativeArray(EntityTypeHandle);
                var typeInfoEnumerator = ConflictTypeTupleFieldEnumerable.GetEnumerator(ref DetectConflictsJobData);

                while (typeInfoEnumerator.Next(out var conflictTypeTuple))
                {
                    var targetComponentTypeHandle = conflictTypeTuple.TargetComponentTypeInfo.DynamicComponentTypeHandle;
                    var targetComponentType = conflictTypeTuple.TargetComponentTypeInfo.ComponentType;

                    if (!chunk.Has(ref targetComponentTypeHandle))
                    {
                        continue;
                    }

                    var hasTarget = false;

                    if (conflictTypeTuple.HasInstanceIdComponent)
                    {
                        var instanceIdComponentTypeHandle = conflictTypeTuple.InstanceIdComponentTypeInfo.DynamicComponentTypeHandle;

                        if (chunk.Has(ref instanceIdComponentTypeHandle))
                        {
                            hasTarget = true;
                        
                            var instanceIds = chunk.GetDynamicComponentDataArrayReinterpret<int>(ref instanceIdComponentTypeHandle, IntSize);

                            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                            while (enumerator.NextEntityIndex(out var i))
                            {
                                var entity = entities[i];
                                var instanceId = instanceIds[i];
                            
                                var key = new ConflictLookup.TransformInstanceIdKey(instanceId, targetComponentType);
                
                                // No TweenTarget so we treat the Tween as the target
                                if (GameObjectTypeKeyToTweenMap.TryGetValue(key, out var oldTweenEntity))
                                {
                                    OnConflictDetected(oldTweenEntity);
                                
                                    // This Tween is now in control of itself
                                    GameObjectTypeKeyToTweenMap[key] = entity;
                                }
                                else
                                {
                                    // This Tween is now in control of itself
                                    GameObjectTypeKeyToTweenMap.Add(key, entity);
                                }
                            }
                        }
                    }
                    
                    
                    if (chunk.Has(ref TweenTargetTypeHandle))
                    {
                        hasTarget = true;
                        
                        var tweenTargets = chunk.GetNativeArray(ref TweenTargetTypeHandle);

                        var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                        while (enumerator.NextEntityIndex(out var i))
                        {
                            var entity = entities[i];
                            var targetEntity = tweenTargets[i].Target;
                            
                            var key = new ConflictLookup.EntityTypeKey(targetEntity, targetComponentType);
                
                            // No TweenTarget so we treat the Tween as the target
                            if (EntityTypeKeyToTweenMap.TryGetValue(key, out var oldTweenEntity))
                            {
                                OnConflictDetected(oldTweenEntity);
                                
                                // This Tween is now in control of itself
                                EntityTypeKeyToTweenMap[key] = entity;
                            }
                            else
                            {
                                // This Tween is now in control of itself
                                EntityTypeKeyToTweenMap.Add(key, entity);
                            }
                        }
                    }
                    
                    if(!hasTarget)
                    {
                        var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                        while (enumerator.NextEntityIndex(out var i))
                        {
                            var entity = entities[i];
                            
                            var key = new ConflictLookup.EntityTypeKey(entity, targetComponentType);
                
                            // No TweenTarget so we treat the Tween as the target
                            if (EntityTypeKeyToTweenMap.TryGetValue(key, out var oldTweenEntity))
                            {
                                OnConflictDetected(oldTweenEntity);
                                
                                // This Tween is now in control of itself
                                EntityTypeKeyToTweenMap[key] = entity;
                            }
                            else
                            {
                                // This Tween is now in control of itself
                                EntityTypeKeyToTweenMap.Add(key, entity);
                            }
                        }
                    }
                }
            }

            [BurstCompile]
            private void OnConflictDetected(Entity oldTweenEntity)
            {
                // Stop the Tween currently in control of the target
                EntityCommandBuffer.RemoveComponent<TweenRequestPlaying>(oldTweenEntity);
                
                // TODO: Add TweenRequestConflict component
                EntityCommandBuffer.AddComponent<TweenConflicted>(oldTweenEntity);
            }
        }
        
        private EntityQuery _query;
        private DetectConflictsJobData _detectConflictsJobData;
        private FieldEnumerable<DetectConflictsJobData, DetectConflictsJobData.ConflictTypeTuple> _conflictTypeTupleFieldEnumerable;

        public void OnCreate(ref SystemState state)
        {
            var componentTypes = DetectConflictsHelper.GetConflictComponentTypes();

            _query = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAny(ref componentTypes)
                .WithAll<TweenRequestPlaying>()
                .WithNone<TweenPlaying>());

            _conflictTypeTupleFieldEnumerable =
                new FieldEnumerable<DetectConflictsJobData, DetectConflictsJobData.ConflictTypeTuple>(Allocator.Persistent);

            _detectConflictsJobData.Initialise(ref state);
            
            state.RequireForUpdate(_query);
            state.RequireForUpdate<TweenStructuralChangeECBSystem.Singleton>();

            componentTypes.Dispose();
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _conflictTypeTupleFieldEnumerable.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            var conflictLookup = SystemAPI.GetSingletonRW<ConflictLookup>();
            _detectConflictsJobData.Update(ref state);
            
            state.Dependency = new Job()
            {
                EntityTypeKeyToTweenMap = conflictLookup.ValueRW.EntityTypeKeyToTweenMap,
                GameObjectTypeKeyToTweenMap = conflictLookup.ValueRW.GameObjectTypeKeyToTweenMap,
                TweenTargetTypeHandle = SystemAPI.GetComponentTypeHandle<TweenTarget>(true),
                DetectConflictsJobData = _detectConflictsJobData,
                ConflictTypeTupleFieldEnumerable = _conflictTypeTupleFieldEnumerable,
                EntityTypeHandle = SystemAPI.GetEntityTypeHandle(),
                EntityCommandBuffer = ecb,
                IntSize = UnsafeUtility.SizeOf<int>()
            }.Schedule(_query, state.Dependency);
        }
    }
}
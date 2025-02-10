using System;
using HyperTween.Auto.Systems;
using HyperTween.ECS.ConflictDetection.Components;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Structural.Systems;
using HyperTween.ECS.Update.Components;
using HyperTween.ECS.Util;
using HyperTween.Modules.Transform;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace HyperTween.ECS.ConflictDetection.Systems
{
    [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
    [BurstCompile]
    public partial struct RemoveFromConflictLookupSystem : ISystem
    {
        [BurstCompile]
        private struct Job : IJobChunk
        {
            public NativeHashMap<ConflictLookup.EntityTypeKey, Entity> TargetToTweenMap;
            public NativeHashMap<ConflictLookup.TransformInstanceIdKey, Entity> GameObjectTypeKeyToTweenMap;

            public DetectConflictsJobData DetectConflictsJobData;
            public FieldEnumerable<DetectConflictsJobData, DynamicTypeInfo> DynamicTypeInfoFieldEnumerable;
            
            public EntityTypeHandle EntityTypeHandle;
            
            [ReadOnly]
            public ComponentTypeHandle<TweenTarget> TweenTargetTypeHandle;

            [ReadOnly]
            public ComponentTypeHandle<TransformInstanceId> TransformInstanceIdTypeHandle;
            
            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            { 
                var entities = chunk.GetNativeArray(EntityTypeHandle);
                var typeInfoEnumerator = DynamicTypeInfoFieldEnumerable.GetEnumerator(ref DetectConflictsJobData);

                while (typeInfoEnumerator.Next(out var dynamicTypeInfo))
                {
                    var dynamicComponentTypeHandle = dynamicTypeInfo.DynamicComponentTypeHandle;
                    var componentType = dynamicTypeInfo.ComponentType;

                    if (!chunk.Has(ref dynamicComponentTypeHandle))
                    {
                        continue;
                    }

                    var hasTarget = false;
                    
                    if (chunk.Has(ref TransformInstanceIdTypeHandle))
                    {
                        hasTarget = true;
                        
                        var transformInstanceIds = chunk.GetNativeArray(ref TransformInstanceIdTypeHandle);

                        var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                        while (enumerator.NextEntityIndex(out var i))
                        {
                            var transformInstanceId = transformInstanceIds[i].Value;

                            if (!GameObjectTypeKeyToTweenMap.Remove(new ConflictLookup.TransformInstanceIdKey(transformInstanceId, componentType)))
                            {
                                throw new InvalidOperationException("Tween Entity does not exist in ConflictLookup");
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
                            var entity = tweenTargets[i].Target;

                            if (!TargetToTweenMap.Remove(new ConflictLookup.EntityTypeKey(entity, componentType)))
                            {
                                throw new InvalidOperationException("Tween Entity does not exist in ConflictLookup");
                            }
                        }
                    }
                    
                    if(!hasTarget)
                    {
                        var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                        while (enumerator.NextEntityIndex(out var i))
                        {
                            var entity = entities[i];
                            if (!TargetToTweenMap.Remove(new ConflictLookup.EntityTypeKey(entity, componentType)))
                            {
                                throw new InvalidOperationException("Tween Entity does not exist in ConflictLookup");
                            }
                        }
                    }
                }
            }
        }

        private EntityQuery _query;
        private DetectConflictsJobData _detectConflictsJobData;
        private FieldEnumerable<DetectConflictsJobData, DynamicTypeInfo> _dynamicTypeInfoFieldEnumerable;
        private NativeList<ComponentType> _componentTypes;

        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(ConflictLookup.Allocate());

            _componentTypes = DetectConflictsHelper.GetConflictComponentTypes();

            _query = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAny(ref _componentTypes)
                .WithAll<TweenPlaying>()
                // Don't remove Tweens that have conflicted - we know the target has been taken over by another entity
                .WithNone<TweenRequestPlaying, TweenConflicted>());

            _dynamicTypeInfoFieldEnumerable =
                new FieldEnumerable<DetectConflictsJobData, DynamicTypeInfo>(Allocator.Persistent);

            _detectConflictsJobData.Initialise(ref state);
            
            state.RequireForUpdate(_query);
        }

        public void OnDestroy(ref SystemState state)
        {
            _dynamicTypeInfoFieldEnumerable.Dispose();
            
            ref var conflictLookup = ref SystemAPI.GetSingletonRW<ConflictLookup>().ValueRW;
            conflictLookup.Dispose();

            _componentTypes.Dispose();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var conflictLookup = SystemAPI.GetSingletonRW<ConflictLookup>();
            _detectConflictsJobData.Update(ref state);
            
            state.Dependency = new Job()
            {
                TargetToTweenMap = conflictLookup.ValueRW.EntityTypeKeyToTweenMap,
                GameObjectTypeKeyToTweenMap = conflictLookup.ValueRW.GameObjectTypeKeyToTweenMap,
                TweenTargetTypeHandle = SystemAPI.GetComponentTypeHandle<TweenTarget>(),
                TransformInstanceIdTypeHandle = SystemAPI.GetComponentTypeHandle<TransformInstanceId>(),
                DetectConflictsJobData = _detectConflictsJobData,
                DynamicTypeInfoFieldEnumerable = _dynamicTypeInfoFieldEnumerable,
                EntityTypeHandle = SystemAPI.GetEntityTypeHandle()
            }.Schedule(_query, state.Dependency);
        }
    }
}
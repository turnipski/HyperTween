using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Structural.Systems;
using HyperTween.ECS.Update.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

namespace HyperTween.Modules.Transform.Systems
{
    [UpdateInGroup(typeof(UpdateTweenSystemGroup))]
    [UpdateBefore(typeof(TweenTransformStructuralChangeECBSystem))]
    [BurstCompile]
    public partial struct UpdateTransformAccessArraySystem : ISystem
    {
        private ComponentLookup<TransformInstanceId> _transformInstanceIdLookup;

        private EntityQuery _removalQuery, _additionQuery, _missingLocalToWorldQuery;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TweenTransformStructuralChangeECBSystem.Singleton>();
            state.EntityManager.CreateSingleton(new TransformAccessSingleton()
            {
                TransformAccessArray = new TransformAccessArray(64),
                EntityLookup = new NativeList<Entity>(64, Allocator.Persistent),
                IndexLookup = new NativeHashMap<Entity, int>(64, Allocator.Persistent),
            });
            state.RequireForUpdate<TransformAccessSingleton>();
            
            _transformInstanceIdLookup = state.GetComponentLookup<TransformInstanceId>();

            _removalQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<InTransformAccessArray>()
                .WithNone<TweenPlaying, TweenForceOutput>()
                .Build(ref state);

            _additionQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TransformInstanceId>()
                .WithAny<TweenPlaying, TweenForceOutput>()
                .WithNone<InTransformAccessArray>()
                .Build(ref state);
            
            _missingLocalToWorldQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TransformInstanceId>()
                .WithAny<TweenPlaying, TweenForceOutput>()
                .WithNone<LocalToWorld>()
                .Build(ref state);
        }

        public void OnDestroy(ref SystemState state)
        {
            ref var conflictLookup = ref SystemAPI.GetSingletonRW<TransformAccessSingleton>().ValueRW;
            conflictLookup.Dispose();        
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var singleton = SystemAPI.GetSingleton<TransformAccessSingleton>();
            
            var ecbSingleton = SystemAPI.GetSingleton<TweenTransformStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            _transformInstanceIdLookup.Update(ref state);
            
            var requiredCapacity = math.ceilpow2(singleton.EntityLookup.Length - _removalQuery.CalculateEntityCount() + _additionQuery.CalculateEntityCount());
            if (singleton.EntityLookup.Capacity < requiredCapacity)
            {
                singleton.EntityLookup.Capacity = requiredCapacity;
                singleton.TransformAccessArray.capacity = requiredCapacity;
            }
            
            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<InTransformAccessArray>>()
                         .WithNone<TweenPlaying, TweenForceOutput>()
                         .WithEntityAccess())
            {
                var removeIndex = singleton.IndexLookup[entity];
                var backEntity = singleton.EntityLookup[^1];

                singleton.IndexLookup[backEntity] = removeIndex;

                //Debug.Log($"Replace transform at {removeIndex} with {backEntity}");
                
                singleton.EntityLookup.RemoveAtSwapBack(removeIndex);
                singleton.TransformAccessArray.RemoveAtSwapBack(removeIndex);
            }
            
            ecb.RemoveComponent<InTransformAccessArray>(_removalQuery, EntityQueryCaptureMode.AtPlayback);
            
            foreach (var (transformInstanceId, entity) in SystemAPI
                         .Query<RefRO<TransformInstanceId>>()
                         .WithAny<TweenPlaying, TweenForceOutput>()
                         .WithNone<InTransformAccessArray>()
                         .WithEntityAccess())
            {
                singleton.IndexLookup[entity] = singleton.TransformAccessArray.length;
                
                //Debug.Log($"Add transform {entity} at {singleton.TransformAccessArray.length}");

                singleton.TransformAccessArray.Add(transformInstanceId.ValueRO.Value);
                singleton.EntityLookup.Add(entity);
            }

            ecb.AddComponent<InTransformAccessArray>(_additionQuery, EntityQueryCaptureMode.AtPlayback);
            ecb.AddComponent<LocalToWorld>(_missingLocalToWorldQuery, EntityQueryCaptureMode.AtPlayback);
        }
    }
}
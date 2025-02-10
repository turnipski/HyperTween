using HyperTween.Auto.Components;
using HyperTween.ECS.Structural.Systems;
using HyperTween.ECS.Update.Systems;
using HyperTween.Modules.LocalTransform.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

namespace HyperTween.Modules.Transform.Systems
{
    [UpdateInGroup(typeof(UpdateTweenSystemGroup))]
    [UpdateAfter(typeof(TweenTransformStructuralChangeECBSystem))]
    [UpdateBefore(typeof(AddTweenFromSystemGroup))]
    public partial struct SyncTransformOnPlaySystem : ISystem
    {
        [BurstCompile]
        struct CopyFromTransformJob : IJobParallelForTransform
        {
            [ReadOnly]
            public NativeList<Entity> EntityLookup;
            [NativeDisableParallelForRestriction]
            public ComponentLookup<Unity.Transforms.LocalTransform> LocalTransforms;
            
            public void Execute(int index, TransformAccess transform)
            {
                var entity = EntityLookup[index];
                
                if (!LocalTransforms.HasComponent(entity))
                {
                    Debug.LogError("Missing LocalTransform component");
                    return;
                }

                LocalTransforms[entity] = new Unity.Transforms.LocalTransform()
                {
                    Position = transform.localPosition,
                    Rotation = transform.localRotation,
                    Scale = transform.localScale.x
                };
            }
        }

        private EntityTypeHandle _entityTypeHandle;
        private ComponentTypeHandle<TransformInstanceId> _transformInstanceIdTypeHandle;
        private ComponentLookup<Unity.Transforms.LocalTransform> _localTransformLookup;
        private EntityQuery _query;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TransformAccessSingleton>();
            
            _localTransformLookup = state.GetComponentLookup<Unity.Transforms.LocalTransform>();

            _query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<InTransformAccessArray>()
                .WithAny<TweenLocalPosition, TweenLocalRotation, TweenLocalUniformScale>()
                .Build(ref state);
            
            state.RequireForUpdate(_query);

            _entityTypeHandle = state.GetEntityTypeHandle();
            _transformInstanceIdTypeHandle = state.GetComponentTypeHandle<TransformInstanceId>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var singleton = SystemAPI.GetSingleton<TransformAccessSingleton>();

            _entityTypeHandle.Update(ref state);
            _transformInstanceIdTypeHandle.Update(ref state);
            _localTransformLookup.Update(ref state);

            state.Dependency = new CopyFromTransformJob()
            {
                LocalTransforms = _localTransformLookup,
                EntityLookup = singleton.EntityLookup
            }.ScheduleReadOnly(singleton.TransformAccessArray, 256, state.Dependency);
        }
    }
}
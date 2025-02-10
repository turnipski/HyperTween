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
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial struct CopyToTransformAccessArraySystem : ISystem
    {
        [BurstCompile]
        struct Job : IJobParallelForTransform
        {
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorld;
            [ReadOnly] public NativeList<Entity> Entities;

            public unsafe void Execute(int index, TransformAccess transform)
            {
                var ltw = LocalToWorld[Entities[index]];
                transform.localPosition = ltw.Position;

                // We need to use the safe version as the vectors will not be normalized if there is some scale
                transform.localRotation = quaternion.LookRotationSafe(ltw.Forward, ltw.Up);
                
                var mat = *(Matrix4x4*) &ltw;
                transform.localScale = mat.lossyScale;
            }
        }

        private ComponentLookup<LocalToWorld> _localToWorkLookup;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TransformAccessSingleton>();
            
            _localToWorkLookup = state.GetComponentLookup<LocalToWorld>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var singleton = SystemAPI.GetSingleton<TransformAccessSingleton>();

            _localToWorkLookup.Update(ref state);

            state.Dependency = new Job()
            {
                LocalToWorld = _localToWorkLookup,
                Entities = singleton.EntityLookup
            }.Schedule(singleton.TransformAccessArray, state.Dependency);
        }
    }
}
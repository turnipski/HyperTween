using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Update.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace HyperTween.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    partial struct TweenProcessDestroyRequestsSystem : ISystem
    {
        private EntityQuery _query;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            
            _query = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenRequestDestroy>());
            
            state.RequireForUpdate(_query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            ecb.DestroyEntity(_query, EntityQueryCaptureMode.AtPlayback);
        }
    }
}
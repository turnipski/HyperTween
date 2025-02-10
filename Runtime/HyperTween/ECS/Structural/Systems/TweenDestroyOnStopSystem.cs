using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Update.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace HyperTween.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
    [UpdateBefore(typeof(RemoveTweenPlayingSystem))]
    [BurstCompile]
    partial struct TweenDestroyOnStopSystem : ISystem
    {
        private EntityQuery _query;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CleanTweenStructuralChangeECBSystem.Singleton>();
            
            _query = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenPlaying>()
                .WithNone<TweenRequestPlaying, TweenAllowReuse>());
            
            state.RequireForUpdate(_query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<CleanTweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            // We don't destroy immediately so that zero duration tweens can perform at least one output
            ecb.AddComponent<TweenRequestDestroy>(_query, EntityQueryCaptureMode.AtPlayback);
        }
    }
}
using HyperTween.ECS.Sequencing.Components;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Structural.Systems;
using HyperTween.ECS.Update.Components;
using Unity.Burst;
using Unity.Entities;

namespace HyperTween.ECS.Sequencing.Systems
{
    [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
    [BurstCompile]
    partial struct TweenDestroyTargetOnStopSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CleanTweenStructuralChangeECBSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<CleanTweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var tweenTarget in SystemAPI
                         .Query<RefRO<TweenTarget>>()
                         .WithAll<TweenPlaying, TweenDestroyTargetOnStop>()
                         .WithNone<TweenRequestPlaying>())
            {
                ecb.DestroyEntity(tweenTarget.ValueRO.Target);
            }
        }
    }
}
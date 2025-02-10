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
    partial struct TweenPlayOnStopSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TweenStructuralChangeECBSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (tweenPlayOnStop, tweenDurationOverflow) in SystemAPI
                         .Query<RefRO<TweenPlayOnStop>, RefRO<TweenDurationOverflow>>()
                         .WithAll<TweenPlaying>()
                         .WithNone<TweenRequestPlaying>())
            {
                ecb.AddComponent(tweenPlayOnStop.ValueRO.Target, new TweenRequestPlaying()
                {
                    DurationOverflow = tweenDurationOverflow.ValueRO.Value
                });
            }
        }
    }
}
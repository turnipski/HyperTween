using HyperTween.ECS.Sequencing.Components;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Structural.Systems;
using Unity.Burst;
using Unity.Entities;

namespace HyperTween.ECS.Sequencing.Systems
{
    [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    partial struct TweenPlayOnPlaySystem : ISystem
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

            foreach (var (tweenPlayOnPlay, tweenRequestPlaying) in SystemAPI
                         .Query<RefRO<TweenPlayOnPlay>, RefRO<TweenRequestPlaying>>()
                         .WithNone<TweenPlaying>())
            {
                ecb.AddComponent(tweenPlayOnPlay.ValueRO.Target, new TweenRequestPlaying()
                {
                    DurationOverflow = tweenRequestPlaying.ValueRO.DurationOverflow
                });
            }
        }
    }
}
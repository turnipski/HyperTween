using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Structural.Systems;
using HyperTween.ECS.Update.Components;
using Unity.Burst;
using Unity.Entities;

namespace HyperTween.ECS.Update.Systems
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateTweenSystemGroup))]
    [BurstCompile]
    partial struct UpdateTweenTimers : ISystem
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
            var dt = SystemAPI.Time.DeltaTime;

            foreach (var (tweenTimer, tweenDuration, tweenDurationOverflow, entity) in SystemAPI
                         .Query<RefRW<TweenTimer>, RefRO<TweenDuration>, RefRW<TweenDurationOverflow>>()
                         .WithAll<TweenPlaying>()
                         .WithEntityAccess())
            {
                tweenTimer.ValueRW.Value += dt;
                
                if (tweenTimer.ValueRO.Value < tweenDuration.ValueRO.Value)
                {
                    continue;
                }

                tweenDurationOverflow.ValueRW.Value = tweenTimer.ValueRW.Value - tweenDuration.ValueRO.Value;
                tweenTimer.ValueRW.Value = tweenDuration.ValueRO.Value;

                ecb.RemoveComponent<TweenRequestPlaying>(entity);
            }
        }
    }
}

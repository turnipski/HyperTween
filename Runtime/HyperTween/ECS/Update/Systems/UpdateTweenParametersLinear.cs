using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Update.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace HyperTween.ECS.Update.Systems
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateTweenSystemGroup))]
    [UpdateBefore(typeof(TweenOutputSystemGroup))]
    [UpdateAfter(typeof(UpdateTweenTimers))]
    [BurstCompile]
    partial struct UpdateTweenParametersLinear : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (tweenParameter, tweenTimer, tweenDuration) in SystemAPI
                         .Query<RefRW<TweenParameter>, RefRO<TweenTimer>, RefRO<TweenDuration>>()
                         .WithAll<TweenPlaying>()
                         .WithOptions(EntityQueryOptions.FilterWriteGroup))
            {
                tweenParameter.ValueRW.Value = tweenTimer.ValueRO.Value * tweenDuration.ValueRO.InverseValue;
            }
            
            // Tweens with no duration need to have their parameter forced to 1
            foreach (var tweenParameter in SystemAPI
                         .Query<RefRW<TweenParameter>>()
                         .WithAny<TweenPlaying, TweenForceOutput>()
                         .WithNone<TweenDuration>()
                         .WithOptions(EntityQueryOptions.FilterWriteGroup))
            {
                tweenParameter.ValueRW.Value = 1f;
            }
        }
    }
}
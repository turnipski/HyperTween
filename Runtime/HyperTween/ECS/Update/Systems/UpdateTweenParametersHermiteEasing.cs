using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Update.Components;
using Unity.Burst;
using Unity.Entities;

namespace HyperTween.ECS.Update.Systems
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateTweenSystemGroup))]
    [UpdateBefore(typeof(TweenOutputSystemGroup))]
    [BurstCompile]
    partial struct UpdateTweenParametersHermiteEasing : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (tweenParameter, tweenTimer, tweenDuration, tweenHermiteEasing) in SystemAPI
                         .Query<RefRW<TweenParameter>, RefRO<TweenTimer>, RefRO<TweenDuration>, RefRO<TweenHermiteEasing>>()
                         .WithAll<TweenPlaying>()
                         .WithOptions(EntityQueryOptions.FilterWriteGroup))
            {
                var linearParameter = (float)(tweenTimer.ValueRO.Value * tweenDuration.ValueRO.InverseValue);
                
                tweenParameter.ValueRW.Value = tweenHermiteEasing.ValueRO.Interpolate(linearParameter);
            }
        }
    }
}
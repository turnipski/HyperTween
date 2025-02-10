using HyperTween.ECS.Sequencing.Components;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Update.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace HyperTween.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
    [BurstCompile]
    partial struct AddTweenPlayingSystem : ISystem
    {
        private EntityQuery _playTweensQuery;
        private EntityQuery _zeroDurationTweensQuery;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CleanTweenStructuralChangeECBSystem.Singleton>();
            state.RequireForUpdate<TweenStructuralChangeECBSystem.Singleton>();
            
            _playTweensQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenRequestPlaying>()
                .WithNone<TweenPlaying>());
            
            _zeroDurationTweensQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenRequestPlaying>()
                // Tweens with TweenStopOnJoin won't have a duration but shouldn't be stopped
                .WithNone<TweenDuration, TweenStopOnJoin>());
            
            state.RequireForUpdate(_playTweensQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var cleanEcbSingleton = SystemAPI.GetSingleton<CleanTweenStructuralChangeECBSystem.Singleton>();
            var cleanEcb = cleanEcbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var ecbSingleton = SystemAPI.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            cleanEcb.AddComponent<TweenPlaying>(_playTweensQuery, EntityQueryCaptureMode.AtPlayback);

            foreach (var (tweenRequestPlaying, tweenDuration, tweenTimer, tweenDurationOverflow, entity) in SystemAPI
                         .Query<RefRO<TweenRequestPlaying>, RefRO<TweenDuration>, RefRW<TweenTimer>, RefRW<TweenDurationOverflow>>()
                         .WithNone<TweenPlaying>()
                         .WithEntityAccess())
            {
                tweenTimer.ValueRW.Value = tweenRequestPlaying.ValueRO.DurationOverflow;

                // Need to account for very short tweens having so much overflow that they finish immediately
                if (tweenTimer.ValueRW.Value < tweenDuration.ValueRO.Value)
                {
                    continue;
                }
                
                // If we already reached the duration, remove RequestTweenPlaying to stop the tween immediately

                tweenTimer.ValueRW.Value = tweenDuration.ValueRO.Value;
                tweenDurationOverflow.ValueRW.Value = tweenTimer.ValueRW.Value - tweenDuration.ValueRO.Value;
                
                ecb.RemoveComponent<TweenRequestPlaying>(entity);
            }

            cleanEcb.AddComponent<TweenForceOutput>(_zeroDurationTweensQuery, EntityQueryCaptureMode.AtPlayback);
            ecb.RemoveComponent<TweenRequestPlaying>(_zeroDurationTweensQuery, EntityQueryCaptureMode.AtPlayback);
        }
    }
}
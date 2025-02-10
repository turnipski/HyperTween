using HyperTween.ECS.ConflictDetection.Components;
using HyperTween.ECS.Structural.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace HyperTween.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
    [BurstCompile]
    partial struct RemoveTweenPlayingSystem : ISystem
    {
        private EntityQuery _nonConflictedQuery;
        private EntityQuery _conflictedQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CleanTweenStructuralChangeECBSystem.Singleton>();
            
            _nonConflictedQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenPlaying>()
                .WithNone<TweenRequestPlaying>());
            
            _conflictedQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenPlaying, TweenConflicted>()
                .WithNone<TweenRequestPlaying>());
            
            state.RequireAnyForUpdate(_nonConflictedQuery, _conflictedQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<CleanTweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            ecb.RemoveComponent<TweenPlaying>(_nonConflictedQuery, EntityQueryCaptureMode.AtPlayback);
            ecb.RemoveComponent<TweenConflicted>(_conflictedQuery, EntityQueryCaptureMode.AtPlayback);
        }
    }
}
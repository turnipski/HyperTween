using HyperTween.ECS.Sequencing.Components;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Structural.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace HyperTween.ECS.Sequencing.Systems
{
    [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
    [BurstCompile]
    partial struct TweenForkOnPlaySystem : ISystem
    {
        [BurstCompile]
        public partial struct Job : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

            [BurstCompile]
            private void Execute(in DynamicBuffer<TweenForkOnPlay> forkOnPlays, in TweenRequestPlaying tweenRequestPlaying, [ChunkIndexInQuery]int chunkIndexInQuery)
            {
                foreach (var tweenScatterOnPlay in forkOnPlays)
                {
                    EntityCommandBuffer.AddComponent(chunkIndexInQuery, tweenScatterOnPlay.Target, tweenRequestPlaying);
                }
            }
        }

        private EntityQuery _query;
        private BufferLookup<TweenForkOnPlay> _bufferLookup;

        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenForkOnPlay, TweenRequestPlaying>()
                .WithNone<TweenPlaying>());

            _bufferLookup = state.GetBufferLookup<TweenForkOnPlay>();
            
            state.RequireForUpdate<TweenStructuralChangeECBSystem.Singleton>();
            state.RequireForUpdate(_query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            
            _bufferLookup.Update(ref state);

            new Job()
            {
                EntityCommandBuffer = ecb.AsParallelWriter()
            }.ScheduleParallel(_query);

        }
    }
}
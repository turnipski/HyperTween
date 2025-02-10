using HyperTween.ECS.Sequencing.Components;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Structural.Systems;
using HyperTween.ECS.Update.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HyperTween.ECS.Sequencing.Systems
{
    [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
    [BurstCompile]
    partial struct TweenSignalJoinOnStopSystem : ISystem
    {
        private struct GatherData
        {
            public int Count;
            public float MaxDurationOverflow;
        }
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TweenStructuralChangeECBSystem.Singleton>();
            state.RequireForUpdate<TweenSignalJoinOnStop>();
        }
 
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            NativeHashMap<Entity, GatherData> gatherMap = new NativeHashMap<Entity, GatherData>(16, Allocator.Temp);

            try
            {
                foreach (var (tweenIncrementGatherOnStop, tweenDurationOverflow) in SystemAPI
                             .Query<RefRO<TweenSignalJoinOnStop>, RefRO<TweenDurationOverflow>>()
                             .WithAll<TweenPlaying>()
                             .WithNone<TweenRequestPlaying>())
                {
                    var tweenEntity = tweenIncrementGatherOnStop.ValueRO.Target;
                    
                    if (gatherMap.TryGetValue(tweenEntity, out var gatherData))
                    {
                        gatherData.Count++;
                        gatherData.MaxDurationOverflow = math.max(gatherData.MaxDurationOverflow, tweenDurationOverflow.ValueRO.Value);
                        
                        gatherMap[tweenEntity] = gatherData;
                    }
                    else
                    {
                        gatherMap.Add(tweenEntity, new GatherData()
                        {
                            Count = 1,
                            MaxDurationOverflow = tweenDurationOverflow.ValueRO.Value
                        });
                    }
                }

                foreach (var pair in gatherMap)
                {
                    var existingGather = SystemAPI.GetComponent<TweenStopOnJoin>(pair.Key);
                    existingGather.CurrentSignals += pair.Value.Count;
                    
                    ecb.SetComponent(pair.Key, existingGather);

                    if (existingGather.CurrentSignals >= existingGather.RequiredSignals)
                    {
                        // We only care about the duration overflow in this frame
                        ecb.SetComponent(pair.Key, new TweenDurationOverflow()
                        {
                            Value = pair.Value.MaxDurationOverflow
                        });
                        
                        ecb.RemoveComponent<TweenRequestPlaying>(pair.Key);
                    }
                    
                    if (existingGather.CurrentSignals > existingGather.RequiredSignals)
                    {
                        Debug.LogError($"Gather exceeded threshold: {pair.Key}");
                    }
                }
            }
            finally
            {
                gatherMap.Dispose();
            }
        }
    }
}
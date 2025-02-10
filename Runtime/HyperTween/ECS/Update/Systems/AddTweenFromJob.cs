using HyperTween.ECS.Update.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace HyperTween.ECS.Update.Systems
{
        [BurstCompile]
        public struct AddTweenFromJob<TTweenFrom, TTweenTo, TTargetComponent, TValue> : IJobChunk
            where TTweenFrom : unmanaged, ITweenFrom<TValue>
            where TTweenTo : unmanaged, ITweenTo<TTargetComponent, TValue>
            where TTargetComponent : unmanaged, IComponentData
        {
            [ReadOnly]
            public ComponentTypeHandle<TTargetComponent> TargetComponentHandle;
            
            [ReadOnly]
            public ComponentTypeHandle<TweenTarget> TweenTargetComponentHandle;

            [ReadOnly]
            public ComponentLookup<TTargetComponent> TargetComponents;
            
            public EntityTypeHandle EntityTypeHandle;
            
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            
            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(EntityTypeHandle);

                if (chunk.Has(ref TweenTargetComponentHandle))
                {
                    var tweenTargets = chunk.GetNativeArray(ref TweenTargetComponentHandle);

                    var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                    while(enumerator.NextEntityIndex(out var i))
                    {
                        var tweenTarget = tweenTargets[i];
                        if (!TargetComponents.HasComponent(tweenTarget.Target))
                        {
                            Debug.LogError($"TweenTarget does not have required target component");
                            //Debug.LogError($"TweenTarget {tweenTarget.Target} does not have required target component: {typeof(TTargetComponent).Name}");
                            continue;
                        }
                    
                        var targetComponent = TargetComponents[tweenTarget.Target];
                        var from = default(TTweenTo).Read(in targetComponent);

                        var tweenFrom = default(TTweenFrom);
                        tweenFrom.SetValue(from);
                    
                        EntityCommandBuffer.AddComponent(unfilteredChunkIndex, entities[i], tweenFrom);
                    }
                }
                else if(chunk.Has(ref TargetComponentHandle))
                {
                    var targetComponents = chunk.GetNativeArray(ref TargetComponentHandle);

                    var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                    while(enumerator.NextEntityIndex(out var i))
                    {
                        var targetComponent = targetComponents[i];
                        var from = default(TTweenTo).Read(in targetComponent);

                        var tweenFrom = default(TTweenFrom);
                        tweenFrom.SetValue(from);
                    
                        EntityCommandBuffer.AddComponent(unfilteredChunkIndex, entities[i], tweenFrom);
                    }
                }
                else
                {
                    Debug.LogError("Expected TargetComponent");
                }
            }
        }
    
}
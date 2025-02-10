using HyperTween.ECS.Update.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace HyperTween.ECS.Update.Systems
{
    [BurstCompile]
    public struct WriteTweenOutputJob<TTweenFrom, TTweenTo, TTargetComponent, TValue> : IJobChunk
        where TTweenFrom : unmanaged, ITweenFrom<TValue> 
        where TTweenTo : unmanaged, ITweenTo<TTargetComponent, TValue> 
        where TTargetComponent : unmanaged, IComponentData
    {
        [ReadOnly]
        public ComponentTypeHandle<TTweenFrom> TweenFromHandle;
        [ReadOnly]
        public ComponentTypeHandle<TTweenTo> TweenToHandle;
        [ReadOnly]
        public ComponentTypeHandle<TweenParameter> TweenParameterHandle;
        [ReadOnly]
        public ComponentTypeHandle<TweenTarget> TweenTargetHandle;
        
        public ComponentTypeHandle<TTargetComponent> TargetComponentHandle;
             
        // Conflict detection ensures that each Entity is only pointed to by a single TweenTarget for a given component
        [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public ComponentLookup<TTargetComponent> TargetComponents;
        
        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var tweenFroms = chunk.GetNativeArray(ref TweenFromHandle);
            var tweenTos = chunk.GetNativeArray(ref TweenToHandle);
            var tweenParameters = chunk.GetNativeArray(ref TweenParameterHandle);

            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

            if (chunk.Has(ref TweenTargetHandle))
            {
                var tweenTargets = chunk.GetNativeArray(ref TweenTargetHandle);
                while (enumerator.NextEntityIndex(out var i))
                {
                    var from = tweenFroms[i].GetValue();
                    var toComponent = tweenTos[i];
                    var to = toComponent.GetValue();
                    var interpolated = toComponent.Lerp(from, to, tweenParameters[i].Value);
                    
                    var tweenTarget = tweenTargets[i];
                     
                    var targetComponent = TargetComponents[tweenTarget.Target];
                    toComponent.Write(ref targetComponent, interpolated);
                    TargetComponents[tweenTarget.Target] = targetComponent;
                }
            }
            else
            {            
                var targetComponents = chunk.GetNativeArray(ref TargetComponentHandle);
                
                while (enumerator.NextEntityIndex(out var i))
                {
                    var from = tweenFroms[i].GetValue();
                    var toComponent = tweenTos[i];
                    var to = toComponent.GetValue();
                    var interpolated = toComponent.Lerp(from, to, tweenParameters[i].Value);

                    var targetComponent = targetComponents[i];
                    toComponent.Write(ref targetComponent, interpolated);
                    targetComponents[i] = targetComponent;
                }
            }
        }
    }
}
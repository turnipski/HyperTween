using System.Buffers;
using HyperTween.API;
using HyperTween.TweenBuilders;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;

namespace HyperTween.Modules.Transform
{
    public static class TweenTransformBatchExtensions
    {
        public static BatchTweenHandle<T> WithManagedTransformOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<TransformInstanceId> transformInstanceIds)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.BatchTweenBuilder.AddComponents(transformInstanceIds);
            return tweenHandle;
        }
        
        public static NativeArray<TransformInstanceId> ToTransformInstanceIds(this UnityEngine.Transform[] transforms, Allocator allocator)
        {
            using var profilerMarker = new ProfilerMarker("ToTransformInstanceIds").Auto();
            
            var transformInstanceIds = new NativeArray<TransformInstanceId>(transforms.Length, allocator);
            
            for (var i = 0; i < transforms.Length; i++)
            {
                transformInstanceIds[i] = new TransformInstanceId()
                {
                    Value = transforms[i].GetInstanceID()
                };
            }

            return transformInstanceIds;
        }
    }
}
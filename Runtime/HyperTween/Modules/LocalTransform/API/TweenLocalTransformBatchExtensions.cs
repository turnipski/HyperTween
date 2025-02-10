using HyperTween.API;
using HyperTween.Auto.Components;
using HyperTween.Modules.LocalTransform.Components;
using HyperTween.Modules.Transform;
using HyperTween.TweenBuilders;
using Unity.Collections;
using Unity.Mathematics;

namespace HyperTween.Modules.LocalTransform.API
{
    public static class TweenLocalTransformBatchExtensions
    {
        public static BatchTweenHandle<T> WithLocalPositionOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<float3> froms, NativeArray<float3> tos)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.BatchTweenBuilder.AddComponents(froms.Reinterpret<TweenLocalPositionFrom>());
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalPosition>());

            return tweenHandle;
        }
        
        public static BatchTweenHandle<T> WithLocalPositionOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<float3> tos)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalPosition>());

            return tweenHandle;
        }
        
        public static BatchTweenHandle<T> WithLocalRotationOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<quaternion> froms, NativeArray<quaternion> tos)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.BatchTweenBuilder.AddComponents(froms.Reinterpret<TweenLocalRotationFrom>());
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalRotation>());

            return tweenHandle;
        }
        
        public static BatchTweenHandle<T> WithLocalRotationOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<quaternion> tos)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalRotation>());

            return tweenHandle;
        }
        
        public static BatchTweenHandle<T> WithLocalUniformScaleOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<float> froms, NativeArray<float> tos)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.BatchTweenBuilder.AddComponents(froms.Reinterpret<TweenLocalUniformScaleFrom>());
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalUniformScale>());

            return tweenHandle;
        }
        
        public static BatchTweenHandle<T> WithLocalUniformScaleOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<float> tos)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalUniformScale>());

            return tweenHandle;
        }
    }
}
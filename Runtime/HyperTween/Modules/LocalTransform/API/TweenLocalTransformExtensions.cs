using HyperTween.API;
using HyperTween.Auto.Components;
using HyperTween.Modules.LocalTransform.Components;
using HyperTween.Modules.Transform;
using HyperTween.TweenBuilders;
using Unity.Mathematics;

namespace HyperTween.Modules.LocalTransform.API
{
    public static class TweenLocalTransformExtensions
    {
        public static TweenHandle<T> WithLocalPositionOutput<T>(this TweenHandle<T> tweenHandle, float3 from, float3 to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalPositionFrom()
            {
                Value = from
            });
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalPosition()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithLocalPositionOutput<T>(this TweenHandle<T> tweenHandle, float3 to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalPosition()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithLocalRotationOutput<T>(this TweenHandle<T> tweenHandle, quaternion from, quaternion to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalRotationFrom()
            {
                Value = from
            });
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalRotation()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithLocalRotationOutput<T>(this TweenHandle<T> tweenHandle, quaternion to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalRotation()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithLocalUniformScaleOutput<T>(this TweenHandle<T> tweenHandle, float from, float to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalUniformScaleFrom()
            {
                Value = from
            });
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalUniformScale()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithLocalUniformScaleOutput<T>(this TweenHandle<T> tweenHandle, float to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalUniformScale()
            {
                Value = to
            });

            return tweenHandle;
        }
    }
}
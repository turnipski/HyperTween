using HyperTween.API;
using HyperTween.TweenBuilders;

namespace HyperTween.Modules.Transform.API
{
    public static class TweenTransformExtensions
    {
        public static TweenHandle<TBuilder> WithTransform<TBuilder>(this TweenHandle<TBuilder> tweenHandle, UnityEngine.Transform transform) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponent<Unity.Transforms.LocalTransform>(tweenHandle.Entity);
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TransformInstanceId()
            {
                Value = transform.GetInstanceID()
            });
            
            return tweenHandle;
        }
    }
}
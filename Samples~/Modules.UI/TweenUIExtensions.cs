using HyperTween.API;
using HyperTween.Auto.Components;
using HyperTween.Modules.UI;
using HyperTween.TweenBuilders;
using UnityEngine;
using UnityEngine.UI;

namespace HyperTween.Modules.Transform
{
    public static class TweenUIExtensions
    {
        public static TweenHandle<T> WithColor<T>(this TweenHandle<T> tweenHandle, Image image, Color to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new ImageInstanceId()
            {
                Value = image.GetInstanceID()
            });
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, image);
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenImageColour()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithColor<T>(this TweenHandle<T> tweenHandle, Image image, Color from, Color to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new ImageInstanceId()
            {
                Value = image.GetInstanceID()
            });
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, image);

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenImageColourFrom()
            {
                Value = from
            });
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenImageColour()
            {
                Value = to
            });

            return tweenHandle;
        }
    }
}
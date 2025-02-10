using System;
using HyperTween.API;
using HyperTween.Auto.Components;
using HyperTween.TweenBuilders;

namespace HyperTween.Modules.InvokeAction.API
{
    public static class TweenInvokeActionExtensions
    {
        public static TweenHandle<TBuilder> InvokeActionOnPlay<TBuilder>(this TweenHandle<TBuilder> tweenHandle, Action<TweenInvokeActionOnPlay.Context> action) where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, new TweenInvokeActionOnPlay()
            {
                Action = action
            });
            
            return tweenHandle;
        }
        
        public static TweenHandle<TBuilder> InvokeActionOnStop<TBuilder>(this TweenHandle<TBuilder> tweenHandle, Action<TweenInvokeActionOnStop.Context> action) where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, new TweenInvokeActionOnStop()
            {
                Action = action
            });
            
            return tweenHandle;
        }
    }
}
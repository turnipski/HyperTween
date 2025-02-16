using Cysharp.Threading.Tasks;
using HyperTween.API;
using HyperTween.Auto.Components;
using HyperTween.TweenBuilders;

namespace HyperTween.Modules.UniTask.API
{
    public static class TweenUniTaskExtensions
    {
        public static Cysharp.Threading.Tasks.UniTask AsUniTask<TBuilder>(this TweenHandle<TBuilder> tweenHandle) where TBuilder : unmanaged, ITweenBuilder
        {
            var tcs = new UniTaskCompletionSource();
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, new TweenCompleteTaskOnStop()
            {
                TaskCompletionSource = tcs
            });
            
            return tcs.Task;
        }
    }
}
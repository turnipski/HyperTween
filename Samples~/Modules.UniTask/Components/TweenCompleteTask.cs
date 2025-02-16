using Cysharp.Threading.Tasks;
using HyperTween.ECS.Invoke.Components;
using HyperTween.ECS.Update.Components;
using Unity.Entities;

namespace HyperTween.Modules.UniTask.Components
{
    public class TweenCompleteTask : ITweenInvokeOnStop
    {
        public UniTaskCompletionSource TaskCompletionSource;

        public void Invoke()
        {
            TaskCompletionSource.TrySetResult();
        }
        
    }
}
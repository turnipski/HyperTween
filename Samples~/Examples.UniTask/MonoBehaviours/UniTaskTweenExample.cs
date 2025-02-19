using HyperTween.API;
using HyperTween.Modules.UniTask.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class UniTaskTweenExample : MonoBehaviour
    {
        private async void Start()
        {
            var tweenHandle = (TweenHandle)HyperTweenFactory.CreateTween()
                .WithDuration(1f)
                .Play();
            
            await tweenHandle
                .AsUniTask();
            
            Debug.Log("Tween task completed");
        }
    }
}
using HyperTween.API;
using HyperTween.Modules.UniTask.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class UniTaskTweenExample : MonoBehaviour
    {
        private async void Start()
        {
            await HyperTweenFactory.CreateTween()
                .WithDuration(5f)
                .Play()
                .AsUniTask();
            
            Debug.Log("Tween task completed");
        }
    }
}
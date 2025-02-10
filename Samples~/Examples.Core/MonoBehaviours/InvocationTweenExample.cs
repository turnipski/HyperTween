using HyperTween.API;
using HyperTween.Modules.InvokeAction.API;
using HyperTween.Modules.LocalTransform.API;
using HyperTween.Modules.Transform.API;
using HyperTween.TweenBuilders;
using UnityEngine;

namespace HyperTween.Examples
{
    public class InvocationTweenExample : MonoBehaviour
    {
        void Start()
        {
            CreateTween(HyperTweenFactory.Get());
        }

        void CreateTween<T>(TweenFactory<T> tweenFactory) where T : unmanaged, ITweenBuilder
        {
            tweenFactory.CreateTween()
                .WithDuration(1f)
                .WithTransform(transform)
                .WithLocalPositionOutput(5f * Random.onUnitSphere)
                .WithEaseInOut()
                .InvokeActionOnPlay(_ =>
                {
                    Debug.Log("Tween played!");
                })
                .InvokeActionOnStop(context =>
                {
                    Debug.Log("Tween stopped!");
                    CreateTween(context.EntityCommandBuffer.CreateTweenFactory());
                })
                .Play();
        }
    }
}
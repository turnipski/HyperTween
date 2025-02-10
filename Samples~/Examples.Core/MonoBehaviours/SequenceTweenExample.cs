using HyperTween.API;
using HyperTween.Modules.InvokeAction.API;
using HyperTween.Modules.LocalTransform.API;
using HyperTween.Modules.Transform.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class SequenceTweenExample : MonoBehaviour
    {
        void Start()
        {
            var factory = HyperTweenFactory.Get();

            var serialTweenFactory = factory.Serial();
            
            serialTweenFactory
                .Append(factory.CreateTween()
                    .WithDuration(1f)
                    .WithTransform(transform)
                    .WithLocalPositionOutput(new Vector3(5f, 0f, 0f)))
                .Append(factory.CreateTween()
                    .WithDuration(1f)
                    .WithTransform(transform)
                    .WithLocalPositionOutput(new Vector3(-5f, 0f, 0f)));

            // Build creates a tween like any other, that could be added to another sequence
            var serialTween = serialTweenFactory
                .Build()
                .InvokeActionOnStop(_ => Debug.Log("Serial tween stopped..."));
            
            serialTween.Play();
        }
    }
}
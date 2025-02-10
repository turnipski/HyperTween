using System.Linq;
using HyperTween.API;
using HyperTween.Modules.InvokeAction.API;
using HyperTween.Modules.LocalTransform.API;
using HyperTween.Modules.Transform.API;
using HyperTween.TweenBuilders;
using UnityEngine;

namespace HyperTween.Examples
{
    public class CompositionTweenExample : MonoBehaviour
    {
        private void Start()
        {
            var factory = HyperTweenFactory.Get(true);
            
            var transforms = Enumerable.Range(1, 20)
                .Select(_ => GameObject.CreatePrimitive(PrimitiveType.Cube).transform)
                .ToArray();

            // Create a serial sequence factory to add child tweens to
            var serial = factory.Serial();
            for (var i = 0; i < 10; i++)
            {
                var randomPositionsTween = CreateRandomPositionsTween(factory, transforms);
                
                serial.Append(randomPositionsTween);
            }

            // Builds the serial sequence factory so that it becomes a regular tween
            serial.Build()
                // Destroys all the transforms when the serial sequence stops
                .InvokeActionOnStop(_ =>
                {
                    foreach (var t in transforms)
                    {
                        Destroy(t.gameObject);
                    }
                })
                .Play();
        }

        private static TweenHandle<T> CreateRandomPositionsTween<T>(TweenFactory<T> factory, Transform[] transforms) 
            where T : unmanaged, ITweenBuilder
        {
            // Creates a tween for each transform that moves it to a random position
            var tweens = transforms
                .Select(t => CreateRandomPositionTween(factory, t));

            // Creates a parallel sequence so that all the transforms move at the same time
            return factory.Parallel()
                .Append(tweens)
                .Build();
        }
        
        private static TweenHandle<T> CreateRandomPositionTween<T>(TweenFactory<T> factory, Transform t) 
            where T : unmanaged, ITweenBuilder
        {
            return factory.CreateTween()
                .WithDuration(1f)
                .WithTransform(t)
                .WithLocalPositionOutput(Random.onUnitSphere);
        }
    }
}
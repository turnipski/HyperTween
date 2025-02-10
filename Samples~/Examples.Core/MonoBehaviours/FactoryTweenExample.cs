using HyperTween.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class FactoryTweenExample : MonoBehaviour
    {
        void Start()
        {
            var factory = HyperTweenFactory.Get();
            
            factory.CreateTween()
                .WithDuration(1f)
                .Play();
            
            factory.CreateTween()
                .WithDuration(2f)
                .Play();
        }
    }

}

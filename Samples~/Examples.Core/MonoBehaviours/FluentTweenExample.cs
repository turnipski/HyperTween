using HyperTween.API;
using HyperTween.Modules.LocalTransform.API;
using HyperTween.Modules.Transform.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class FluentTweenExample : MonoBehaviour
    {
        void Start()
        {
            HyperTweenFactory.CreateTween()
                .WithDuration(1f)
                .WithTransform(transform)
                .WithLocalPositionOutput(new Vector3(1, 2, 3))
                .WithLocalRotationOutput(Quaternion.Euler(10, 20, 30))
                .WithLocalUniformScaleOutput(2f)
                .Play();
        }
    }

}
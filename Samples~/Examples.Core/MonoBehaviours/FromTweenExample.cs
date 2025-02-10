using HyperTween.API;
using HyperTween.Modules.LocalTransform.API;
using HyperTween.Modules.Transform.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class FromTweenExample : MonoBehaviour
    {
        void Start()
        {
            HyperTweenFactory.CreateTween()
                .WithDuration(1f)
                .WithTransform(transform)
                .WithLocalPositionOutput(from: new Vector3(1, 2, 3), to: new Vector3(2, 3, 4))
                .Play();
        }
    }
}
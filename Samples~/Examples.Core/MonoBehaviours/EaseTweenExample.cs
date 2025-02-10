using HyperTween.API;
using HyperTween.Modules.LocalTransform.API;
using HyperTween.Modules.Transform.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class EaseTweenExample : MonoBehaviour
    {
        void Start()
        {
            HyperTweenFactory.CreateTween()
                .WithDuration(1f)
                .WithTransform(transform)
                .WithEaseIn(strength: 2f)
                .WithEaseOut(strength: 3f)
                .WithEaseInOut()
                .WithHermiteEasing(0f, 3f)
                .WithLocalPositionOutput(to: new Vector3(1, 2, 3))
                .Play();
        }
    }
}
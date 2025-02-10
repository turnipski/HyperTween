using HyperTween.API;
using HyperTween.Modules.LocalTransform.API;
using HyperTween.Modules.Transform.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class CreateTweenExample : MonoBehaviour
    {
        void Start()
        {
            HyperTweenFactory.CreateTween()
                // Sets how long the tween will last
                .WithDuration(1f)
                // Sets which transform to move
                .WithTransform(transform)
                // Sets where the transform should move to
                .WithLocalPositionOutput(new Vector3(1, 2, 3))
                // Plays the tween
                .Play();
        }
    }

}

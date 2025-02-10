using Unity.Entities;
using UnityEngine.Scripting;

namespace HyperTween.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup))]
    // Allow Tweens to naturally stop before we attempt to start any, otherwise we may get false positive conflicts
    [UpdateAfter(typeof(OnTweenStopSystemGroup))]
    public partial class OnTweenPlaySystemGroup : ComponentSystemGroup
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        [Preserve]
        public OnTweenPlaySystemGroup()
        {
        }
    }
}
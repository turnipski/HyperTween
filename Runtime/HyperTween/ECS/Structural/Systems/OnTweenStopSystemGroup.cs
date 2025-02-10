using Unity.Entities;
using UnityEngine.Scripting;

namespace HyperTween.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup))]
    [UpdateBefore(typeof(TweenStructuralChangeECBSystem))]
    public partial class OnTweenStopSystemGroup : ComponentSystemGroup
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        [Preserve]
        public OnTweenStopSystemGroup()
        {
        }
    }
}
using HyperTween.ECS.Structural.Systems;
using HyperTween.Modules.Transform.Systems;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace HyperTween.ECS.Update.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TweenStructuralChangeSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class UpdateTweenSystemGroup : ComponentSystemGroup
    {
    }
    
    [UpdateInGroup(typeof(UpdateTweenSystemGroup))]
    [UpdateBefore(typeof(TweenOutputSystemGroup))]
    public partial class AddTweenFromSystemGroup : ComponentSystemGroup
    {
    }
}
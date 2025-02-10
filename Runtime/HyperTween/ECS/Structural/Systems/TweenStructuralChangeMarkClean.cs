using Unity.Burst;
using Unity.Entities;

namespace HyperTween.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup), OrderFirst = true)]
    [BurstCompile]
    public partial struct TweenStructuralChangeMarkClean : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Mark clean as soon as the group executes so that other systems can mark dirty
            state.World.GetExistingSystemManaged<TweenStructuralChangeSystemGroup>().MarkClean();
        }
    }
}
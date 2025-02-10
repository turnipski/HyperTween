using Unity.Entities;

namespace HyperTween.ECS.Sequencing.Components
{
    public struct TweenSignalJoinOnStop : IComponentData
    {
        public Entity Target;
    }
}
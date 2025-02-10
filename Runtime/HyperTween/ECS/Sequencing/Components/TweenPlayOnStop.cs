using Unity.Entities;

namespace HyperTween.ECS.Sequencing.Components
{
    public struct TweenPlayOnStop : IComponentData
    {
        public Entity Target;
    }
}
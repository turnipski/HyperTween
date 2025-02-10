using Unity.Entities;

namespace HyperTween.ECS.Sequencing.Components
{
    public struct TweenPlayOnPlay : IComponentData
    {
        public Entity Target;
    }
}
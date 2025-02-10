using Unity.Entities;

namespace HyperTween.ECS.Sequencing.Components
{
    [InternalBufferCapacity(4)]
    public struct TweenForkOnPlay : IBufferElementData
    {
        public Entity Target;
    }
}
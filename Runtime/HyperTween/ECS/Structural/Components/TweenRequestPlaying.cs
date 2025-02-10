using Unity.Entities;

namespace HyperTween.ECS.Structural.Components
{
    public struct TweenRequestPlaying : IComponentData
    {
        public float DurationOverflow;
    }
}
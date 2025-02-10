using Unity.Entities;

namespace HyperTween.ECS.Update.Components
{
    public struct TweenDurationOverflow : IComponentData
    {
        public float Value;
    }
}
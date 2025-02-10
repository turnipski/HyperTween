using Unity.Entities;

namespace HyperTween.ECS.Update.Components
{
    public struct TweenDuration : IComponentData
    {
        public float Value;
        public float InverseValue;

        public static TweenDuration Create(float duration)
        {
            return new TweenDuration()
            {
                Value = duration,
                InverseValue = 1.0f / duration
            };
        }
    }
}
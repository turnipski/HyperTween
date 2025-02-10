using Unity.Entities;

namespace HyperTween.ECS.Update.Components
{
    public struct TweenTarget : IComponentData
    {
        public Entity Target;
    }
}
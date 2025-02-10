using Unity.Entities;

namespace HyperTween.ECS.Update.Components
{
    public interface ITweenFrom<TValue> : IComponentData
    {
        public TValue GetValue();
        public void SetValue(TValue value);
    }
}
using Unity.Entities;

namespace HyperTween.ECS.Update.Components
{
    public interface ITweenTo<TTargetComponent, TValue> : IComponentData
    {
        public TValue GetValue();
        TValue Lerp(TValue from, TValue to, float parameter);
        TValue Read(in TTargetComponent component);
        void Write(ref TTargetComponent component, TValue value);
    }
}
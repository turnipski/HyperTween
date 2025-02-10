using Unity.Entities;

namespace HyperTween.ECS.Util
{
    public struct DynamicTypeInfo
    {
        public ComponentType ComponentType;
        public DynamicComponentTypeHandle DynamicComponentTypeHandle;

        public void Initialise<TComponentType>(ref SystemState state)
        {
            ComponentType = ComponentType.ReadWrite<TComponentType>();
            DynamicComponentTypeHandle = state.GetDynamicComponentTypeHandle(ComponentType);
        }
        
        public void InitialiseReadOnly<TComponentType>(ref SystemState state)
        {
            ComponentType = ComponentType.ReadOnly<TComponentType>();
            DynamicComponentTypeHandle = state.GetDynamicComponentTypeHandle(ComponentType);
        }
        
        public void Update(ref SystemState state)
        {
            DynamicComponentTypeHandle.Update(ref state);
        }
    }
}
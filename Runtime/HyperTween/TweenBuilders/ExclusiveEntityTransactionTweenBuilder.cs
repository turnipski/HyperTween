using Unity.Collections;
using Unity.Entities;

namespace HyperTween.TweenBuilders
{
    public struct ExclusiveEntityTransactionTweenBuilder : ITweenBuilder
    {
        private ExclusiveEntityTransaction _exclusiveEntityTransaction;
        private EntityArchetype _emptyArchetype;
        
        public ExclusiveEntityTransactionTweenBuilder(WorldUnmanaged worldUnmanaged, ExclusiveEntityTransaction exclusiveEntityTransaction)
        {
            WorldUnmanaged = worldUnmanaged;
            _exclusiveEntityTransaction = exclusiveEntityTransaction;
            _emptyArchetype = exclusiveEntityTransaction.CreateArchetype();
        }

        public WorldUnmanaged WorldUnmanaged { get; }

        public Entity CreateEntity()
        {
            return _exclusiveEntityTransaction.CreateEntity(_emptyArchetype);
        }

        public void DestroyEntity(Entity entity)
        {
            _exclusiveEntityTransaction.DestroyEntity(entity);
        }

        public void Instantiate(Entity prefab, NativeArray<Entity> entities)
        {
            _exclusiveEntityTransaction.Instantiate(prefab, entities);
        }

        public void SetName(Entity entity, in FixedString64Bytes name)
        {
            throw new System.NotImplementedException();
        }

        public void AddComponent<T>(Entity e) where T : unmanaged, IComponentData
        {
            _exclusiveEntityTransaction.AddComponent(e, ComponentType.ReadOnly<T>());
        }

        public void AddComponent<TComponent>(NativeArray<Entity> entities) where TComponent : unmanaged, IComponentData
        {
            foreach (var entity in entities)
            {
                _exclusiveEntityTransaction.AddComponent(entity, ComponentType.ReadOnly<TComponent>());
            }
        }

        public void AddComponent<TComponentData>(Entity e, TComponentData componentData) where TComponentData : unmanaged, IComponentData
        {
            _exclusiveEntityTransaction.AddComponent(e, ComponentType.ReadOnly<TComponentData>());
            _exclusiveEntityTransaction.SetComponentData(e, componentData);
        }

        public void AddComponent<TComponentData>(NativeArray<Entity> entities, TComponentData componentData) where TComponentData : unmanaged, IComponentData
        {
            foreach (var entity in entities)
            {
                _exclusiveEntityTransaction.AddComponent(entity, ComponentType.ReadOnly<TComponentData>());
                _exclusiveEntityTransaction.SetComponentData(entity, componentData);
            }
        }

        public void AddComponentObject<T>(Entity e, T componentData) where T : class
        {
            throw new System.NotImplementedException();
        }

        public DynamicBuffer<T> AddBuffer<T>(Entity e) where T : unmanaged, IBufferElementData
        {
            return _exclusiveEntityTransaction.AddBuffer<T>(e);
        }

    }
}
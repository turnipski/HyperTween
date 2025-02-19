using Unity.Collections;
using Unity.Entities;

namespace HyperTween.TweenBuilders
{
    public struct EntityCommandBufferTweenBuilder : ITweenBuilder
    {
        private EntityCommandBuffer _entityCommandBuffer;

        public EntityCommandBufferTweenBuilder(WorldUnmanaged worldUnmanaged, EntityCommandBuffer entityCommandBuffer)
        {
            WorldUnmanaged = worldUnmanaged;
            _entityCommandBuffer = entityCommandBuffer;
        }

        public WorldUnmanaged WorldUnmanaged { get; }

        public EntityCommandBufferParallelWriterTweenBuilder AsParallelWriter()
        {
            return new EntityCommandBufferParallelWriterTweenBuilder(WorldUnmanaged, _entityCommandBuffer.AsParallelWriter());
        }


        public Entity CreateEntity()
        {
            return _entityCommandBuffer.CreateEntity();
        }

        public void DestroyEntity(Entity entity)
        {
            _entityCommandBuffer.DestroyEntity(entity);
        }

        public void Instantiate(Entity prefab, NativeArray<Entity> entities)
        {
            _entityCommandBuffer.Instantiate(prefab, entities);
        }

        public void SetName(Entity entity, in FixedString64Bytes name)
        {
            _entityCommandBuffer.SetName(entity, name);
        }

        public void AddComponent<T>(Entity e) where T : unmanaged, IComponentData
        {
            _entityCommandBuffer.AddComponent<T>(e);
        }

        public void AddComponent<TComponent>(NativeArray<Entity> entities) where TComponent : unmanaged, IComponentData
        {
            _entityCommandBuffer.AddComponent<TComponent>(entities);
        }

        public void AddComponent<T>(Entity e, T componentData) where T : unmanaged, IComponentData
        {
            _entityCommandBuffer.AddComponent(e, componentData);
        }

        public void AddComponent<TComponent>(NativeArray<Entity> entities, TComponent componentData) where TComponent : unmanaged, IComponentData
        {
            _entityCommandBuffer.AddComponent(entities, componentData);
        }

        public void AddComponentObject<T>(Entity e, T componentData) where T : class
        {
            _entityCommandBuffer.AddComponent(e, componentData);
        }

        public DynamicBuffer<T> AddBuffer<T>(Entity e) where T : unmanaged, IBufferElementData
        {
            return _entityCommandBuffer.AddBuffer<T>(e);
        }

    }
}
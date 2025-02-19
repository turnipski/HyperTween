using System;
using Unity.Collections;
using Unity.Entities;

namespace HyperTween.TweenBuilders
{
    public struct EntityCommandBufferParallelWriterTweenBuilder : ITweenBuilder
    {
        private EntityCommandBuffer.ParallelWriter _entityCommandBuffer;

        public EntityCommandBufferParallelWriterTweenBuilder(WorldUnmanaged worldUnmanaged, EntityCommandBuffer.ParallelWriter entityCommandBuffer)
        {
            WorldUnmanaged = worldUnmanaged;
            _entityCommandBuffer = entityCommandBuffer;
        }

        public WorldUnmanaged WorldUnmanaged { get; }

        public Entity CreateEntity()
        {
            return _entityCommandBuffer.CreateEntity(0);
        }

        public void DestroyEntity(Entity entity)
        {
            _entityCommandBuffer.DestroyEntity(0, entity);
        }

        public void Instantiate(Entity prefab, NativeArray<Entity> entities)
        {
            _entityCommandBuffer.Instantiate(0, prefab, entities);
        }

        public void SetName(Entity entity, in FixedString64Bytes name)
        {
            _entityCommandBuffer.SetName(0, entity, name);
        }

        public void AddComponent<T>(Entity e) where T : unmanaged, IComponentData
        {
            _entityCommandBuffer.AddComponent<T>(0, e);
        }

        public void AddComponent<TComponent>(NativeArray<Entity> entities) where TComponent : unmanaged, IComponentData
        {
            _entityCommandBuffer.AddComponent<TComponent>(0, entities);
        }

        public void AddComponent<T>(Entity e, T componentData) where T : unmanaged, IComponentData
        {
            _entityCommandBuffer.AddComponent(0, e, componentData);
        }

        public void AddComponent<TComponent>(NativeArray<Entity> entities, TComponent componentData) where TComponent : unmanaged, IComponentData
        {
            _entityCommandBuffer.AddComponent(0, entities, componentData);
        }

        public void AddComponentObject<T>(Entity e, T componentData) where T : class
        {
            throw new NotImplementedException();
        }

        public DynamicBuffer<T> AddBuffer<T>(Entity e) where T : unmanaged, IBufferElementData
        {
            return _entityCommandBuffer.AddBuffer<T>(0, e);
        }
    }
}
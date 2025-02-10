using Unity.Collections;
using Unity.Entities;

namespace HyperTween.TweenBuilders
{
    public struct EntityManagerTweenBuilder : ITweenBuilder
    {
        private EntityManager _entityManager;

        public EntityManagerTweenBuilder(EntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        public Entity CreateEntity()
        {
            return _entityManager.CreateEntity();
        }

        public void DestroyEntity(Entity entity)
        {
            _entityManager.DestroyEntity(entity);
        }

        public void Instantiate(Entity prefab, NativeArray<Entity> entities)
        {
            _entityManager.Instantiate(prefab, entities);
        }

        public void SetName(Entity entity, in FixedString64Bytes name)
        {
            _entityManager.SetName(entity, name);
        }

        public void AddComponent<T>(Entity e) where T : unmanaged, IComponentData
        {
            _entityManager.AddComponent<T>(e);
        }

        public void AddComponent<TComponent>(NativeArray<Entity> entities) where TComponent : unmanaged, IComponentData
        {
            _entityManager.AddComponent<TComponent>(entities);
        }

        public void AddComponent<T>(Entity e, T componentData) where T : unmanaged, IComponentData
        {
            _entityManager.AddComponentData(e, componentData);
        }

        public void AddComponent<TComponent>(NativeArray<Entity> entities, TComponent componentData) where TComponent : unmanaged, IComponentData
        {
            foreach (var entity in entities)
            {
                _entityManager.AddComponentData(entity, componentData);
            }
        }

        public void AddComponentObject<T>(Entity e, T componentData) where T : class
        {
            _entityManager.AddComponentObject(e, componentData);
        }

        public DynamicBuffer<T> AddBuffer<T>(Entity e) where T : unmanaged, IBufferElementData
        {
            return _entityManager.AddBuffer<T>(e);
        }
    }
}
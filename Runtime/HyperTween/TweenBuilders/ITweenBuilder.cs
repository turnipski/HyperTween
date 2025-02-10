using System;
using Unity.Collections;
using Unity.Entities;

namespace HyperTween.TweenBuilders
{
    public interface ITweenBuilder
    {
        Entity CreateEntity();
        void DestroyEntity(Entity entity);
        void Instantiate(Entity prefab, NativeArray<Entity> entities);
        void SetName(Entity entity, in FixedString64Bytes name);
        void AddComponent<TComponent>(Entity e) where TComponent : unmanaged, IComponentData;
        void AddComponent<TComponent>(NativeArray<Entity> entities) where TComponent : unmanaged, IComponentData;
        void AddComponent<TComponent>(Entity e, TComponent componentData) where TComponent : unmanaged, IComponentData;
        void AddComponent<TComponent>(NativeArray<Entity> entities, TComponent componentData) where TComponent : unmanaged, IComponentData;
        void AddComponentObject<TComponent>(Entity e, TComponent componentData) where TComponent : class;
        DynamicBuffer<TBufferElement> AddBuffer<TBufferElement>(Entity e) where TBufferElement : unmanaged, IBufferElementData;
    }
    
    public interface IBatchTweenBuilder
    {
        public void SetNames(NativeArray<FixedString64Bytes> names);
        public void AddComponent<TComponent>() where TComponent : unmanaged, IComponentData;
        void AddComponent<TComponent>(TComponent componentData) where TComponent : unmanaged, IComponentData;
        void AddComponents<TComponent>(NativeArray<TComponent> componentDatas) where TComponent : unmanaged, IComponentData;
        void AddComponentObjects<TComponent>(TComponent[] componentData) where TComponent : class;
    }
}
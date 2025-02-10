using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace HyperTween.TweenBuilders
{
    public struct BatchTweenBuilder<TTweenBuilder> : IBatchTweenBuilder where TTweenBuilder : ITweenBuilder
    {
        private TTweenBuilder _tweenBuilder;
        
        [ReadOnly]
        private NativeArray<Entity> _entities;

        public BatchTweenBuilder(NativeArray<Entity> entities, TTweenBuilder tweenBuilder)
        {
            _entities = entities;
            _tweenBuilder = tweenBuilder;
        }

        public void SetNames(NativeArray<FixedString64Bytes> names)
        {
            for (var i = 0; i < _entities.Length; i++)
            {
                _tweenBuilder.SetName(_entities[i], names[i]);
            }
        }
        
        public void AddComponent<TComponent>() where TComponent : unmanaged, IComponentData
        {
            _tweenBuilder.AddComponent<TComponent>(_entities);
        }
        
        public void AddComponent<TComponent>(TComponent componentData) where TComponent : unmanaged, IComponentData
        {
            _tweenBuilder.AddComponent(_entities, componentData);
        }

        public void AddComponents<TComponent>(NativeArray<TComponent> componentDatas) where TComponent : unmanaged, IComponentData
        {
            for (var i = 0; i < _entities.Length; i++)
            {
                _tweenBuilder.AddComponent(_entities[i], componentDatas[i]);
            }
        }

        public void AddComponentObjects<TComponent>(TComponent[] componentDatas) where TComponent : class
        {
            for (var i = 0; i < _entities.Length; i++)
            {
                _tweenBuilder.AddComponentObject(_entities[i], componentDatas[i]);
            }          
        }
    }
}
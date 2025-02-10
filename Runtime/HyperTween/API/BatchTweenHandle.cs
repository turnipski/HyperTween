using System;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Update.Components;
using HyperTween.TweenBuilders;
using Unity.Collections;
using Unity.Entities;

namespace HyperTween.API
{
    public struct BatchTweenHandle<T> : ITweenHandle, IDisposable
        where T : ITweenBuilder
    {
        private BatchTweenBuilder<T> _batchTweenBuilder;

        [ReadOnly]
        private NativeArray<Entity> _entities;
        
        private readonly bool _disposeEntities;

        public BatchTweenHandle(Entity prefab, int count, Allocator allocator, T tweenBuilder)
        {
            tweenBuilder.AddComponent<Prefab>(prefab);

            _entities = new NativeArray<Entity>(count, allocator);

            tweenBuilder.Instantiate(prefab, _entities);
            
            // No longer need the prefab
            tweenBuilder.DestroyEntity(prefab);

            _batchTweenBuilder = new BatchTweenBuilder<T>(_entities, tweenBuilder);

            _disposeEntities = true;
        }
        
        public BatchTweenHandle(NativeArray<Entity> entities, T tweenBuilder)
        {
            _entities = entities;
            _batchTweenBuilder = new BatchTweenBuilder<T>(_entities, tweenBuilder);
            _disposeEntities = false;
        }
        
        public BatchTweenBuilder<T> BatchTweenBuilder => _batchTweenBuilder;

        public BatchTweenHandle<T> WithNames(NativeArray<FixedString64Bytes> names)
        {
            _batchTweenBuilder.SetNames(names);
            return this;
        }
        
        public BatchTweenHandle<T> WithTargets(NativeArray<Entity> entities)
        {
            _batchTweenBuilder.AddComponents(entities.Reinterpret<TweenTarget>());

            return this;
        }

        public BatchTweenHandle<T> WithDuration(float duration)
        {
            _batchTweenBuilder.AddComponent(TweenDuration.Create(duration));
            return this;
        }
        
        public BatchTweenHandle<T> WithDurations(NativeArray<float> durations, int startIndex = 0)
        {
            var tweenDurations = new NativeArray<TweenDuration>(durations.Length, Allocator.Temp);
            for (var i = 0; i < durations.Length; i++)
            {
                tweenDurations[i] = TweenDuration.Create(durations[i]);
            }

            _batchTweenBuilder.AddComponents(tweenDurations);
            tweenDurations.Dispose();

            return this;
        }
        
        public BatchTweenHandle<T> Play()
        {
            _batchTweenBuilder.AddComponent<TweenRequestPlaying>();
            return this;
        }

        public void Dispose()
        {
            if (_disposeEntities)
            {
                _entities.Dispose();
            }
        }
    }
}
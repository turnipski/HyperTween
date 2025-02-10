using HyperTween.ECS.Update.Components;
using HyperTween.SequenceBuilders;
using HyperTween.TweenBuilders;
using Unity.Collections;
using Unity.Entities;

namespace HyperTween.API
{
    public struct TweenFactory<TTweenBuilder> where TTweenBuilder : unmanaged, ITweenBuilder
    {
        private TTweenBuilder _tweenBuilder;
        private readonly bool _withJournal;
        
        internal TTweenBuilder TweenBuilder => _tweenBuilder;

        public TweenFactory(TTweenBuilder tweenBuilder, bool withJournal = false)
        {
            _tweenBuilder = tweenBuilder;
            _withJournal = withJournal;
        }

        public TweenHandle<TTweenBuilder> CreateTween()
        {
            var entity = _tweenBuilder.CreateEntity();
            ConfigureTween(entity);
            
            var tweenHandle = new TweenHandle<TTweenBuilder>(entity, _tweenBuilder);
            
            if (_withJournal)
            {
                tweenHandle = tweenHandle.WithJournaling();
            }

            return tweenHandle;
        }
        
        public BatchTweenHandle<TTweenBuilder> CreateBatch(NativeArray<Entity> entities)
        {
            return new BatchTweenHandle<TTweenBuilder>(entities, _tweenBuilder);
        }

        public TweenHandle<TTweenBuilder> AddTween(Entity entity)
        {
            ConfigureTween(entity);
            
            var tweenHandle = new TweenHandle<TTweenBuilder>(entity, _tweenBuilder);

            return tweenHandle.AllowReuse();
        }

        private void ConfigureTween(Entity entity)
        {
            _tweenBuilder.AddComponent<TweenTimer>(entity);
            _tweenBuilder.AddComponent<TweenParameter>(entity);
            _tweenBuilder.AddComponent<TweenDurationOverflow>(entity);
            
#if HYPERTWEEN_JOURNAL_ALL && UNITY_EDITOR
            _tweenBuilder.AddComponent<TweenJournal>(entity);
#endif
            
        }
        
        public NativeArray<TweenHandle<TTweenBuilder>> AllocateNativeArray(int capacity, Allocator allocator)
        {
            return new NativeArray<TweenHandle<TTweenBuilder>>(capacity, allocator);
        }
        
        public TweenHandle<TTweenBuilder> Serial(NativeArray<TweenHandle<TTweenBuilder>> subTweens, Allocator allocator = Allocator.Temp)
        {
            return Serial(allocator)
                .Append(subTweens)
                .Build();
        }
        
        public SequenceFactory<TTweenBuilder, SerialSequenceBuilder<TTweenBuilder>> Serial(Allocator allocator = Allocator.Temp)
        {
            return new SequenceFactory<TTweenBuilder, SerialSequenceBuilder<TTweenBuilder>>(
                _tweenBuilder,
                new SerialSequenceBuilder<TTweenBuilder>(),
                allocator);
        }
        
        public TweenHandle<TTweenBuilder> Parallel(NativeArray<TweenHandle<TTweenBuilder>> subTweens, Allocator allocator = Allocator.Temp)
        {
            return Parallel(allocator)
                .Append(subTweens)
                .Build();
        }

        public SequenceFactory<TTweenBuilder, ParallelSequenceBuilder<TTweenBuilder>> Parallel(Allocator allocator = Allocator.Temp)
        {
            return new SequenceFactory<TTweenBuilder, ParallelSequenceBuilder<TTweenBuilder>>(
                _tweenBuilder, 
                new ParallelSequenceBuilder<TTweenBuilder>(),
                allocator);
        }
        
        public TweenHandle<TTweenBuilder> Stagger(NativeArray<TweenHandle<TTweenBuilder>> subTweens, float delayPerTween, Allocator allocator = Allocator.Temp)
        {
            return Stagger(delayPerTween, allocator)
                .Append(subTweens)
                .Build();
        }
        
        public SequenceFactory<TTweenBuilder, StaggerSequenceBuilder<TTweenBuilder>> Stagger(float delayPerTween, Allocator allocator = Allocator.Temp)
        {
            return new SequenceFactory<TTweenBuilder, StaggerSequenceBuilder<TTweenBuilder>>(
                _tweenBuilder,
                new StaggerSequenceBuilder<TTweenBuilder>(delayPerTween),
                allocator);
        }
    }
}
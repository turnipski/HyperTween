using HyperTween.ECS.Sequencing.Components;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Update.Components;
using HyperTween.TweenBuilders;
using HyperTween.TweenDebug.Journal;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace HyperTween.API
{
    public struct TweenHandle<TBuilder> : ITweenHandle 
        where TBuilder : unmanaged, ITweenBuilder
    {
        private readonly Entity _entity;
        private TBuilder _tweenBuilder;

        public Entity Entity => _entity;
        public TBuilder TweenBuilder => _tweenBuilder;

        
        public TweenHandle(Entity entity, TBuilder tweenBuilder)
        {
            _entity = entity;
            _tweenBuilder = tweenBuilder;
        }
        
        public TweenHandle<TBuilder> AllowReuse()
        {
            _tweenBuilder.AddComponent<TweenAllowReuse>(_entity);
            
            return this;
        }
                
        public TweenHandle<TBuilder> WithName(in FixedString64Bytes name)
        {
            _tweenBuilder.SetName(_entity, name);
            return this;
        }
        
        public TweenHandle<TBuilder> WithTarget(Entity entity)
        {
            _tweenBuilder.AddComponent(_entity, new TweenTarget()
            {
                Target = entity
            });

            return this;
        }

        public TweenHandle<TBuilder> WithDuration(float duration)
        {
            if (duration <= 0f)
            {
                return this;
            }
            
            _tweenBuilder.AddComponent(_entity, new TweenDuration()
            {
                Value = duration,
                InverseValue = 1f / duration
            });

            return this;
        }
        
        public TweenHandle<TBuilder> WithLocalTransform()
        {
            _tweenBuilder.AddComponent<LocalTransform>(_entity);
            return this;
        }

        public TweenHandle<TBuilder> WithEaseIn(float strength = 1f)
        {
            return WithHermiteEasing(0, strength);
        }
        
        public TweenHandle<TBuilder> WithEaseOut(float strength = 1f)
        {
            return WithHermiteEasing(strength, 0);
        }
        
        public TweenHandle<TBuilder> WithEaseInOut()
        {
            return WithHermiteEasing(0, 0);
        }
        
        public TweenHandle<TBuilder> WithHermiteEasing(float m0, float m1)
        {
            _tweenBuilder.AddComponent(_entity, new TweenHermiteEasing(m0, m1));

            return this;
        }

        public TweenHandle<TBuilder> Play(float skipDuration)
        {
            _tweenBuilder.AddComponent(_entity, new TweenRequestPlaying()
            {
                DurationOverflow = skipDuration
            });

            return this;
        }
        
        public TweenHandle<TBuilder> Play()
        {
            _tweenBuilder.AddComponent<TweenRequestPlaying>(_entity);

            return this;
        }
        
        public TweenHandle<TBuilder> PlayOnPlay(TweenHandle<TBuilder> tweenHandle)
        {
            _tweenBuilder.AddComponent(_entity, new TweenPlayOnPlay()
            {
                Target = tweenHandle._entity
            });

            return this;
        }

        public TweenHandle<TBuilder> PlayOnStop(TweenHandle<TBuilder> tweenHandle)
        {
            _tweenBuilder.AddComponent(_entity, new TweenPlayOnStop()
            {
                Target = tweenHandle._entity
            });

            return this;
        }
        
        internal TweenHandle<TBuilder> ForkOnPlay(NativeArray<TweenHandle<TBuilder>> tweenHandles) 
        {
            var buffer =  _tweenBuilder.AddBuffer<TweenForkOnPlay>(_entity);
            foreach (var tweenHandle in tweenHandles)
            {
                buffer.Add(new TweenForkOnPlay()
                {
                    Target = tweenHandle._entity
                });
            }

            return this;
        }
        
        internal TweenHandle<TBuilder> StopOnJoin(TweenHandle<TBuilder> tweenHandle)
        {
            _tweenBuilder.AddComponent(tweenHandle._entity, new TweenSignalJoinOnStop()
            {
                Target = _entity
            });
            
            _tweenBuilder.AddComponent(_entity, new TweenStopOnJoin()
            {
                RequiredSignals = 1
            });

            return this;
        }
        
        internal TweenHandle<TBuilder> StopOnJoin(NativeArray<TweenHandle<TBuilder>> tweenHandles)
        {
            var count = 0;
            
            foreach (var tweenHandle in tweenHandles)
            {
                _tweenBuilder.AddComponent(tweenHandle._entity, new TweenSignalJoinOnStop()
                {
                    Target = _entity
                });

                count++;
            }
            
            _tweenBuilder.AddComponent(_entity, new TweenStopOnJoin()
            {
                RequiredSignals = count
            });

            return this;
        }

        public TweenHandle<TBuilder> DestroyTargetOnStop()
        {
            _tweenBuilder.AddComponent<TweenDestroyTargetOnStop>(_entity);
            
            return this;
        }
        
        public TweenHandle<TBuilder> WithJournaling()
        {
#if UNITY_EDITOR
            _tweenBuilder.AddComponent<TweenJournal>(_entity);
#else
            UnityEngine.Debug.LogWarning("TweenJournal is disabled outside of the editor.");
#endif
            
            return this;
        }

        public BatchTweenHandle<TBuilder> CreateBatch(int numTweens, Allocator allocator)
        {
            return new BatchTweenHandle<TBuilder>(_entity, numTweens, allocator, _tweenBuilder);
        }
    }
}
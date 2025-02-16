using System.Collections.Generic;
using HyperTween.TweenBuilders;
using Unity.Collections;

namespace HyperTween.API
{
    public struct SequenceFactory<TTweenBuilder, TSequenceBuilder>
        where TSequenceBuilder : ISequenceBuilder<TTweenBuilder> 
        where TTweenBuilder : unmanaged, ITweenBuilder
    {
        private TTweenBuilder _tweenBuilder;
        private TSequenceBuilder _sequenceBuilder;
        private NativeList<TweenHandle> _subTweens;
        private Allocator _allocator;
        
        public SequenceFactory(TTweenBuilder tweenBuilder, TSequenceBuilder sequenceBuilder, Allocator allocator)
        {
            _tweenBuilder = tweenBuilder;
            _sequenceBuilder = sequenceBuilder;
            _allocator = allocator;
            
            _subTweens = new NativeList<TweenHandle>(2, allocator);
        }
        
        public SequenceFactory<TTweenBuilder, TSequenceBuilder> Append(IEnumerable<TweenHandle> subTweens)
        {
            foreach (var subTween in subTweens)
            {
                Append(subTween);
            }

            return this;
        }
        
        public SequenceFactory<TTweenBuilder, TSequenceBuilder> Append(NativeArray<TweenHandle> subTweens)
        {
            foreach (var subTween in subTweens)
            {
                Append(subTween);
            }

            return this;
        }

        public SequenceFactory<TTweenBuilder, TSequenceBuilder> Append(TweenHandle subTween)
        {
            _subTweens.Add(subTween);

            return this;
        }

        public TweenHandle<TTweenBuilder> Build()
        {
            try
            {
                if (_subTweens.Length == 0)
                {
                    return new TweenHandle<TTweenBuilder>(_tweenBuilder.CreateEntity(), _tweenBuilder);
                }
                
                if (_subTweens.Length == 1)
                {
                    return new TweenHandle<TTweenBuilder>(_subTweens[0].Entity, _tweenBuilder);
                }
                
                return _sequenceBuilder.Build(_tweenBuilder, _subTweens, _allocator);
            }
            finally
            {
                _subTweens.Dispose();
            }
        }
    }
}
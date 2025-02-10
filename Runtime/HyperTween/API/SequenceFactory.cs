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
        private NativeList<TweenHandle<TTweenBuilder>> _subTweens;
        private Allocator _allocator;
        
        public SequenceFactory(TTweenBuilder tweenBuilder, TSequenceBuilder sequenceBuilder, Allocator allocator)
        {
            _tweenBuilder = tweenBuilder;
            _sequenceBuilder = sequenceBuilder;
            _allocator = allocator;
            
            _subTweens = new NativeList<TweenHandle<TTweenBuilder>>(2, allocator);
        }
        
        public SequenceFactory<TTweenBuilder, TSequenceBuilder> Append(IEnumerable<TweenHandle<TTweenBuilder>> subTweens)
        {
            foreach (var subTween in subTweens)
            {
                Append(subTween);
            }

            return this;
        }
        
        public SequenceFactory<TTweenBuilder, TSequenceBuilder> Append(NativeArray<TweenHandle<TTweenBuilder>> subTweens)
        {
            foreach (var subTween in subTweens)
            {
                Append(subTween);
            }

            return this;
        }

        public SequenceFactory<TTweenBuilder, TSequenceBuilder> Append(TweenHandle<TTweenBuilder> subTween)
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
                    return _subTweens[0];
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
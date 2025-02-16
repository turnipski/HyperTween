using HyperTween.API;
using HyperTween.TweenBuilders;
using Unity.Collections;

namespace HyperTween.SequenceBuilders
{
    public struct StaggerSequenceBuilder<TTweenBuilder> : ISequenceBuilder<TTweenBuilder> where TTweenBuilder : unmanaged, ITweenBuilder
    {
        private float _delayPerTween;

        public StaggerSequenceBuilder(float delayPerTween)
        {
            _delayPerTween = delayPerTween;
        }

        public TweenHandle<TTweenBuilder> Build(TTweenBuilder tweenBuilder, NativeList<TweenHandle> subTweens, Allocator allocator)
        {
            var tweenFactory = new TweenFactory<TTweenBuilder>(tweenBuilder);
            var parallelBuilder = tweenFactory.Parallel(allocator);

            var currentDelay = 0f;
            
            foreach (var subTween in subTweens)
            {
                parallelBuilder
                    .Append(tweenFactory.Serial(allocator)
                        .Append(tweenFactory
                            .CreateTween()
                            .WithDuration(currentDelay)
                            .WithName("Delay"))
                        .Append(subTween)
                        .Build()
                        .WithName("StaggerSerial"));

                currentDelay += _delayPerTween;
            }

            return parallelBuilder.Build().WithName("StaggerParallel");
        }
    }
}
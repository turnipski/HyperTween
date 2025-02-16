using HyperTween.API;
using HyperTween.ECS.Update.Components;
using HyperTween.TweenBuilders;
using HyperTween.TweenDebug.Journal;
using Unity.Collections;
using Unity.Transforms;

namespace HyperTween.SequenceBuilders
{
    public struct SerialSequenceBuilder<TTweenBuilder> : ISequenceBuilder<TTweenBuilder> where TTweenBuilder : unmanaged, ITweenBuilder
    {
        public TweenHandle<TTweenBuilder> Build(TTweenBuilder tweenBuilder, NativeList<TweenHandle> subTweens, Allocator allocator)
        {
            var entity = tweenBuilder.CreateEntity();
            tweenBuilder.AddComponent<TweenDurationOverflow>(entity);
            
#if HYPERTWEEN_JOURNAL_ALL
            tweenBuilder.AddComponent<TweenJournal>(entity);
#endif
            
            var serialTweenHandle = new TweenHandle<TTweenBuilder>(entity, tweenBuilder);

            var first = true;
            var previousTweenHandle = serialTweenHandle;
            foreach (var subTween in subTweens)
            {
                if (first)
                {
                    previousTweenHandle.PlayOnPlay(subTween);
                    first = false;
                }
                else
                {
                    previousTweenHandle.PlayOnStop(subTween);
                }

                previousTweenHandle = new TweenHandle<TTweenBuilder>(subTween.Entity, tweenBuilder);
            }

            return serialTweenHandle.StopOnJoin(previousTweenHandle);
        }
    }
}
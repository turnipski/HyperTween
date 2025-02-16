using HyperTween.API;
using HyperTween.ECS.Update.Components;
using HyperTween.TweenBuilders;
using HyperTween.TweenDebug.Journal;
using Unity.Collections;
using Unity.Transforms;

namespace HyperTween.SequenceBuilders
{
    public struct ParallelSequenceBuilder<TTweenBuilder> : ISequenceBuilder<TTweenBuilder> where TTweenBuilder : unmanaged, ITweenBuilder
    {
        public TweenHandle<TTweenBuilder> Build(TTweenBuilder tweenBuilder, NativeList<TweenHandle> subTweens, Allocator allocator)
        {
            var entity = tweenBuilder.CreateEntity();
            tweenBuilder.AddComponent<TweenDurationOverflow>(entity);

#if HYPERTWEEN_JOURNAL_ALL
            tweenBuilder.AddComponent<TweenJournal>(entity);
#endif
            
            var parallelTweenHandle = new TweenHandle<TTweenBuilder>(entity, tweenBuilder);

            return parallelTweenHandle
                .ForkOnPlay(subTweens.AsArray())
                .StopOnJoin(subTweens.AsArray());
        }
    }
}
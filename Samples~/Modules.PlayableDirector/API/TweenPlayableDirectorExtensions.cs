using HyperTween.API;
using HyperTween.Auto.Components;
using HyperTween.Modules.PlayableDirector.Components;
using HyperTween.TweenBuilders;
using UnityEngine.Playables;

namespace HyperTween.Modules.PlayableDirector.API
{
    public static class TweenPlayableDirectorExtensions
    {
        public static TweenHandle<T> WithPlayableDirector<T>(this TweenHandle<T> tweenHandle, UnityEngine.Playables.PlayableDirector playableDirector)
            where T : unmanaged, ITweenBuilder
        {
            return tweenHandle.WithPlayableDirector(playableDirector, playableDirector.playableAsset);
        }
        
        public static TweenHandle<T> WithPlayableDirector<T>(this TweenHandle<T> tweenHandle, UnityEngine.Playables.PlayableDirector playableDirector, float duration)
            where T : unmanaged, ITweenBuilder
        {
            return tweenHandle.WithPlayableDirector(playableDirector, playableDirector.playableAsset, duration);
        }
        
        public static TweenHandle<T> WithPlayableDirector<T>(this TweenHandle<T> tweenHandle, UnityEngine.Playables.PlayableDirector playableDirector, PlayableAsset playableAsset)
            where T : unmanaged, ITweenBuilder
        {
            return tweenHandle.WithPlayableDirector(playableDirector, playableAsset, (float)playableAsset.duration);
        }
        
        public static TweenHandle<T> WithPlayableDirector<T>(this TweenHandle<T> tweenHandle, UnityEngine.Playables.PlayableDirector playableDirector, PlayableAsset playableAsset, float duration)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.WithDuration(duration);

            if (!playableDirector.playableGraph.IsValid())
            {
                playableDirector.RebuildGraph();
            }
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, new TweenPlayableDirectorOnPlay()
            {
                PlayableDirector = playableDirector,
                PlayableAsset = playableAsset,
            });

            return tweenHandle;
        }
    }
}
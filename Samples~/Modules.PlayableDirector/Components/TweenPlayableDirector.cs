using HyperTween.ECS.Invoke.Components;
using HyperTween.ECS.Update.Components;
using UnityEngine;
using UnityEngine.Playables;

namespace HyperTween.Modules.PlayableDirector.Components
{
    public class TweenPlayableDirector : ITweenInvokeOnPlay
    {
        public UnityEngine.Playables.PlayableDirector PlayableDirector;
        public PlayableAsset PlayableAsset;
        
        public void Invoke(in TweenDuration tweenDuration)
        {
            var speed = PlayableAsset.duration / tweenDuration.Value;

            PlayableDirector.Play(PlayableAsset);
            PlayableDirector.playableGraph.GetRootPlayable(0).SetSpeed(speed);
        }
    }
}
using HyperTween.API;
using HyperTween.Modules.InvokeAction.API;
using HyperTween.Modules.PlayableDirector.API;
using HyperTween.TweenBuilders;
using UnityEngine;

namespace HyperTween.Examples.PlayableDirector
{
    public class PlayableDirectorTweenExample : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Playables.PlayableDirector _playableDirector;
        
        private void Start()
        {
            HyperTweenFactory.CreateTween()
                .WithPlayableDirector(_playableDirector, 10f)
                .Play();
        }
    }
}
using HyperTween.API;
using HyperTween.Modules.Transform;
using UnityEngine;
using UnityEngine.UI;

namespace HyperTween.Examples
{
    public class ImageColorTweenExample : MonoBehaviour
    {
        [SerializeField] private Image _target;
        
        void Start()
        {
            HyperTweenFactory.CreateTween()
                .WithDuration(5f)
                .WithColor(_target, Color.green)
                .Play();
        }
    }
}
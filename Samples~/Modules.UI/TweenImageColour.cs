using HyperTween.ECS.ConflictDetection.Attributes;
using HyperTween.ECS.Update.Components;
using UnityEngine;
using UnityEngine.UI;

namespace HyperTween.Modules.UI
{
    [DetectConflicts(typeof(ImageInstanceId))]
    public struct TweenImageColour : ITweenTo<Image, Color>
    {
        public Color Value;

        public Color GetValue()
        {
            return Value;
        }

        public Color Lerp(Color from, Color to, float parameter)
        {
            return Color.Lerp(from, to, parameter);
        }

        public readonly Color Read(in Image component)
        {
            return component.color;
        }

        public readonly void Write(ref Image component, Color value)
        {
            component.color = value;
        }
    }
}
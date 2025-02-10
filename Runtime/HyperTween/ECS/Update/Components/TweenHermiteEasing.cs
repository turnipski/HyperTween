using System;
using Unity.Entities;

namespace HyperTween.ECS.Update.Components
{
    [Serializable]
    public struct TweenHermiteEasingArgs
    {
        public float m0, m1;
    }
    
    [WriteGroup(typeof(TweenParameter))]
    public struct TweenHermiteEasing : IComponentData
    {
        public float a, b, c;
        
        public TweenHermiteEasing(float m0, float m1)
        {
            a = m0 + m1 - 2;
            b = 3 - 2 * m0 - m1;
            c = m0;
        }

        public readonly float Interpolate(float x)
        {
            float x2 = x * x;
            float x3 = x2 * x;

            return a * x3 + b * x2 + c * x;
        }
    }
}
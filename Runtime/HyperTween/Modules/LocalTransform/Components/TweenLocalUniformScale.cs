using HyperTween.ECS.ConflictDetection.Attributes;
using HyperTween.ECS.Update.Components;
using HyperTween.Modules.Transform;
using Unity.Mathematics;
using Unity.Transforms;

namespace HyperTween.Modules.LocalTransform.Components
{
    [DetectConflicts(typeof(TransformInstanceId))]
    public struct TweenLocalUniformScale : ITweenTo<Unity.Transforms.LocalTransform, float>
    {
        public float Value;

        public float GetValue()
        {
            return Value;
        }

        public float Lerp(float from, float to, float parameter)
        {
            return math.lerp(from, to, parameter);
        }

        public readonly float Read(in Unity.Transforms.LocalTransform component)
        {
            return component.Scale;
        }

        public readonly void Write(ref Unity.Transforms.LocalTransform component, float value)
        {
            component.Scale = value;
        }
    }
}
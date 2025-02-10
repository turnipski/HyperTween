using HyperTween.ECS.ConflictDetection.Attributes;
using HyperTween.ECS.Update.Components;
using HyperTween.Modules.Transform;
using Unity.Mathematics;
using Unity.Transforms;

namespace HyperTween.Modules.LocalTransform.Components
{
    [DetectConflicts(typeof(TransformInstanceId))]
    public struct TweenLocalPosition : ITweenTo<Unity.Transforms.LocalTransform, float3>
    {
        public float3 Value;

        public float3 GetValue()
        {
            return Value;
        }

        public float3 Lerp(float3 from, float3 to, float parameter)
        {
            return math.lerp(from, to, parameter);
        }

        public readonly float3 Read(in Unity.Transforms.LocalTransform component)
        {
            return component.Position;
        }

        public readonly void Write(ref Unity.Transforms.LocalTransform component, float3 value)
        {
            component.Position = value;
        }
    }
}
using HyperTween.ECS.ConflictDetection.Attributes;
using HyperTween.ECS.Update.Components;
using HyperTween.Modules.Transform;
using Unity.Mathematics;
using Unity.Transforms;

namespace HyperTween.Modules.LocalTransform.Components
{
    [DetectConflicts(typeof(TransformInstanceId))]
    public struct TweenLocalRotation : ITweenTo<Unity.Transforms.LocalTransform, quaternion>
    {
        public quaternion Value;

        public quaternion GetValue()
        {
            return Value;
        }

        public quaternion Lerp(quaternion from, quaternion to, float parameter)
        {
            return math.slerp(from, to, parameter);
        }

        public readonly quaternion Read(in Unity.Transforms.LocalTransform component)
        {
            return component.Rotation;
        }

        public readonly void Write(ref Unity.Transforms.LocalTransform component, quaternion value)
        {
            component.Rotation = value;
        }
    }
}
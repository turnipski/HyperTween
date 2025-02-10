using System;

namespace HyperTween.ECS.ConflictDetection.Attributes
{
    public class DetectConflictsAttribute : Attribute
    {
        public Type TargetInstanceIdComponentType;

        public DetectConflictsAttribute(Type targetInstanceIdComponentType)
        {
            TargetInstanceIdComponentType = targetInstanceIdComponentType;
        }
        
        public DetectConflictsAttribute()
        {
            TargetInstanceIdComponentType = null;
        }
    }
}
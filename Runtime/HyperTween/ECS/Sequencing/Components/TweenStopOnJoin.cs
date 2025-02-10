using Unity.Entities;

namespace HyperTween.ECS.Sequencing.Components
{
    public struct TweenStopOnJoin : IComponentData
    {
        public int CurrentSignals, RequiredSignals;
    }
}
using Unity.Entities;

namespace HyperTween.ECS.Invoke.Systems
{
    public interface ITweenInvokeJob<TJobData> : IJobChunk
        where TJobData : unmanaged
    {
        public TJobData JobData { get; set; }
    }
}
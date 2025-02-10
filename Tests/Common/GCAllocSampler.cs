using System;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Tests.Common
{
    public class GCAllocSampler : IDisposable
    {
        private readonly long _initialMemory = GC.GetTotalMemory(false);
        private readonly GarbageCollector.Mode _initialMode;
        
        public GCAllocSampler()
        {
#if !UNITY_EDITOR
            _initialMode = GarbageCollector.GCMode;
            GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
#endif
        }
        
        public void Dispose()
        {
            var allocated = GC.GetTotalMemory(false) - _initialMemory;
            Measure.Custom(new SampleGroup("AllocatedMemory", SampleUnit.Byte), allocated);
            
#if !UNITY_EDITOR
            GarbageCollector.GCMode = _initialMode;
#endif
        }
    }
}
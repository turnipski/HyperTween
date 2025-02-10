
using Unity.Entities;

namespace HyperTween.TweenDebug.Journal
{
#if UNITY_EDITOR
    public struct TweenJournal : IComponentData
    {
        public enum Event : byte
        {
            Stop,
            Play,
            Conflict,
            ECBPlayback,
            CleanECBPlayback
        }
        
        public struct LiteEntry
        {
            public Event Event;
            public Entity Entity;
        }
        
        public struct Entry
        {
            public LiteEntry LiteEntry;
            public int Index;
            public int Frame;
            public double Time;
            public int Iteration;

            public Entry(LiteEntry liteEntry, int frame, double time, int index, int iteration)
            {
                LiteEntry = liteEntry;
                Frame = frame;
                Time = time;
                Index = index;
                Iteration = iteration;
            }
        }

    }
#endif
}

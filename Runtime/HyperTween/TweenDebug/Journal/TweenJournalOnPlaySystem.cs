using System;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Structural.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace HyperTween.TweenDebug.Journal
{
#if UNITY_EDITOR
    [BurstCompile]
    public struct JournalCopyJob : IJob
    {
        public TweenJournalSingleton Singleton;
        public double Time;
            
        [BurstCompile]
        public void Execute()
        {
            var bufferLength = Singleton.Buffer.Length;

            var index = Singleton.Index.Value;
            var length = Singleton.Length.Value;
            var count = Singleton.Count.Value;
            var frame = Singleton.CurrentFrame.Value;
            var iteration = Singleton.CurrentStructuralChangeIteration.Value;
            
            foreach (var entry in Singleton.LastFrame)
            {

                Singleton.Buffer[index] = new TweenJournal.Entry(entry, frame, Time, count, iteration);

                count++;
                index = (index + 1) % bufferLength;
                length = math.min(bufferLength, length + 1);
            }
                
            Singleton.LastFrame.Clear();

            Singleton.Index.Value = index;
            Singleton.Length.Value = length;
            Singleton.Count.Value = count;
        }
    }
    
    public struct TweenJournalSingleton : IComponentData, IDisposable
    {
        public NativeList<TweenJournal.LiteEntry> LastFrame;
        public NativeArray<TweenJournal.Entry> Buffer;
        public NativeReference<int> Index;
        public NativeReference<int> Length;
        public NativeReference<int> Count;
        public NativeReference<int> CurrentFrame;
        public NativeReference<int> CurrentStructuralChangeIteration;
        
        public void Dispose()
        {
            LastFrame.Dispose();
            Buffer.Dispose();
            Index.Dispose();
            Length.Dispose();
            Count.Dispose();
            CurrentFrame.Dispose();
            CurrentStructuralChangeIteration.Dispose();
        }
    }
    
    [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
    public partial struct TweenJournalOnPlaySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new TweenJournalSingleton()
            {
                LastFrame = new NativeList<TweenJournal.LiteEntry>(512 , Allocator.Persistent),
                Buffer = new NativeArray<TweenJournal.Entry>(512 , Allocator.Persistent),
                Index = new NativeReference<int>(Allocator.Persistent),
                Length = new NativeReference<int>(Allocator.Persistent),
                Count = new NativeReference<int>(Allocator.Persistent),
                CurrentFrame = new NativeReference<int>(Allocator.Persistent),
                CurrentStructuralChangeIteration = new NativeReference<int>(Allocator.Persistent),
            });
            
            state.RequireForUpdate<TweenJournalSingleton>();
            state.RequireForUpdate<TweenJournal>();
        }

        public void OnDestroy(ref SystemState state)
        {
            var singleton = SystemAPI.GetSingleton<TweenJournalSingleton>();
            singleton.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.CompleteDependencyBeforeRW<TweenJournalSingleton>();
            var singleton = SystemAPI.GetSingletonRW<TweenJournalSingleton>();

            var parallelWriter = singleton.ValueRW.LastFrame.AsParallelWriter();
            
            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<TweenRequestPlaying>>()
                         .WithAll<TweenJournal>()
                         .WithNone<TweenPlaying>()
                         .WithEntityAccess())
            {
                parallelWriter.AddNoResize(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.Play
                });
            }
            
            var copyJob = new JournalCopyJob()
            {
                Singleton = singleton.ValueRW,
                Time = SystemAPI.Time.ElapsedTime
            };
            
            state.Dependency = copyJob.ScheduleByRef(state.Dependency);

            
            SystemAPI.SetSingleton(copyJob.Singleton);
        }
    }
    
     [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
    public partial struct TweenJournalOnStopSystem : ISystem
    {
        private int _frame;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TweenJournalSingleton>();
            state.RequireForUpdate<TweenJournal>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.CompleteDependencyBeforeRW<TweenJournalSingleton>();
            var singleton = SystemAPI.GetSingletonRW<TweenJournalSingleton>();

            var parallelWriter = singleton.ValueRW.LastFrame.AsParallelWriter();

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<TweenPlaying>>()
                         .WithAll<TweenJournal>()
                         .WithNone<TweenRequestPlaying>()
                         .WithEntityAccess())
            {
                parallelWriter.AddNoResize(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.Stop
                });
            }
            
            var copyJob = new JournalCopyJob()
            {
                Singleton = singleton.ValueRW,
                Time = SystemAPI.Time.ElapsedTime
            };
            
            state.Dependency = copyJob.ScheduleByRef(state.Dependency);
        }
    }
    
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct TweenJournalIncrementFrameSystem : ISystem
    {
        private int _frame;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TweenJournalSingleton>();
            state.RequireForUpdate<TweenJournal>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.CompleteDependencyBeforeRW<TweenJournalSingleton>();
            
            var singleton = SystemAPI.GetSingletonRW<TweenJournalSingleton>();
            singleton.ValueRW.CurrentFrame.Value++;
            singleton.ValueRW.CurrentStructuralChangeIteration.Value = 0;
        }
    }
    
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup), OrderLast = true)]
    public partial struct TweenJournalResetIterationSystem : ISystem
    {
        private int _frame;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TweenJournalSingleton>();
            state.RequireForUpdate<TweenJournal>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.CompleteDependencyBeforeRW<TweenJournalSingleton>();
            var singleton = SystemAPI.GetSingletonRW<TweenJournalSingleton>();
            singleton.ValueRW.CurrentStructuralChangeIteration.Value++;
        }
    }
#endif
}

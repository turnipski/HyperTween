// ReSharper disable AccessToDisposedClosure

using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests.Common
{
    [TestFixture]
    public abstract class BasePerformanceTests
    {
        private const float TweenDuration = 10f;
        
        [Test, Performance]
        public void ManagedTransform_Create([Values(1, 10, 100, 1000, 10000, 20000)]int numTweens)
        {
            World world = null;
            var transforms = new Transform[numTweens];

            using var positions = new NativeArray<float3>(numTweens, Allocator.TempJob);
            CreatePositions(positions);
            
            Measure.Method(() =>
                {
                    using var profilerMarker = new ProfilerMarker("Create").Auto();
                    using var gcAllocSampler = new GCAllocSampler();
                    
                    CreateTransformTweens(world, transforms, positions, TweenDuration);
                })
                .SetUp(() =>
                {
                    world = CreateWorld();
                    CreateTransforms(transforms);
                })
                .CleanUp(() =>
                {
                    world?.Dispose();
                    DestroyTransforms(transforms);
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(1)
                .MeasurementCount(30)
                .Run();
        }
        
        [UnityTest, Performance]
        public IEnumerator ManagedTransform_Update([Values(1, 10, 100, 1000, 10000, 20000)]int numTweens)
        {
            using var world = CreateWorld();

            {
                using var positions = new NativeArray<float3>(numTweens, Allocator.TempJob);
                CreatePositions(positions);

                var transforms = new Transform[numTweens];
                CreateTransforms(transforms);
                
                CreateTransformTweens(world, transforms, positions, TweenDuration);
            }

            var sampleGroups = GetUpdateProfileMarkers()
                .Select(s => new SampleGroup(s, SampleUnit.Nanosecond))
                .ToArray();

            using var profileMarkers = Measure.ProfilerMarkers(sampleGroups);

            var timer = new Timer(5f);
            while(timer.IsRunning)
            {
                yield return null;
            }
        }

        [Test, Performance]
        public void DirectUnmanagedTransform_Create([Values(1, 10, 100, 1000, 10000, 20000)]int numTweens)
        {
            World world = null;

            using var entities = new NativeArray<Entity>(numTweens, Allocator.TempJob);
            
            using var positions = new NativeArray<float3>(numTweens, Allocator.TempJob);
            CreatePositions(positions);
            
            Measure
                .Method(() =>
                {
                    using var profilerMarker = new ProfilerMarker("Create").Auto();
                    using var gcAllocSampler = new GCAllocSampler();
                    
                    CreateDirectLocalTransformTweens(world, entities, positions, TweenDuration);
                })
                .SetUp(() =>
                {
                    world = CreateWorld();
                    CreateEntities(world, entities);
                })
                .CleanUp(() =>
                {
                    world.Dispose();
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(1)
                .MeasurementCount(30)
                .Run();
        }

        [UnityTest, Performance]
        public IEnumerator DirectUnmanagedTransform_Update([Values(1, 10, 100, 1000, 10000, 20000)]int numTweens)
        {
            using var world = CreateWorld();

            {
                using var entities = new NativeArray<Entity>(numTweens, Allocator.TempJob);
                CreateEntities(world, entities);

                using var positions = new NativeArray<float3>(numTweens, Allocator.TempJob);
                CreatePositions(positions);

                CreateDirectLocalTransformTweens(world, entities, positions, TweenDuration);
            }

            var sampleGroups = GetUpdateProfileMarkers()
                .Select(s => new SampleGroup(s, SampleUnit.Nanosecond))
                .ToArray();

            using var profileMarkers = Measure.ProfilerMarkers(sampleGroups);

            var timer = new Timer(5f);
            while(timer.IsRunning)
            {
                yield return null;
            }
        }
        
        [Test, Performance]
        public void IndirectUnmanagedTransform_Create([Values(1, 10, 100, 1000, 10000, 20000)]int numTweens)
        {
            World world = null;
            
            using var entities = new NativeArray<Entity>(numTweens, Allocator.TempJob);
            
            using var positions = new NativeArray<float3>(numTweens, Allocator.TempJob);
            CreatePositions(positions);
            
            // ReSharper disable once AccessToDisposedClosure
            Measure.Method(() =>
                {
                    using var profilerMarker = new ProfilerMarker("Create").Auto();
                    using var gcAllocSampler = new GCAllocSampler();
                    
                    CreateIndirectLocalTransformTweens(world, entities, positions, TweenDuration);
                })
                .SetUp(() =>
                {
                    world = CreateWorld();
                    CreateEntities(world, entities);
                })
                .CleanUp(() =>
                {
                    world.Dispose();
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(1)
                .MeasurementCount(30)
                .Run();
        }

        [UnityTest, Performance]
        public IEnumerator IndirectUnmanagedTransform_Update([Values(1, 10, 100, 1000, 10000, 20000)]int numTweens)
        {
            using var world = CreateWorld();

            {
                using var entities = new NativeArray<Entity>(numTweens, Allocator.TempJob);
                CreateEntities(world, entities);

                using var positions = new NativeArray<float3>(numTweens, Allocator.TempJob);
                CreatePositions(positions);

                CreateIndirectLocalTransformTweens(world, entities, positions, TweenDuration);
            }
            
            var sampleGroups = GetUpdateProfileMarkers()
                .Select(s => new SampleGroup(s, SampleUnit.Nanosecond))
                .ToArray();

            using var profileMarkers = Measure.ProfilerMarkers(sampleGroups);

            var timer = new Timer(5f);
            while(timer.IsRunning)
            {
                yield return null;
            }
        }
        
        private static void CreateTransforms(Transform[] outTransforms)
        {
            for (var i = 0; i < outTransforms.Length; i++)
            {
                outTransforms[i] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            }
        }
        
        private static void CreatePositions(NativeArray<float3> outPositions)
        {
            var random = new Unity.Mathematics.Random(123);
            for (var i = 0; i < outPositions.Length; i++)
            {
                outPositions[i] = random.NextFloat3();
            }
        }
        
        private static void DestroyTransforms(Transform[] transforms)
        {
            for (var i = 0; i < transforms.Length; i++)
            {
                Object.Destroy(transforms[i].gameObject);
            }
        }
        
        private static void CreateEntities(World world, NativeArray<Entity> outEntities)
        {
            if (world == null)
            {
                throw new NotImplementedException();
            }
            
            for (var i = 0; i < outEntities.Length; i++)
            {
                var entity = world.EntityManager.CreateEntity();
                world.EntityManager.AddComponent<LocalTransform>(entity);

                outEntities[i] = entity;
            }
        }

        protected abstract World CreateWorld();
        protected abstract void CreateTransformTweens(World world, Transform[] transforms, NativeArray<float3> positions, float duration);
        protected abstract void CreateDirectLocalTransformTweens(World world, NativeArray<Entity> entities, NativeArray<float3> positions, float duration);
        protected abstract void CreateIndirectLocalTransformTweens(World world, NativeArray<Entity> entities, NativeArray<float3> positions, float duration);
        protected abstract string[] GetUpdateProfileMarkers();
    }
}

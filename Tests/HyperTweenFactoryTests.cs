using System.Collections;
using System.Linq;
using HyperTween.API;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Update.Components;
using HyperTween.Modules.InvokeAction.API;
using HyperTween.Modules.LocalTransform.API;
using HyperTween.Modules.Transform.API;
using HyperTween.TweenBuilders;
using HyperTween.Util;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.TestTools;

namespace HyperTween.Tests
{
    public class HyperTweenFactoryTests
    {
        private World _world;
        private TweenFactory<EntityManagerTweenBuilder> _tweenFactory;
    
        [OneTimeSetUp]
        public void SetUp()
        {
            _world = World.DefaultGameObjectInjectionWorld;
            _tweenFactory = _world.EntityManager.CreateTweenFactory(true);
        }

        [Test]
        public void Tween_WithDuration_PlaysAfterUpdate()
        {
            const float duration = 1f;
        
            var tweenHandle = _tweenFactory.CreateTween()
                .WithDuration(duration);
            
            Assert.IsFalse(_world.EntityManager.HasComponent<TweenRequestPlaying>(tweenHandle.Entity));

            tweenHandle
                .Play();

            Assert.IsTrue(_world.EntityManager.HasComponent<TweenRequestPlaying>(tweenHandle.Entity));
            Assert.IsFalse(_world.EntityManager.HasComponent<TweenPlaying>(tweenHandle.Entity));

            _world.Update();
        
            Assert.IsTrue(_world.EntityManager.HasComponent<TweenRequestPlaying>(tweenHandle.Entity));
            Assert.IsTrue(_world.EntityManager.HasComponent<TweenPlaying>(tweenHandle.Entity));
        }
    
        [UnityTest]
        public IEnumerator Tween_WithDuration_CorrectDuration([Values(0f, 0.1f, 1f, 5f, 30f)] float duration)
        {
            var tweenHandle = _tweenFactory.CreateTween()
                .WithDuration(duration);

            foreach (var yieldInstruction in AssertTweenDuration(tweenHandle, duration))
            {
                yield return yieldInstruction;
            }
        }

        private IEnumerable AssertTweenDuration<T>(TweenHandle<T> tweenHandle, float duration) 
            where T : unmanaged, ITweenBuilder
        { 
            // Reasoning about timing accuracy is relatively unintuitive, especially due to execution order relative
            // to UpdateWorldTimeSystem. For the purposes of these tests, it's defined that timing is measured from the
            // perspective of the OnTween[Stop/Play]Systems

            // Due to limitations of coroutines, the only good context we can get into to measure the start time is at
            // the end of the frame after all systems have run
            yield return new WaitForEndOfFrame();

            // Record the start time as a reference to measure other times against
            var worldStartTime = _world.Time.ElapsedTime;

            double? startTime = null, endTime = null;

            tweenHandle.InvokeActionOnPlay(_ =>
                {
                    // We actually have a frame of latency between calling Play() and the tween starting here, due to
                    // execution order so we need to subtract deltaTime. See the giant comment below...
                    startTime = _world.Time.ElapsedTime - _world.Time.DeltaTime - worldStartTime;
                    Debug.Log($"startTime: {startTime}");
                })
                .InvokeActionOnStop(context =>
                {
                    // Because tweens with duration update their timers after the OnTween[Stop/Play]Systems, we actually
                    // end up with a frame of latency between when the tween is requested to stop and when it actually
                    // stops. This isn't a problem though, because any new tweens that are started in this frame are
                    // going to immediately simulate and incorporate the DeltaTime which comes from the previous frame,
                    // negating the latency and resulting in overall accurate timing. This does however mean that
                    // DeltaTime must be subtracted from ElapsedTime in order to produce an accurate endTime for the test.
                    endTime = _world.Time.ElapsedTime - _world.Time.DeltaTime - worldStartTime - context.TweenDurationOverflow.Value;
                    Debug.Log($"endTime: {endTime}");
                })
                .AllowReuse()
                .Play();
            
            do
            {
                Debug.Log("---- tick");
                yield return new WaitForEndOfFrame();
                Debug.Log($"Elapsed: {_world.Time.ElapsedTime - worldStartTime}, dt: {_world.Time.DeltaTime}");
                //Debug.Log($"Timer: {_world.EntityManager.GetComponentData<TweenTimer>(tweenHandle.Entity).Value}");
            } while (_world.Time.ElapsedTime - worldStartTime < duration + 1f);
            
            // Because of the extra frame of latency in processing tweens with duration, we wait an extra frame here
            yield return new WaitForEndOfFrame();

            Assert.IsTrue(startTime.HasValue);
            Assert.IsTrue(endTime.HasValue);
            Assert.AreEqual(0, startTime);
            
            // Although ECS supports double time, we require less memory bandwidth with floats, so we need some approximation
            Assert.AreEqual(duration, endTime - startTime, 1d/1000);
        }
    
        [UnityTest]
        public IEnumerator Tween_SerialSequence_CorrectDuration()
        {
            var numTweens = 20;
            var duration = 0.2f;
            var totalDuration = numTweens * duration;
        
            using var tweenHandles = Enumerable.Range(0, numTweens)
                .Select(i => (TweenHandle)_tweenFactory.CreateTween().WithDuration(duration))
                .ToNativeArray(Allocator.Temp);

            var serialTween = _tweenFactory
                .Serial()
                .Append(tweenHandles)
                .Build()
                .AllowReuse();

            foreach (var yieldInstruction in AssertTweenDuration(serialTween, totalDuration))
            {
                yield return yieldInstruction;
            }
        }

        [UnityTest]
        public IEnumerator Tween_ParallelSequence_CorrectDuration()
        {
            var numTweens = 20;
            var duration = 0.2f;
        
            using var tweenHandles = Enumerable.Range(0, numTweens)
                .Select(i => (TweenHandle)_tweenFactory.CreateTween().WithDuration(0.2f))
                .ToNativeArray(Allocator.Temp);

            var parallelTween = _tweenFactory
                .Parallel()
                .Append(tweenHandles)
                .Build()
                .AllowReuse();

            foreach (var yieldInstruction in AssertTweenDuration(parallelTween, duration))
            {
                yield return yieldInstruction;
            }
        }
        
        [UnityTest]
        public IEnumerator Tween_CompositeSequence_CorrectDuration()
        {
            var numSerials = 2;
            var numTweens = 2;
            var duration = 0.1f;
            var totalDuration = numSerials * duration;
            
            using var sequenceHandles = Enumerable
                .Range(0, numSerials)
                .Select(i =>
                {
                    using var tweenHandles = Enumerable
                        .Range(0, numTweens)
                        .Select(j => (TweenHandle)_tweenFactory.CreateTween().WithDuration(duration))
                        .ToNativeArray(Allocator.Temp);

                    return (TweenHandle)_tweenFactory.Parallel(tweenHandles);
                })
                .ToNativeArray(Allocator.Temp);

            var serialTween = _tweenFactory.Serial(sequenceHandles);

            foreach (var yieldInstruction in AssertTweenDuration(serialTween, totalDuration))
            {
                yield return yieldInstruction;
            }
        }
    
        [UnityTest]
        public IEnumerator Tween_WithTransform_CorrectPosition()
        {
            const float duration = 1f;
        
            var gameObject = new GameObject();

            var expectedPosition = new Vector3(1f, 2f, 3f);
            
            var tweenHandle = _tweenFactory.CreateTween()
                .WithDuration(duration)
                .WithTransform(gameObject.transform)
                .WithLocalPositionOutput(expectedPosition)
                .Play();
            
            do
            {
                yield return new WaitForEndOfFrame();
            } while (_world.EntityManager.HasComponent<TweenPlaying>(tweenHandle.Entity));
        
            Assert.AreEqual(expectedPosition, gameObject.transform.position);
        }
        
        [UnityTest]
        public IEnumerator IndirectTween_WithTargetLocalTransform_CorrectPosition()
        {
            var expectedPosition = new float3(1f, 2f, 3f);

            var otherEntity = _world.EntityManager.CreateEntity();
            _world.EntityManager.AddComponent<LocalTransform>(otherEntity);
            
            _tweenFactory.CreateTween()
                .WithTarget(otherEntity)
                .WithLocalPositionOutput(expectedPosition)
                .Play();
            
            yield return new WaitForEndOfFrame();

            Assert.AreEqual(expectedPosition, _world.EntityManager.GetComponentData<LocalTransform>(otherEntity).Position);
        }
        
        [UnityTest]
        public IEnumerator Tween_WithZeroDurationTransform_CorrectPosition()
        {
            var gameObject = new GameObject();

            var expectedPosition = new Vector3(1f, 2f, 3f);
            
            var tweenHandle = _tweenFactory.CreateTween()
                .WithTransform(gameObject.transform)
                .WithLocalPositionOutput(expectedPosition)
                .Play();

            _world.Update();
        
            Assert.AreEqual(expectedPosition, gameObject.transform.position);

            yield break;
        }
        
        [OneTimeTearDown]
        public void TearDown()
        {
            _world.Dispose();
        }
    }
}

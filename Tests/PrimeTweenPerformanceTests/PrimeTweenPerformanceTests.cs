using System;
using System.Collections;
using PrimeTween;
using Tests.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Tests.PrimeTween
{
    public class PrimeTweenPerformanceTests : BasePerformanceTests
    {
        protected override void CreateTransformTweens(World world, Transform[] transforms, NativeArray<float3> positions, float duration)
        {
            Tween.StopAll();
            PrimeTweenConfig.SetTweensCapacity(transforms.Length + 1);

            for (var i = 0; i < transforms.Length; i++)
            {
                var transform = transforms[i];
                Tween.LocalPosition(transform, positions[i], duration);
            }
        }

        protected override void CreateDirectLocalTransformTweens(World world, NativeArray<Entity> entities, NativeArray<float3> positions, float duration)
        {
            throw new NotImplementedException();
        }

        protected override void CreateIndirectLocalTransformTweens(World world, NativeArray<Entity> entities, NativeArray<float3> positions, float duration)
        {
            throw new NotImplementedException();
        }

         protected override string[] GetUpdateProfileMarkers()
        {
            return new string[] 
            { 
                "PrimeTween.Runtime.dll!PrimeTween::PrimeTweenManager.Update() [Invoke]"
            };
        }

        protected override World CreateWorld()
        {
            return default;
        }
    }
}
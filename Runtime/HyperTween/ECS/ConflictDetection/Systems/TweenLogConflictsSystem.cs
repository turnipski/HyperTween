using HyperTween.ECS.ConflictDetection.Components;
using HyperTween.ECS.Structural.Components;
using HyperTween.ECS.Structural.Systems;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace HyperTween.ECS.ConflictDetection.Systems
{
    [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
    [BurstCompile]
    partial struct TweenLogConflictsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = false;
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (_, entity) in SystemAPI
                         // ReSharper disable once Unity.Entities.MustBeSurroundedWithRefRwRo
                         .Query<TweenConflicted>()
                         .WithAll<TweenPlaying>()
                         .WithEntityAccess())
            {
                state.EntityManager.GetName(entity, out var name);
                Debug.LogWarning($"TweenConflicted: {name} {entity.ToFixedString()}");
            }
        }
    }
}
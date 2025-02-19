using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace HyperTween.TweenBuilders
{
    [BurstCompile]
    public struct ExclusiveEntityTransactionScope
    {
        private readonly WorldUnmanaged _stagingWorld;
        private readonly WorldUnmanaged _targetWorld;
        
        private ExclusiveEntityTransaction _exclusiveEntityTransaction;
        private NativeArray<Entity> _targetEntities;

        public ExclusiveEntityTransactionScope(World targetWorld)
        {
            _targetEntities = default;
            _stagingWorld = TweenStagingWorld.Instance.Unmanaged;
            _targetWorld = targetWorld.Unmanaged;
            _exclusiveEntityTransaction = _stagingWorld.EntityManager.BeginExclusiveEntityTransaction();
        }
        
        public ExclusiveEntityTransactionScope(World targetWorld, NativeArray<Entity> targetEntities)
        {
            _targetEntities = targetEntities;
            _stagingWorld = TweenStagingWorld.Instance.Unmanaged;
            _targetWorld = targetWorld.Unmanaged;
            _exclusiveEntityTransaction = _stagingWorld.EntityManager.BeginExclusiveEntityTransaction();
        }
        
        public ExclusiveEntityTransactionScope(WorldUnmanaged targetWorld, WorldUnmanaged stagingWorld, NativeArray<Entity> targetEntities)
        {
            _targetEntities = targetEntities;
            _stagingWorld = stagingWorld;
            _targetWorld = targetWorld;
            _exclusiveEntityTransaction = _stagingWorld.EntityManager.BeginExclusiveEntityTransaction();
        }

        public ExclusiveEntityTransactionTweenBuilder GetTweenBuilder()
        {
            return new ExclusiveEntityTransactionTweenBuilder(_stagingWorld, _exclusiveEntityTransaction);
        }

        [BurstCompile]
        public void Playback()
        {
            _stagingWorld.EntityManager.EndExclusiveEntityTransaction();

            if (_targetEntities.IsCreated)
            {
                var remaps = _stagingWorld.EntityManager.CreateEntityRemapArray(Allocator.TempJob);
                foreach (var entity in _targetEntities)
                {
                    EntityRemapUtility.AddEntityRemapping(ref remaps, entity, entity);
                }
                _targetWorld.EntityManager.MoveEntitiesFrom(_stagingWorld.EntityManager, remaps);
                remaps.Dispose();
            }
            else
            {
                _targetWorld.EntityManager.MoveEntitiesFrom(_stagingWorld.EntityManager);
            }
        }
    }
}
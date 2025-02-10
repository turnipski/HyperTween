using HyperTween.API;
using HyperTween.Modules.LocalTransform.API;
using HyperTween.Modules.Transform;
using HyperTween.TweenBuilders;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public partial struct ExclusiveEntityTransationTweenExampleSystem : ISystem
{
    [BurstCompile]
    private struct CreateIndirectTweensJob : IJob
    {
        public TweenFactory<ExclusiveEntityTransactionTweenBuilder> TweenFactory;
            
        public float Duration;
            
        [ReadOnly]
        public NativeArray<Entity> Targets;

        [BurstCompile]
        public void Execute()
        {
            var positions = new NativeArray<float3>(Targets.Length, Allocator.Temp);

            var random = new Random(1);
            for (var i = 0; i < positions.Length; i++)
            {
                positions[i] = random.NextFloat3();
            }

            using var batchTweenHandle = TweenFactory.CreateTween()
                .WithDuration(Duration)
                .WithLocalTransform()
                .Play()
                .CreateBatch(Targets.Length, Allocator.Temp)
                .WithTargets(Targets)
                .WithLocalPositionOutputs(positions);
            
            positions.Dispose();
        }
    }
    
    private EntityQuery _entityQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();

        _entityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<TweenTest>()
            .Build(ref state);
        
        state.RequireForUpdate<TweenTest>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
    
        using var entities = _entityQuery.ToEntityArray(Allocator.TempJob);
        
        var exclusiveEntityTransactionScope = new ExclusiveEntityTransactionScope(state.WorldUnmanaged, TweenStagingWorld.Instance.Unmanaged, entities);

        new CreateIndirectTweensJob()
        { 
            TweenFactory = exclusiveEntityTransactionScope.CreateTweenFactory(),
            Duration = 1f,
            Targets = entities
        }.Run();
        
        exclusiveEntityTransactionScope.Playback();

        ecb.RemoveComponent<TweenTest>(_entityQuery, EntityQueryCaptureMode.AtPlayback);
    }
}
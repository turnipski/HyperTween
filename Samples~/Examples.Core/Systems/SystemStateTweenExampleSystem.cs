using HyperTween.API;
using HyperTween.Modules.LocalTransform.API;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public partial struct SystemStateTweenExampleSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
    
        var tweenFactory = state.CreateTweenFactory();
        var random = new Random(1);
        
        foreach (var (_, entity) in SystemAPI.Query<RefRO<TestComponentB>>().WithEntityAccess())
        {
            tweenFactory.CreateTween()
                .WithTarget(entity)
                .WithDuration(1f)
                .WithLocalTransform()
                .WithLocalPositionOutput(to: -5f + (10f * random.NextFloat3()))
                .Play();

            ecb.RemoveComponent<TestComponentB>(entity);
        }
    }
}
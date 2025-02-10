using HyperTween.API;
using HyperTween.Modules.LocalTransform.API;
using HyperTween.Modules.Transform;
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
    
        foreach (var (_, entity) in SystemAPI.Query<RefRO<TweenTest>>().WithEntityAccess())
        {
            tweenFactory.CreateTween()
                .WithTarget(entity)
                .WithDuration(1f)
                .WithLocalTransform()
                .WithLocalPositionOutput(to: new float3(1, 2, 3))
                .Play();

            ecb.RemoveComponent<TweenTest>(entity);
        }
    }
}
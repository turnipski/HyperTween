using HyperTween.ECS.Structural.Systems;
using HyperTween.TweenBuilders;
using Unity.Collections;
using Unity.Entities;

namespace HyperTween.API
{
    public static class TweenFactoryExtensions
    {
        public static TweenFactory<EntityCommandBufferTweenBuilder> CreateTweenFactory(this ref SystemState systemState, bool withJournaling = false)
        {
            return systemState.WorldUnmanaged.CreateTweenFactory(withJournaling);
        }
        
        public static TweenFactory<EntityCommandBufferTweenBuilder> CreateTweenFactory(this World world, bool withJournaling = false)
        {
            return world.Unmanaged.CreateTweenFactory(withJournaling);
        }
        
        public static TweenFactory<EntityCommandBufferTweenBuilder> CreateTweenFactory(this WorldUnmanaged world, bool withJournaling = false)
        {
            using var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp);
                
            var query = entityQueryBuilder
                .WithAll<PreTweenStructuralChangeECBSystem.Singleton>()
                .WithOptions(EntityQueryOptions.IncludeSystems)
                .Build(world.EntityManager);

            var singleton = query.GetSingleton<PreTweenStructuralChangeECBSystem.Singleton>();
            
            return singleton.CreateCommandBuffer(world).CreateTweenFactory(withJournaling);
        }

        public static TweenFactory<EntityManagerTweenBuilder> CreateTweenFactory(this EntityManager entityManager, bool withJournaling = false)
        {
            return new EntityManagerTweenBuilder(entityManager).CreateTweenFactory(withJournaling);
        }

        public static TweenFactory<EntityCommandBufferTweenBuilder> CreateTweenFactory(this EntityCommandBuffer entityCommandBuffer, bool withJournaling = false)
        {
            return new EntityCommandBufferTweenBuilder(entityCommandBuffer).CreateTweenFactory(withJournaling);
        }

        public static TweenFactory<EntityCommandBufferParallelWriterTweenBuilder> AsParallelWriter(this TweenFactory<EntityCommandBufferTweenBuilder> tweenFactory)
        {
            return new TweenFactory<EntityCommandBufferParallelWriterTweenBuilder>(tweenFactory.TweenBuilder.AsParallelWriter());
        }
        
        public static TweenFactory<ExclusiveEntityTransactionTweenBuilder> CreateTweenFactory(this ExclusiveEntityTransactionScope exclusiveEntityTransactionScope, bool withJournaling = false)
        {
            return exclusiveEntityTransactionScope.GetTweenBuilder().CreateTweenFactory(withJournaling);
        }
                
        public static TweenFactory<T> CreateTweenFactory<T>(this T tweenBuilder, bool withJournaling = false) where T : unmanaged, ITweenBuilder
        {
            return new TweenFactory<T>(tweenBuilder, withJournaling);
        }
    }
}
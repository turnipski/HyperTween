using HyperTween.SequenceBuilders;
using HyperTween.TweenBuilders;
using Unity.Entities;

namespace HyperTween.API
{
    public static class HyperTweenFactory
    {
        private static TweenFactory<EntityManagerTweenBuilder> _tweenFactory;
        
        public static TweenFactory<EntityManagerTweenBuilder> Get(bool withJournaling = false)
        {
            return World.DefaultGameObjectInjectionWorld.EntityManager.CreateTweenFactory(withJournaling);
        }
        
        public static TweenHandle<EntityManagerTweenBuilder> CreateTween()
        {
            return Get().CreateTween();
        }
        
        public static TweenHandle<EntityManagerTweenBuilder> AddTween(Entity entity)
        {
            return Get().AddTween(entity);
        }

        public static SequenceFactory<EntityManagerTweenBuilder, ParallelSequenceBuilder<EntityManagerTweenBuilder>> Parallel()
        {
            return Get().Parallel();
        }
        
        public static SequenceFactory<EntityManagerTweenBuilder, SerialSequenceBuilder<EntityManagerTweenBuilder>> Serial()
        {
            return Get().Serial();
        }
    }
}
using System;
using HyperTween.API;
using HyperTween.ECS.Invoke.Components;
using HyperTween.ECS.Update.Components;
using HyperTween.TweenBuilders;
using Unity.Entities;

namespace HyperTween.Modules.InvokeAction.Components
{
    public class TweenInvokeAction : ITweenInvokeOnPlay, ITweenInvokeOnStop
    {
        public readonly struct Context
        {
            public readonly Entity Entity;
            public readonly Entity TargetEntity;
            public readonly EntityCommandBuffer EntityCommandBuffer;
            public readonly TweenDurationOverflow TweenDurationOverflow;
            public readonly TweenFactory<EntityCommandBufferTweenBuilder> TweenFactory;
            
            public Context(Entity entity, Entity targetEntity, EntityCommandBuffer entityCommandBuffer, TweenDurationOverflow tweenDurationOverflow, TweenFactory<EntityCommandBufferTweenBuilder> tweenFactory)
            {
                Entity = entity;
                EntityCommandBuffer = entityCommandBuffer;
                TweenDurationOverflow = tweenDurationOverflow;
                TweenFactory = tweenFactory;
                TargetEntity = targetEntity;
            }
        }
        
        public Action<Context> Action;

        public void Invoke(Entity tweenEntity, Entity targetEntity, EntityCommandBuffer entityCommandBuffer, TweenFactory<EntityCommandBufferTweenBuilder> tweenFactory, in TweenDurationOverflow tweenDurationOverflow)
        {
            Action.Invoke(new Context(tweenEntity, targetEntity, entityCommandBuffer, tweenDurationOverflow, tweenFactory));
        }
        
    }
}
# Advanced Tween Creation

For more control and efficiency, especially when working with ECS, you can use `TweenFactoryExtensions` to create a `TweenFactory` directly. This allows you to create tweens using different underlying `TweenBuilders` which are useful in different situations:

[!code-csharp[CS](../../Runtime/HyperTween/API/TweenFactoryExtensions.cs#L10)]

This method allows you to create a `TweenFactory` from a `SystemState` component that uses an `EntityCommandBuffer` under the hood. This method allows you to create tweens from a Burst compiled `ISystem` and is the most efficient simple option. The `EntityCommandBuffer` plays back in `PreTweenStructuralChangeECBSystem` which runs before all tween systems.

[!code-csharp[CS](../../Runtime/HyperTween/API/TweenFactoryExtensions.cs#L20)]

Exactly the same as the `SystemState` overload, which is just a shortcut to this.

[!code-csharp[CS](../../Runtime/HyperTween/API/TweenFactoryExtensions.cs#L15)]

The same as the `SystemState` overload, but non-Burst compilable as `World` is a managed type.

[!code-csharp[CS](../../Runtime/HyperTween/API/TweenFactoryExtensions.cs#L34)]

Allows you to use an `EntityManager` to create tweens, which is not as efficient but means that the tweens are created immediately.

[!code-csharp[CS](../../Runtime/HyperTween/API/TweenFactoryExtensions.cs#L34)]

Allows you to use an arbitrary `EntityCommandBuffer` to create tweens.

[!code-csharp[CS](../../Runtime/HyperTween/API/TweenFactoryExtensions.cs#L44)]

Allows you to convert a `TweenFactory<EntityCommandBufferTweenBuilder>` to a `TweenFactory<EntityCommandBufferParallelWriterTweenBuilder>` which you can use to create tweens from parallel jobs.

[!code-csharp[CS](../../Runtime/HyperTween/API/TweenFactoryExtensions.cs#L49)]

Allows you to create tweens using a `ExclusiveEntityTransaction`. This is the most efficient way to create a significant number of tweens at the same time. See [Batch Tween Creation](advanced-batch-tween-creation.md) for more information.

Here are some examples:

### 2.1 Creating tweens from an ISystem

[!code-csharp[CS](../../Samples~/Examples.Core/Systems/SystemStateTweenExampleSystem.cs)]
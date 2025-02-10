# Tween Creation

The simplest way to create a tween is by using the `HyperTweenFactory.Create()` method:

[!code-csharp[CS](../../Samples~/Examples.Core/MonoBehaviours/CreateTweenExample.cs)]

This tween will move the object's transform to the position (1, 2, 3) over the course of 1 second.

## Fluent API

All tween creation methods return a `TweenHandle`, which provides access to configuration methods for the tween. These methods can be chained for fluent configuration:

[!code-csharp[CS](../../Samples~/Examples.Core/MonoBehaviours/FluentTweenExample.cs#L11-L17)]

## Tween From

By default, tweens being from the existing value when the tween is played, however you can also specify the starting value yourself:

[!code-csharp[CS](../../Samples~/Examples.Core/MonoBehaviours/FromTweenExample.cs#L11-L15)]

This method is actually slightly more efficient because the starting value does not need to be initialized automatically.

## Tween Factories

`HyperTweenFactory.CreateTween()` uses `HyperTweenFactory.Get()` under the hood. Using this method is slightly more efficient if you are creating multiple tweens, but it also allows you to create methods for creating tweens that can be reused with different types of factories:

[!code-csharp[CS](../../Samples~/Examples.Core/MonoBehaviours/FactoryTweenExample.cs)]

## Tween Destruction

By default, using the `CreateTween()` method will cause the created `Entity` to be destroyed when the tween stops. See [Tween Reuse](advanced-misc.md) for more info.

## Entity Targets

In order to use HyperTween to manipulate the position of regular ECS `Entities`, you can use `WithTarget(entity)` to cause the tween to output to an arbitrary entity.
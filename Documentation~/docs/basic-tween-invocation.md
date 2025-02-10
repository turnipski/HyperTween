# Invocation

Invocation in HyperTween allows you to execute specific code at key points during a tween's lifecycle, such as when it starts or stops. This feature is crucial for creating complex, reactive animations and game logic.

## Action Invocation

The simplest method to run code when a tween plays or stops is to use `InvokeActionOnPlay` or `InvokeActionOnStop`. Here's an example:

[!code-csharp[CS](../../Samples~/Examples.Core/MonoBehaviours/InvocationTweenExample.cs)]

Note that to create new tweens from the invoked action, you need to use a `TweenFactory` created from the `EntityCommandBuffer` passed to the action. This requires use of a different type of `TweenFactory`. This example shows how it's possible to write a single method that can work with any type of `TweenFactory`.

> [!CAUTION]
> Because `InvokeActionOnPlay` and `InvokeActionOnStop` use managed delegates, code using it cannot be [Burst compiled](https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/index.html), limiting efficiency. See [Extending Invocation](extending-invocation.md) for ways to get around this limitation.
# Extending Invocation

In order to extend HyperTween to add new output types, you just need to add a component that extends `ITweenInvokeOnPlay` and/or `ITweenInvokeOnStop`

Note that these interfaces define no methods because the code generation actually supports an arbitrary method definition for `Invoke()`. The supported parameters are:

* `Entity tweenEntity`
* `Entity targetEntity`
* `EntityCommandBuffer entityCommandBuffer`
* Any type that inherits from `IComponentData`

[!code-csharp[CS](../../Runtime/HyperTween/Modules/InvokeAction/Components/TweenInvokeAction.cs)]

TODO: Source Generation for the API methods
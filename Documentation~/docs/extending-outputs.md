# Extending Outputs

In order to extend HyperTween to add new output types, you just need to add a component that extends `ITweenTo<TComponent, TValue>`. HyperTween will automatically generate a `From` version of the component and the systems required.

[!code-csharp[CS](../../Runtime/HyperTween/Modules/LocalTransform/Components/TweenLocalPosition.cs)]

## Conflict Detection

Adding the `[DetectConflicts]` attribute to the output component will also generate the system required to detect conflicts. See [Miscellaneous Topics](advanced-misc.md) for more information regarding conflicts. By default, this system will detect conflicts by checking if there is an existing tween playing with the same target entity and output component. Alternatively, you can use `[DetectConflicts(typeof(InstanceIdComponent))]` to indicate an `IComponentType` type with a single `int` that is used as a conflict detection id in place of the target entity. The use case for this is to use `UnityEngine.Object.GetInstanceID()` such that conflict detection can function when using managed component outputs such as `Transform`. See below for an example of this.

## API Extensions Methods

You could add this component to tweens using the regular ECS methods, but it is recommended to create extension methods for `TweenHandle` with overloads with parameters with/without a from component.

[!code-csharp[CS](../../Runtime/HyperTween/Modules/Transform/API/TweenTransformExtensions.cs)]

Optionally, you can also add extension methods for `BatchTweenHandle`.

[!code-csharp[CS](../../Runtime/HyperTween/Modules/Transform/API/TweenTransformBatchExtensions.cs)]

TODO: Source Generation for the API methods


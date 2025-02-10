# Easing

Easing functions specify the rate of change of the tween parameter over time. See [here](https://easings.net/) for examples of different easing functions.

Currently, in HyperTween there is just a single easing method called Hermite easing, but it is configurable enough to emulate the most common easing methods, meaning that a single component and system can handle most cases. The following TweenHandle methods exist:

[!code-csharp[CS](../../Samples~/Examples.Core/MonoBehaviours/EaseTweenExample.cs#L14-L17)]

> Note that WithEaseInOut() has no `strength` parameter because the polynomial used does not allow for modifying the gradient in this case

See [this interactive graph](https://www.desmos.com/calculator/1mp77ldefr) for a way to experiment with the interpolation function.
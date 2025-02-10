# Sequencing

HyperTween allows combining tweens into sequences with arbitrarily complex structures. They are primarily composed of Serial and Parallel tweens which are created by appending tweens to `SequenceFactories` and finally calling `Build()` to create a tween which is interchangeable with any other tween.  

[!code-csharp[CS](../../Samples~/Examples.Core/MonoBehaviours/SequenceTweenExample.cs)]

### Functional Tween Composition

Because the tweens created by `SequenceFactories` are just regular tweens, with different components, they can be added to other sequences. Like this it is possible to build up composite tweens with arbitrarily complex behaviour.

A very useful pattern to employ when working with such structures is to create methods that return `TweenHandle<T>`, then combine them into sequences. This can help to simplify your code and allow you to reuse methods for the creation of tweens:

[!code-csharp[CS](../../Samples~/Examples.Core/MonoBehaviours/CompositionTweenExample.cs)]
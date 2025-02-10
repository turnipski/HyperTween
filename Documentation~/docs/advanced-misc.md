# Miscellaneous Topics

## Reusing Tweens

By default, tweens are destroyed when they stop. However, by using `AllowReuse()` tweens will remain in existence after they stop, and you can either call `Play()` again on the original `TweenHandle` or add the `RequestTweenPlaying` component to the `Entity`.

## Conflict Detection

HyperTween employs tween conflict detection meaning that if a tween starts that would write to the same output and target as another tween, the existing tween will be stopped.

## Sequence Timing Accuracy

As tweens are only processed once per frame, in most cases tweens will actually overflow their duration slightly. When tweens are added to sequences, HyperTween actually causes subsequent tweens to skip this portion of their duration such that the duration of the total sequence is more accurate.

In the case of playing tweens manually, for example using `InvokeActionOnStop()`, the invocation context contains the `TweenDurationOverflow` component which indicates how much excess time the tween was active for. You can pass this value to `Play()` in order to skip that amount of time on the next tween. This ensures that you get accurate timing overall. See the [Polyrhythm Sample](https://github.com/david-rzepa/HyperTween/tree/main/Samples~/Polyrhythm) for an example of this in action. 
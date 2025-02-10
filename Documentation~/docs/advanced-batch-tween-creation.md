# Batch Tween Creation

In order to efficiently create significant numbers of tweens, you can use `BatchTweenBuilder<ExclusiveEntityTransactionTweenBuilder>` in a Burst compiled job for maximum possible efficiency.

Note that it is possible to use an `EntityCommandBuffer` to write to the `ExclusiveEntityTransaction` in parallel, but in practise it seems to produce significant contention and actually harm performance. So unless your job actually has significant processing outside the creation of tweens, it's probably not worth it.

[!code-csharp[CS](../../Samples~/Examples.Core/Systems/ExclusiveEntityTransactionTweenExampleSystem.cs)]


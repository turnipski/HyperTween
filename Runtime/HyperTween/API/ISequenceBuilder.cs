using HyperTween.TweenBuilders;
using Unity.Collections;

namespace HyperTween.API
{
    public interface ISequenceBuilder<TTweenBuilder> where TTweenBuilder : unmanaged, ITweenBuilder
    {
        TweenHandle<TTweenBuilder> Build(TTweenBuilder tweenBuilder, NativeList<TweenHandle> subTweens, Allocator allocator);
    }
}
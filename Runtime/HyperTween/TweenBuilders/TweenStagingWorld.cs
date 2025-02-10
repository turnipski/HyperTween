using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace HyperTween.TweenBuilders
{
    public static class TweenStagingWorld
    {
        public static World Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Instance = new World("TweenStagingWorld", WorldFlags.Staging, Allocator.Persistent);
            Application.quitting += () => Instance.Dispose();
        }
    }
}
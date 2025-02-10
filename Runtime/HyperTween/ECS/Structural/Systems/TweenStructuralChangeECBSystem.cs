using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace HyperTween.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup), OrderLast = true)]
    public partial class TweenStructuralChangeECBSystem : EntityCommandBufferSystem
    {
        public unsafe struct Singleton : IComponentData, IECBSingleton
        {
            private UnsafeList<EntityCommandBuffer>* _pendingBuffers;
            private AllocatorManager.AllocatorHandle _allocator;
            
            public EntityCommandBuffer CreateCommandBuffer(WorldUnmanaged world)
            {
                return EntityCommandBufferSystem.CreateCommandBuffer(ref *_pendingBuffers, _allocator, world);
            }

            public void SetPendingBufferList(ref UnsafeList<EntityCommandBuffer> buffers)
            {
                _pendingBuffers = (UnsafeList<EntityCommandBuffer>*)UnsafeUtility.AddressOf(ref buffers);
            }

            public void SetAllocator(Allocator allocatorIn)
            {
                _allocator = allocatorIn;
            }

            public void SetAllocator(AllocatorManager.AllocatorHandle allocatorIn)
            {
                _allocator = allocatorIn;
            }
        }
         
        protected override void OnCreate()
        {
            base.OnCreate();
            this.RegisterSingleton<Singleton>(ref PendingBuffers, World.Unmanaged);

        }
        
        protected override void OnUpdate()
        {
            CompleteDependency();

            if (PendingBuffers.IsEmpty)
            {
                return;
            }
            
            foreach (var entityCommandBuffer in PendingBuffers)
            {
                if (!entityCommandBuffer.IsEmpty)
                {
                    World.GetExistingSystemManaged<TweenStructuralChangeSystemGroup>().MarkDirty();
                    base.OnUpdate();
                    break;
                }
            }
        }
    }
}
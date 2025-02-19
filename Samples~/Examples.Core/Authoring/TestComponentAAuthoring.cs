using Unity.Entities;
using UnityEngine;

public class TestComponentAAuthoring : MonoBehaviour
{
    public class ComponentABaker : Baker<TestComponentAAuthoring>
    {
        public override void Bake(TestComponentAAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<TestComponentA>(entity);
        }
    }
}
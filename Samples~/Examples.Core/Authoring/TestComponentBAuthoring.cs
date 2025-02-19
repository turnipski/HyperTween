using Unity.Entities;
using UnityEngine;

public class TestComponentBAuthoring : MonoBehaviour
{
    public class TestComponentBBaker : Baker<TestComponentBAuthoring>
    {
        public override void Bake(TestComponentBAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<TestComponentB>(entity);
        }
    }
}
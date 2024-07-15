using Unity.Entities;
using UnityEngine;

namespace ECScape
{
    public class LifeTimeAuthoring : MonoBehaviour
    {
        [SerializeField] private float Value = 10f;

        private class Baker : Baker<LifeTimeAuthoring>
        {
            public override void Bake(LifeTimeAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new LifeTime
                {
                    Value = authoring.Value
                });
            }
        }
    }

    public partial struct LifeTime : IComponentData
    {
        public float Value;
    }
}


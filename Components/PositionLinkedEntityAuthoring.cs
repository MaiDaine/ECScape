using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECScape
{
    public class PositionLinkedEntityAuthoring : MonoBehaviour
    {
        public GameObject LinkedEntity;
        public Vector3 Offset;

        public class Baker : Baker<PositionLinkedEntityAuthoring>
        {
            public override void Bake(PositionLinkedEntityAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

                AddComponent(entity, new PositionLinkedEntity
                {
                    Entity = GetEntity(authoring.LinkedEntity, TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace),
                    Offset = new float3(authoring.Offset.x, authoring.Offset.y, authoring.Offset.z)
                });
            }
        }
    }

    public struct PositionLinkedEntity : IComponentData
    {
        public Entity Entity;
        public float3 Offset;
    }
}


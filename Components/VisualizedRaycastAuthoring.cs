using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace ECScape
{
    [DisallowMultipleComponent]
    public class VisualizedRaycastAuthoring : MonoBehaviour
    {
        public Transform FullRay;
        public Transform HitRay;
        public Transform HitPosition;
        public float RayLength;

        class Baker : Baker<VisualizedRaycastAuthoring>
        {
            public override void Bake(VisualizedRaycastAuthoring authoring)
            {
                Entity fullRayEntity = GetEntity(authoring.FullRay, TransformUsageFlags.Dynamic);
                Entity hitRayEntity = GetEntity(authoring.HitRay, TransformUsageFlags.Dynamic);
                Entity hitPosEntity = GetEntity(authoring.HitPosition, TransformUsageFlags.Dynamic);

                Assert.IsTrue(fullRayEntity != Entity.Null);
                Assert.IsTrue(hitRayEntity != Entity.Null);
                Assert.IsTrue(hitPosEntity != Entity.Null);
                Assert.IsTrue(authoring.RayLength != 0.0f);

                VisualizedRaycast visualizedRaycast = new VisualizedRaycast
                {
                    RayLength = authoring.RayLength,

                    FullRayEntity = fullRayEntity,
                    HitRayEntity = hitRayEntity,
                    HitPositionEntity = hitPosEntity,
                };

                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, visualizedRaycast);
            }
        }
    }

    public struct VisualizedRaycast : IComponentData
    {
        public float RayLength;

        public Entity FullRayEntity;
        public Entity HitRayEntity;
        public Entity HitPositionEntity;
    }
}

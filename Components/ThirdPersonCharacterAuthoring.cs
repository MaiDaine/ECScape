using DOTS_TPC.Demo.CharacterController;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECScape
{
    [DisallowMultipleComponent]
    public class ThirdPersonCharacterAuthoring : MonoBehaviour
    {
        public GameObject BallPrefab;
        public Vector3 BallOffset;

        public AuthoringKinematicCharacterProperties CharacterProperties = AuthoringKinematicCharacterProperties.GetDefault();
        public ThirdPersonCharacterComponent Character = ThirdPersonCharacterComponent.GetDefault();


        public class Baker : Baker<ThirdPersonCharacterAuthoring>
        {
            public override void Bake(ThirdPersonCharacterAuthoring authoring)
            {
                KinematicCharacterUtilities.BakeCharacter(this, authoring, authoring.CharacterProperties);

                Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

                AddComponent(entity, authoring.Character);
                AddComponent(entity, new ThirdPersonCharacterControl());

                if (authoring.BallPrefab != null)
                {
                    Entity prefabEntity = GetEntity(authoring.BallPrefab, TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

                    AddComponent(entity, new PositionLinkedEntity
                    {
                        Entity = prefabEntity,
                        Offset = new float3(authoring.BallOffset.x, authoring.BallOffset.y, authoring.BallOffset.z)
                    });
                }
            }
        }
    }
}

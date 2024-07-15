using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECScape
{
    public class CharacterBallAuthoring : MonoBehaviour
    {
        public Vector3 Offset;

        private class Baker : Baker<CharacterBallAuthoring>
        {
            public override void Bake(CharacterBallAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new CharacterBall
                {
                    Position = new float3(),
                    PreviousMovementDirection = new float3(),
                    Value = 0
                });
            }
        }
    }

    public struct CharacterBall : IComponentData
    {
        public float3 Position;
        public float3 PreviousMovementDirection;
        public float Value;
    }
}

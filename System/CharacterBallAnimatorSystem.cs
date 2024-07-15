using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECScape
{
    public partial struct CharacterBallAnimatorSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CharacterBall>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRW<CharacterBall> ball, RefRW<LocalTransform> localTransform, RefRO<LocalToWorld> localToWorld) in SystemAPI.Query<RefRW<CharacterBall>, RefRW<LocalTransform>, RefRO<LocalToWorld>>())
            {
                if (!ball.ValueRO.Position.Equals(float3.zero))
                {

                    float3 currentPosition = localToWorld.ValueRO.Position;
                    float3 previousPosition = ball.ValueRO.Position;

                    currentPosition.y = 0;
                    previousPosition.y = 0;

                    float3 movementDirection = currentPosition - previousPosition;
                    float movementMagnitude = math.length(movementDirection) * 2500f * SystemAPI.Time.DeltaTime;

                    movementDirection = math.normalize(movementDirection);

                    if (movementMagnitude > 0.1f)
                    {
                        quaternion targetedYRotation = quaternion.LookRotationSafe(movementDirection, new float3(0, 1, 0));

                        if (math.dot(movementDirection, ball.ValueRO.PreviousMovementDirection) <= -0.999f)
                            targetedYRotation = math.mul(targetedYRotation, quaternion.AxisAngle(new float3(0, 1, 0), math.PI));

                        quaternion rotation = math.mul(math.inverse(localToWorld.ValueRO.Rotation), targetedYRotation);
                        quaternion rotationX = quaternion.AxisAngle(new float3(1, 0, 0), ball.ValueRO.Value * math.TORADIANS);

                        rotation = math.mul(rotation, rotationX);

                        quaternion newRotation = math.mul(localTransform.ValueRO.Rotation, rotation);

                        localTransform.ValueRW.Rotation = newRotation;
                        ball.ValueRW.Value += movementMagnitude % 360;
                        ball.ValueRW.PreviousMovementDirection = movementDirection;
                    }
                }

                ball.ValueRW.Position = localToWorld.ValueRO.Position;
            }
        }
    }
}

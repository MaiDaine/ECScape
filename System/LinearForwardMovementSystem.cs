using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECScape
{
    public partial struct LinearForwardMovementSystem : ISystem
    {
        private float _strength;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LinearForwardTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _strength = 10f * Time.deltaTime;

            foreach ((RefRO<LinearForwardTag> _, RefRW<LocalTransform> transform) in SystemAPI.Query<RefRO<LinearForwardTag>, RefRW<LocalTransform>>())
            {
                LocalTransform localTransform = transform.ValueRO;
                float3 position = localTransform.Position;

                position += localTransform.Forward() * _strength;
                transform.ValueRW.Position = position;
            }
        }
    }
}

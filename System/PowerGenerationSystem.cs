using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECScape
{
    [UpdateInGroup(typeof(InteractibleSystemGroup))]
    public partial struct PowerGenerationSystem : ISystem
    {
        private quaternion _rotation;
        private float _strength;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PowerGenerationData>();
            state.RequireForUpdate<SystemConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _strength = SystemAPI.GetComponentRW<SystemStrength>(state.SystemHandle).ValueRO.Value;
            _rotation = quaternion.RotateY(_strength * 2f * SystemAPI.Time.DeltaTime);
            float decay = SystemAPI.Time.DeltaTime / 4f;

            foreach ((RefRO<PowerGenerationData> _, RefRW<LocalTransform> localTransform) in SystemAPI.Query<RefRO<PowerGenerationData>, RefRW<LocalTransform>>())
                localTransform.ValueRW.Rotation = math.mul(_rotation, localTransform.ValueRW.Rotation);

            foreach (RefRW<TileData> tileData in SystemAPI.Query<RefRW<TileData>>())
                tileData.ValueRW.AccumulatedPower = math.max(0f, tileData.ValueRO.AccumulatedPower - decay);
        }
    }
}

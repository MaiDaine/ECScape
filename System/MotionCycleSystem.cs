using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECScape
{
    public partial struct MotionCycleSystem : ISystem
    {
        private float _strength;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MotionCycle>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            _strength = 0.25f * SystemAPI.GetComponentRW<SystemStrength>(state.SystemHandle).ValueRO.Value;

            foreach ((RefRW<MotionCycle> cycle, RefRW<LocalTransform> tranform) in SystemAPI.Query<RefRW<MotionCycle>, RefRW<LocalTransform>>())
            {
                cycle.ValueRW.ElapsedTime += _strength * cycle.ValueRO.Direction * deltaTime;

                if (cycle.ValueRO.ElapsedTime > 1 || cycle.ValueRO.ElapsedTime < 0)
                {
                    cycle.ValueRW.Direction *= -1;
                    cycle.ValueRW.ElapsedTime = math.clamp(cycle.ValueRO.ElapsedTime, 0, 1);
                }

                tranform.ValueRW = new LocalTransform
                {
                    Position = math.lerp(cycle.ValueRO.Start, cycle.ValueRO.End, cycle.ValueRO.ElapsedTime),
                    Rotation = tranform.ValueRO.Rotation,
                    Scale = tranform.ValueRO.Scale
                };
            }
        }
    }
}

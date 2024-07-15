using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECScape
{
    public partial struct ManaReceiverSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ManaReceiver>();
            state.RequireForUpdate<PowerGenerationData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRW<ManaReceiver> manaReceiver, RefRW<PowerGenerationData> powerGeneration) in SystemAPI.Query<RefRW<ManaReceiver>, RefRW<PowerGenerationData>>())
            {
                if (manaReceiver.ValueRW.Value > 0)
                {
                    float tmp = 1 - powerGeneration.ValueRO.Strength;

                    tmp = Mathf.Min(tmp, manaReceiver.ValueRW.Value);
                    manaReceiver.ValueRW.Value = math.max(0f, manaReceiver.ValueRO.Value - tmp);
                    powerGeneration.ValueRW.Strength += tmp;
                }
            }
        }
    }
}

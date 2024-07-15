using Unity.Burst;
using Unity.Entities;

namespace ECScape
{
    public partial struct SceneRule03System : ISystem
    {
        private Entity _configEntity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SceneTag03>();
            state.RequireForUpdate<PowerGenerationData>();
            state.RequireForUpdate<ManaReceiver>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_configEntity == Entity.Null)
                _configEntity = _configEntity = SystemAPI.GetSingletonEntity<SystemConfig>();

            foreach ((RefRO<ManaReceiver> _, RefRO<PowerGenerationData> powerGeneration) in SystemAPI.Query<RefRO<ManaReceiver>, RefRO<PowerGenerationData>>())
            {
                if (powerGeneration.ValueRO.Strength < 0.5f)
                    return;

                SystemAPI.GetComponentRW<SystemConfig>(_configEntity).ValueRW.LoadNextScene = true;
                state.Enabled = false;
            }
        }
    }
}


using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace ECScape
{
    public partial struct SceneRule04System : ISystem
    {
        private Entity _configEntity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SceneTag04>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_configEntity == Entity.Null)
                _configEntity = _configEntity = SystemAPI.GetSingletonEntity<SystemConfig>();

            foreach ((RefRO<ManaReceiver> manaReceiver, RefRO<GateTag> gate) in SystemAPI.Query<RefRO<ManaReceiver>, RefRO<GateTag>>())
            {
                if (manaReceiver.ValueRO.Value > 0f)
                {
                    SystemAPI.GetComponentRW<SystemConfig>(_configEntity).ValueRW.LoadNextScene = true;
                    state.Enabled = false;
                }
            }
        }
    }
}

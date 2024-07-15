using Unity.Burst;
using Unity.Entities;

namespace ECScape
{
    public partial struct SceneRule02System : ISystem
    {
        private Entity _configEntity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SceneTag02>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_configEntity == Entity.Null)
                _configEntity = _configEntity = SystemAPI.GetSingletonEntity<SystemConfig>();

            //There should be only one mana well
            foreach ((RefRO<ManaWell> _, RefRO<RequirePower> requirePower) in SystemAPI.Query<RefRO<ManaWell>, RefRO<RequirePower>>())
            {
                if (requirePower.ValueRO.HasPower == false)
                    return;

                SystemAPI.GetComponentRW<SystemConfig>(_configEntity).ValueRW.LoadNextScene = true;
                state.Enabled = false;
            }
        }
    }
}


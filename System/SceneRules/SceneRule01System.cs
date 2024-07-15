using Unity.Burst;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine;

namespace ECScape
{
    public partial struct SceneRule01System : ISystem
    {
        private Entity _configEntity;
        private EntityQuery _sceneTag01Query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SceneTag01>();
            _sceneTag01Query = state.GetEntityQuery(ComponentType.ReadOnly<PowerGenerationData>());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_configEntity == Entity.Null)
                _configEntity = _configEntity = SystemAPI.GetSingletonEntity<SystemConfig>();

            int count = _sceneTag01Query.CalculateEntityCount();

            if (count > 1)
            {
                SystemAPI.GetComponentRW<SystemConfig>(_configEntity).ValueRW.LoadNextScene = true;
                state.Enabled = false;
            }
        }
    }
}

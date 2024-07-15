using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECScape
{
    public partial struct SceneRule00System : ISystem
    {
        private Entity _configEntity;
        private bool _hasFixSystem;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SystemConfig>();

            _configEntity = Entity.Null;
            _hasFixSystem = false;
        }

        //[BurstCompile] TextMesh
        public void OnUpdate(ref SystemState state)
        {
            if (_configEntity == Entity.Null)
                _configEntity = _configEntity = SystemAPI.GetSingletonEntity<SystemConfig>();

            if (!_hasFixSystem)
            {
                SystemConfig _systemConfig = SystemAPI.GetComponent<SystemConfig>(_configEntity);

                if (_systemConfig.InteractibleSystem != Entity.Null)
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        LocalToWorld playerLocal = SystemAPI.GetComponent<LocalToWorld>(SystemAPI.GetSingletonEntity<PlayerTag>());

                        foreach ((RefRO<LocalToWorld> local, RefRO<ColorOverride> _, Entity entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<ColorOverride>>().WithEntityAccess())
                            if (math.distance(playerLocal.Position, local.ValueRO.Position) < 4.5f)
                                state.EntityManager.SetComponentEnabled<ColorTransition>(entity, true);
                        _hasFixSystem = true;
                    }
                }
            }
            else
            {
                int count = 0;

                foreach (var e in SystemAPI.Query<ColorTransition>())
                    count++;
                if (count == 0)
                {
                    InteractibleSystemData interactibleSystem = SystemAPI.GetSingleton<InteractibleSystemData>();
                    TextMesh textMesh = state.EntityManager.GetComponentObject<TextMesh>(interactibleSystem.TextEntity);

                    textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 1);
                    SystemAPI.GetComponentRW<SystemConfig>(_configEntity).ValueRW.LoadNextScene = true;
                    state.Enabled = false;
                }
            }
        }
    }
}

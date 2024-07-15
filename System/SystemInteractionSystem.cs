using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace ECScape
{
    public partial struct SystemInteractionSystem : ISystem
    {
        private ComponentDataHandles _componentDataHandles;
        private Entity _configEntity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<InteractibleSystemData>();
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<SystemConfig>();

            _componentDataHandles = new ComponentDataHandles(ref state);
            _configEntity = Entity.Null;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_configEntity == Entity.Null)
                _configEntity = _configEntity = SystemAPI.GetSingletonEntity<SystemConfig>();

            _componentDataHandles.Update(ref state);

            state.Dependency = new InteractibleSystemTriggerJob
            {
                InteractibleSystemTagGroup = _componentDataHandles.InteractibleSystemGroup,
                LocalToWorldGroup = _componentDataHandles.LocalToWorldGroup,
                PlayerTagGroup = _componentDataHandles.PlayerGroup,
                SystemConfigGroup = _componentDataHandles.SystemConfigGroup,
                SystemConfigEntity = _configEntity,
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        }

        //This is far from ideal, but it's a quick way to get the system to work and there is not enough entities to affect performances
        [BurstCompile]
        private struct InteractibleSystemTriggerJob : ITriggerEventsJob
        {
            [ReadOnly] public ComponentLookup<InteractibleSystemData> InteractibleSystemTagGroup;
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldGroup;
            [ReadOnly] public ComponentLookup<PlayerTag> PlayerTagGroup;
            public ComponentLookup<SystemConfig> SystemConfigGroup;
            public Entity SystemConfigEntity;

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;
                Entity player = Entity.Null;
                Entity system = Entity.Null;

                if (PlayerTagGroup.HasComponent(entityA))
                    player = entityA;
                else if (PlayerTagGroup.HasComponent(entityB))
                    player = entityB;

                if (InteractibleSystemTagGroup.HasComponent(entityA))
                    system = entityA;
                else if (InteractibleSystemTagGroup.HasComponent(entityB))
                    system = entityB;

                if (player == Entity.Null || system == Entity.Null || player == system)
                    return;

                float3 playerPosition = LocalToWorldGroup[player].Position;
                float3 systemPosition = LocalToWorldGroup[system].Position;
                SystemConfig systemConfig = SystemConfigGroup[SystemConfigEntity];

                if (math.distance(playerPosition, systemPosition) < 4.5f)
                    systemConfig.InteractibleSystem = system;
                else
                    systemConfig.InteractibleSystem = Entity.Null;

                SystemConfigGroup[SystemConfigEntity] = systemConfig;
            }
        }

        private struct ComponentDataHandles
        {
            public ComponentLookup<InteractibleSystemData> InteractibleSystemGroup;
            public ComponentLookup<LocalToWorld> LocalToWorldGroup;
            public ComponentLookup<PlayerTag> PlayerGroup;
            public ComponentLookup<SystemConfig> SystemConfigGroup;

            public ComponentDataHandles(ref SystemState state)
            {
                InteractibleSystemGroup = state.GetComponentLookup<InteractibleSystemData>(true);
                LocalToWorldGroup = state.GetComponentLookup<LocalToWorld>(true);
                PlayerGroup = state.GetComponentLookup<PlayerTag>(true);
                SystemConfigGroup = state.GetComponentLookup<SystemConfig>();
            }

            public void Update(ref SystemState state)
            {
                InteractibleSystemGroup.Update(ref state);
                LocalToWorldGroup.Update(ref state);
                PlayerGroup.Update(ref state);
                SystemConfigGroup.Update(ref state);
            }
        }
    }
}

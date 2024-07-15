using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Burst;
using Unity.Mathematics;

namespace ECScape
{
    public partial struct TileStateSystem : ISystem
    {
        private ComponentDataHandles _componentDataHandles;
        private Entity _configEntity;
        private SystemHandle _powerSystemHandle;
        private float _refreshTimer;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PowerGenerationData>();
            state.RequireForUpdate<SystemConfig>();
            state.RequireForUpdate<TileData>();

            _componentDataHandles = new ComponentDataHandles(ref state);
            _configEntity = Entity.Null;
            _powerSystemHandle = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PowerGenerationSystem>();
            _refreshTimer = 0.0f;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_configEntity == Entity.Null)
                _configEntity = _configEntity = SystemAPI.GetSingletonEntity<SystemConfig>();

            _componentDataHandles.Update(ref state);

            _refreshTimer -= SystemAPI.Time.DeltaTime;

            if (_refreshTimer <= 0.0f)
            {
                _refreshTimer = 1f;
                state.Dependency = new TileStateTriggerJob
                {
                    GridColorOverrideGroup = _componentDataHandles.GridColorOverrideGroup,
                    ManaSpawnerGroup = _componentDataHandles.ManaSpawnerGroup,
                    PowerGenerationGroup = _componentDataHandles.PowerGenerationGroup,
                    PowerMultiplier = SystemAPI.GetComponentRW<SystemStrength>(_powerSystemHandle).ValueRW.Value,
                    TileGroup = _componentDataHandles.TileGroup,
                }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
            }
        }

        //TODO: Is there a better way to filter interactions? Ideally we'd like to only have the relevant filters here
        [BurstCompile]
        private struct TileStateTriggerJob : ITriggerEventsJob
        {
            public ComponentLookup<GridColorOverride> GridColorOverrideGroup;
            public ComponentLookup<RequirePower> ManaSpawnerGroup;
            [ReadOnly] public ComponentLookup<PowerGenerationData> PowerGenerationGroup;
            public float PowerMultiplier;
            public ComponentLookup<TileData> TileGroup;

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;
                Entity other = Entity.Null;
                Entity tile = Entity.Null;

                if (TileGroup.HasComponent(entityA))
                    tile = entityA;
                else if (TileGroup.HasComponent(entityB))
                    tile = entityB;

                if (tile == Entity.Null)
                    return;

                if (PowerGenerationGroup.HasComponent(entityA))
                    other = entityA;
                else if (PowerGenerationGroup.HasComponent(entityB))
                    other = entityB;

                TileData tileComponent = TileGroup[tile];

                if (other == Entity.Null)
                {
                    if (ManaSpawnerGroup.HasComponent(entityA))
                        other = entityA;
                    else if (ManaSpawnerGroup.HasComponent(entityB))
                        other = entityB;
                    else
                        return;

                    RequirePower manaComponent = ManaSpawnerGroup[other];

                    manaComponent.HasPower = tileComponent.AccumulatedPower > 0.25f;
                    ManaSpawnerGroup[other] = manaComponent;
                }
                else
                {
                    PowerGenerationData powerComponent = PowerGenerationGroup[other];
                    GridColorOverride overrideComponent = GridColorOverrideGroup[tile];

                    tileComponent.AccumulatedPower = math.clamp(tileComponent.AccumulatedPower + powerComponent.Strength * PowerMultiplier, 0f, 1f);

                    if (tileComponent.AccumulatedPower > 0)
                        overrideComponent.Value = math.lerp(new float4(1f, 0, 0, 1f), new float4(0.047f, 0.376f, 0.569f, 1f), tileComponent.AccumulatedPower);
                    else
                        overrideComponent.Value = new float4(1f, 0, 0, 1f);
                    GridColorOverrideGroup[tile] = overrideComponent;
                    TileGroup[tile] = tileComponent;
                }
            }
        }

        private struct ComponentDataHandles
        {
            public ComponentLookup<GridColorOverride> GridColorOverrideGroup;
            public ComponentLookup<RequirePower> ManaSpawnerGroup;
            public ComponentLookup<PowerGenerationData> PowerGenerationGroup;
            public ComponentLookup<TileData> TileGroup;

            public ComponentDataHandles(ref SystemState state)
            {
                GridColorOverrideGroup = state.GetComponentLookup<GridColorOverride>();
                ManaSpawnerGroup = state.GetComponentLookup<RequirePower>();
                PowerGenerationGroup = state.GetComponentLookup<PowerGenerationData>(true);
                TileGroup = state.GetComponentLookup<TileData>();
            }

            public void Update(ref SystemState state)
            {
                GridColorOverrideGroup.Update(ref state);
                ManaSpawnerGroup.Update(ref state);
                PowerGenerationGroup.Update(ref state);
                TileGroup.Update(ref state);
            }
        }
    }
}

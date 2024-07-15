using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace ECScape
{
    public partial struct ManaInteractionSystem : ISystem
    {
        private ComponentDataHandles _componentDataHandles;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ManaTag>();
            state.RequireForUpdate<LifeTime>();
            state.RequireForUpdate<SystemConfig>();

            _componentDataHandles = new ComponentDataHandles(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _componentDataHandles.Update(ref state);

            state.Dependency = new ManaInteractionTriggerJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                LifeTimeGroup = _componentDataHandles.LifeTimeGroup,
                LocalTransformGroup = _componentDataHandles.LocalTransformGroup,
                ManaAttractorGroup = _componentDataHandles.ManaAttractorGroup,
                ManaColliderTagGroup = _componentDataHandles.ManaColliderTagGroup,
                ManaReceiverGroup = _componentDataHandles.ManaReceiverGroup,
                ManaTagGroup = _componentDataHandles.ManaTagGroup,
                Strength = SystemAPI.GetComponent<SystemStrength>(state.SystemHandle).Value
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        }

        [BurstCompile]
        private struct ManaInteractionTriggerJob : ITriggerEventsJob
        {
            public float DeltaTime;
            public ComponentLookup<LifeTime> LifeTimeGroup;
            public ComponentLookup<LocalTransform> LocalTransformGroup;
            [ReadOnly] public ComponentLookup<ManaAttractorTag> ManaAttractorGroup;
            [ReadOnly] public ComponentLookup<ManaColliderTag> ManaColliderTagGroup;
            public ComponentLookup<ManaReceiver> ManaReceiverGroup;
            [ReadOnly] public ComponentLookup<ManaTag> ManaTagGroup;
            public float Strength;

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;

                Entity manaEntity;
                Entity receiverEntity = Entity.Null;

                if (ManaTagGroup.HasComponent(entityA))
                    manaEntity = entityA;
                else if (ManaTagGroup.HasComponent(entityB))
                    manaEntity = entityB;
                else
                    return;

                if (ManaColliderTagGroup.HasComponent(entityA) || ManaReceiverGroup.HasComponent(entityA))
                    receiverEntity = entityA;
                else if (ManaColliderTagGroup.HasComponent(entityB) || ManaReceiverGroup.HasComponent(entityB))
                    receiverEntity = entityB;

                if (receiverEntity != Entity.Null)//Collided with an obstacle or a reciver
                {
                    LifeTime lifeTime = LifeTimeGroup[manaEntity];

                    lifeTime.Value = 0.0f;
                    LifeTimeGroup[manaEntity] = lifeTime;

                    if (ManaReceiverGroup.HasComponent(receiverEntity))
                    {
                        ManaReceiver receiver = ManaReceiverGroup[receiverEntity];

                        receiver.Value += 0.05f;
                        ManaReceiverGroup[receiverEntity] = receiver;
                    }
                }
                else
                {
                    if (ManaAttractorGroup.HasComponent(entityA))
                        receiverEntity = entityA;
                    else if (ManaAttractorGroup.HasComponent(entityB))
                        receiverEntity = entityB;
                    else
                        return;
                    //In attractor range

                    float3 attractorPosition = LocalTransformGroup[receiverEntity].Position;
                    LocalTransform manaLocal = LocalTransformGroup[manaEntity];

                    attractorPosition.y = manaLocal.Position.y;
                    float3 direction = math.normalize(attractorPosition - manaLocal.Position);
                    quaternion targetRotation = quaternion.LookRotationSafe(direction, math.up());

                    //slightly rotate the mana towards the attractor depending on the strength and time
                    manaLocal.Rotation = math.slerp(manaLocal.Rotation, targetRotation, Strength * DeltaTime);

                    LocalTransformGroup[manaEntity] = manaLocal;
                }
            }
        }

        private struct ComponentDataHandles
        {
            public ComponentLookup<LifeTime> LifeTimeGroup;
            public ComponentLookup<LocalTransform> LocalTransformGroup;
            public ComponentLookup<ManaAttractorTag> ManaAttractorGroup;
            public ComponentLookup<ManaColliderTag> ManaColliderTagGroup;
            public ComponentLookup<ManaReceiver> ManaReceiverGroup;
            public ComponentLookup<ManaTag> ManaTagGroup;

            public ComponentDataHandles(ref SystemState state)
            {
                LifeTimeGroup = state.GetComponentLookup<LifeTime>();
                LocalTransformGroup = state.GetComponentLookup<LocalTransform>();
                ManaAttractorGroup = state.GetComponentLookup<ManaAttractorTag>(true);
                ManaColliderTagGroup = state.GetComponentLookup<ManaColliderTag>(true);
                ManaReceiverGroup = state.GetComponentLookup<ManaReceiver>();
                ManaTagGroup = state.GetComponentLookup<ManaTag>(true);
            }

            public void Update(ref SystemState state)
            {
                LifeTimeGroup.Update(ref state);
                LocalTransformGroup.Update(ref state);
                ManaAttractorGroup.Update(ref state);
                ManaColliderTagGroup.Update(ref state);
                ManaReceiverGroup.Update(ref state);
                ManaTagGroup.Update(ref state);
            }
        }
    }
}

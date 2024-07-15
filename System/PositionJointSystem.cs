using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECScape
{
    public partial struct PositionJointSystem : ISystem
    {
        private bool _shouldInstantiate;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PositionLinkedEntity>();
            _shouldInstantiate = true;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_shouldInstantiate)
            {
                _shouldInstantiate = false;

                foreach ((RefRO<LocalToWorld> _, RefRW<PositionLinkedEntity> positionLinkedEntity, Entity entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<PositionLinkedEntity>>().WithEntityAccess())
                    state.EntityManager.SetComponentData(entity, new PositionLinkedEntity()
                    {
                        Entity = state.EntityManager.Instantiate(positionLinkedEntity.ValueRO.Entity),
                        Offset = positionLinkedEntity.ValueRO.Offset
                    });
            }
            else
            {
                foreach ((RefRO<LocalToWorld> transform, RefRW<PositionLinkedEntity> positionLinkedEntity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<PositionLinkedEntity>>())
                {
                    LocalTransform childTransform = SystemAPI.GetComponent<LocalTransform>(positionLinkedEntity.ValueRO.Entity);

                    SystemAPI.SetComponent(positionLinkedEntity.ValueRO.Entity, new LocalTransform()
                    {
                        Position = new float3(transform.ValueRO.Position.x, transform.ValueRO.Position.y + positionLinkedEntity.ValueRO.Offset.y, transform.ValueRO.Position.z),
                        Rotation = childTransform.Rotation,
                        Scale = childTransform.Scale
                    }); ;
                }
            }
        }
    }
}

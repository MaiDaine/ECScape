using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECScape
{
    public partial struct LifeTimeSystem : ISystem
    {
        private EntityManager _entityManager;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LifeTime>();
            _entityManager = state.EntityManager;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            float time = SystemAPI.Time.DeltaTime;

            foreach ((RefRW<LifeTime> lifeTime, Entity entity) in SystemAPI.Query<RefRW<LifeTime>>().WithEntityAccess())
            {
                lifeTime.ValueRW.Value -= time;
                if (lifeTime.ValueRO.Value <= 0)
                    ecb.DestroyEntity(entity);
            }
            ecb.Playback(_entityManager);
        }
    }
}

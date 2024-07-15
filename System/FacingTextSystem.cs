using DOTS_TPC;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECScape
{
    [UpdateAfter(typeof(InteractibleSystemGroup))]
    public partial struct FacingTextSystem : ISystem
    {
        private Entity _mainCamera;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FacingText>();
            state.Enabled = false;//TODO: Disable for now as i can't figure out why it works on [Systems] entities but not [Components] entities
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_mainCamera == Entity.Null)
            {
                SystemAPI.TryGetSingletonEntity<TPC_MainCamera>(out _mainCamera);
                return;
            }

            LocalToWorld cameraTransform = SystemAPI.GetComponent<LocalToWorld>(_mainCamera);

            foreach ((RefRO<FacingText> _, RefRW<LocalToWorld> transform) in SystemAPI.Query<RefRO<FacingText>, RefRW<LocalToWorld>>())
            {
                LocalToWorld localToWorld = transform.ValueRO;
                float3 direction = math.normalize(cameraTransform.Position - localToWorld.Position);
                quaternion rotation = quaternion.LookRotationSafe(-direction, math.up());

                transform.ValueRW = new LocalToWorld() { Value = new float4x4(rotation, localToWorld.Position) };
            }
        }
    }
}

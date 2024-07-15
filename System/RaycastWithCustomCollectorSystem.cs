using DOTS_TPC;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace ECScape
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct RaycastWithCustomCollectorSystem : ISystem
    {
        private float3 _copyOffset;
        private float3 _pastOffset;
        private bool _copyStart;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VisualizedRaycast>();
            state.RequireForUpdate<SystemConfig>();
            state.RequireForUpdate<PhysicsWorldSingleton>();

            _copyOffset = new float3(-0.5f, 1.5f, 0);
            _pastOffset = new float3(0.5f, 1.5f, 0);
            _copyStart = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
            {
                if (_copyStart)
                {
                    _copyStart = false;
                    foreach ((RefRO<VisualizedRaycast> _, RefRW<LocalTransform> localTransform) in SystemAPI.Query<RefRO<VisualizedRaycast>, RefRW<LocalTransform>>())
                        localTransform.ValueRW.Position = new float3(0f, -10f, 0f);
                }
                return;
            }
            _copyStart = true;

            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            LocalTransform origin = new LocalTransform();

            foreach ((RefRO<PlayerTag> _, RefRW<LocalTransform> localTransform) in SystemAPI.Query<RefRO<PlayerTag>, RefRW<LocalTransform>>())
                origin = localTransform.ValueRO;

            //offset the origin position with the analyser offset and the origin rotation
            origin.Position += math.mul(origin.Rotation, Input.GetKey(KeyCode.Q) ? _copyOffset : _pastOffset);

            //update the origin rotation to match the camera's rotation
            SystemAPI.TryGetSingletonEntity<TPC_MainCamera>(out Entity mainCameraEntity);
            if (mainCameraEntity != Entity.Null)
            {
                LocalTransform cameraTransform = SystemAPI.GetComponent<LocalTransform>(mainCameraEntity);

                origin.Rotation = cameraTransform.Rotation;
            }

            foreach ((RefRO<VisualizedRaycast> _, RefRW<LocalTransform> localTransform) in SystemAPI.Query<RefRO<VisualizedRaycast>, RefRW<LocalTransform>>())
            {
                localTransform.ValueRW.Position = origin.Position;
                localTransform.ValueRW.Rotation = origin.Rotation;
            }

            RaycastWithCustomCollectorJob raycastJob = new RaycastWithCustomCollectorJob
            {
                LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(false),
                PostTransformMatrices = SystemAPI.GetComponentLookup<PostTransformMatrix>(false),
                PhysicsWorldSingleton = physicsWorldSingleton
            };
            state.Dependency = raycastJob.Schedule(state.Dependency);
        }

        [BurstCompile]
        public partial struct RaycastWithCustomCollectorJob : IJobEntity
        {
            public ComponentLookup<LocalTransform> LocalTransforms;
            public ComponentLookup<PostTransformMatrix> PostTransformMatrices;

            [Unity.Collections.ReadOnly] public PhysicsWorldSingleton PhysicsWorldSingleton;

            public void Execute(Entity entity, ref VisualizedRaycast visualizedRaycast, ref SystemConfig systemConfig)
            {
                LocalTransform rayLocalTransform = LocalTransforms[entity];
                float raycastLength = visualizedRaycast.RayLength;


                // Perform the Raycast
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = rayLocalTransform.Position,
                    End = rayLocalTransform.Position + rayLocalTransform.Forward() * visualizedRaycast.RayLength,

                    Filter = CollisionFilter.Default
                };

                IgnoreTransparentClosestHitCollector collector = new IgnoreTransparentClosestHitCollector(PhysicsWorldSingleton.CollisionWorld);
                PhysicsWorldSingleton.CastRay(raycastInput, ref collector);
                Unity.Physics.RaycastHit hit = collector.ClosestHit;
                float hitDistance = raycastLength * hit.Fraction;

                // position the entities and scale based on the ray length and hit distance
                // visualization elements are scaled along the z-axis aka math.forward
                float3 newFullRayPosition = new float3(0, 0, raycastLength * 0.5f);
                float3 newHitPosition = new float3(0, 0, hitDistance);
                float3 newHitRayPosition = new float3(0, 0, hitDistance * 0.5f);
                float3 newFullRayScale = new float3(.025f, .025f, raycastLength * 0.5f);
                float3 newHitRayScale = new float3(0.1f, 0.1f, raycastLength * hit.Fraction);

                LocalTransforms[visualizedRaycast.HitPositionEntity] =
                    LocalTransforms[visualizedRaycast.HitPositionEntity].WithPosition(newHitPosition);
                LocalTransforms[visualizedRaycast.HitRayEntity] = LocalTransforms[visualizedRaycast.HitRayEntity]
                    .WithPosition(newHitRayPosition).WithScale(1);
                PostTransformMatrices[visualizedRaycast.HitRayEntity] = new PostTransformMatrix
                {
                    Value = float4x4.Scale(newHitRayScale)
                };
                LocalTransforms[visualizedRaycast.FullRayEntity] = LocalTransforms[visualizedRaycast.FullRayEntity]
                    .WithPosition(newFullRayPosition).WithScale(1);
                PostTransformMatrices[visualizedRaycast.FullRayEntity] = new PostTransformMatrix
                {
                    Value = float4x4.Scale(newFullRayScale)
                };

                //update system config with the hit entity
                if (hit.Entity != Entity.Null)
                {
                    systemConfig.AnalasedEntity = hit.Entity;
                }
            }
        }
    }
}

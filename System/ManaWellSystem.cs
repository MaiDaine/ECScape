using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECScape
{
    public partial struct ManaWellSystem : ISystem
    {
        private Unity.Mathematics.Random _random;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ManaWell>();
            _random = new Unity.Mathematics.Random(0x6E624EB7u);//this is a simulation after all :>
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRW<ManaWell> manaWell, RefRO<CustomAnimationCurve> animationCurve, RefRO<LocalToWorld> translation, RefRO<RequirePower> requirePower)
                in SystemAPI.Query<RefRW<ManaWell>, RefRO<CustomAnimationCurve>, RefRO<LocalToWorld>, RefRO<RequirePower>>())
            {
                if (manaWell.ValueRO.SpawnPattern == SpawnPattern.Portal && requirePower.ValueRO.HasPower == false)
                {
                    manaWell.ValueRW.SpawnCount = 0;
                    manaWell.ValueRW.ElapsedTime = 0f;
                    return;
                }

                if (manaWell.ValueRO.SpawnCount >= manaWell.ValueRO.TotalSpawnCount)
                {
                    if (manaWell.ValueRO.SpawnPattern == SpawnPattern.Portal)
                    {
                        if (manaWell.ValueRO.ElapsedTime >= animationCurve.ValueRO.Duration)
                        {
                            manaWell.ValueRW.SpawnCount = 0;
                            manaWell.ValueRW.ElapsedTime = 0f;
                        }
                        else
                            manaWell.ValueRW.ElapsedTime += SystemAPI.Time.DeltaTime;
                    }
                    return;
                }

                manaWell.ValueRW.ElapsedTime += SystemAPI.Time.DeltaTime;

                int count = (int)math.round(
                    manaWell.ValueRO.TotalSpawnCount
                    * (float)AnimationCurveUtils.EvaluateCurve(animationCurve.ValueRO.KeyframesBlob, manaWell.ValueRO.ElapsedTime / animationCurve.ValueRO.Duration)
                    - manaWell.ValueRO.SpawnCount);

                if (count < 0)
                    return;

                if (count > manaWell.ValueRO.TotalSpawnCount)
                    count = manaWell.ValueRO.TotalSpawnCount - manaWell.ValueRO.SpawnCount;

                manaWell.ValueRW.SpawnCount += count;

                for (int i = 0; i < count; i++)
                {
                    Entity entity = state.EntityManager.Instantiate(manaWell.ValueRO.ManaPrefab);
                    int index = manaWell.ValueRO.SpawnCount - i;
                    int max = manaWell.ValueRO.TotalSpawnCount;

                    if (manaWell.ValueRO.SpawnPattern == SpawnPattern.Portal)
                    {
                        state.EntityManager.SetComponentData(entity, new LocalTransform
                        {
                            Position = translation.ValueRO.Position + CubeComputeTargetedPosition(),
                            Rotation = translation.ValueRO.Rotation,
                            Scale = 0.1f
                        });
                    }
                    else if (manaWell.ValueRO.SpawnPattern == SpawnPattern.Gate)
                    {
                        state.EntityManager.SetComponentData(entity, new LocalTransform
                        {
                            Position = translation.ValueRO.Position,
                            Rotation = quaternion.identity,
                            Scale = 0.1f
                        });
                        state.EntityManager.SetComponentData(entity, new AnimatedMana
                        {
                            ElapsedTime = 0f,
                            TotalTime = animationCurve.ValueRO.Duration,
                            InPosition = translation.ValueRO.Position + SphereComputeTargetedPosition(index, max),
                            OutPosition = translation.ValueRO.Position + SphereComputeTargetedPosition(index, max, -0.5f),
                            Phase = 0,
                            StartPosition = translation.ValueRO.Position
                        });
                    }
                }
            }
        }

        [BurstCompile]
        private float3 CubeComputeTargetedPosition() => new float3(0f, _random.NextFloat(-0.85f, 0.85f), _random.NextFloat(-0.85f, 0.85f));

        [BurstCompile]
        private float3 SphereComputeTargetedPosition(int index, int max, float pow = 0.5f)
        {
            float dst = math.pow(index / (max - 1f), pow);
            float angle = 2 * math.PI * index * 0.618033f;

            float x = math.cos(angle) * dst;
            float y = math.sin(angle) * dst;
            float ratio = 2.5f;

            return new float3(0f, (float)x * ratio, (float)y * ratio);
        }
    }
}

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECScape
{
    public partial struct AnimateManaSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AnimatedMana>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new AnimateManaJob { DeltaTime = SystemAPI.Time.DeltaTime }.Schedule();
        }
    }

    [BurstCompile]
    public partial struct AnimateManaJob : IJobEntity
    {
        public float DeltaTime;

        void Execute(Entity _, ref AnimatedMana mana, ref LocalTransform transform)
        {
            if (mana.Phase > 3)
                return;

            if (mana.ElapsedTime < mana.TotalTime)
            {
                mana.ElapsedTime += DeltaTime;

                if (mana.Phase == 0)
                    transform.Position = math.lerp(transform.Position, mana.InPosition, mana.ElapsedTime / mana.TotalTime);
                //else if (mana.Phase == 1)
                else if (mana.Phase == 2)
                    transform.Position = math.lerp(transform.Position, mana.StartPosition, mana.ElapsedTime / mana.TotalTime);
                else if (mana.Phase == 3)
                    transform.Position = math.lerp(transform.Position, mana.OutPosition, mana.ElapsedTime / mana.TotalTime);
            }
            else
            {
                mana.Phase++;
                mana.ElapsedTime = 0f;
            }
        }
    }
}


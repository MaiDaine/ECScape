using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace ECScape
{
    public partial struct ColorTransitionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ColorTransition>();
            state.RequireForUpdate<CustomAnimationCurve>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRW<ColorTransition> colorTransition, RefRO<CustomAnimationCurve> animationCurve, RefRW<ColorOverride> color, Entity entity)
                in SystemAPI.Query<RefRW<ColorTransition>, RefRO<CustomAnimationCurve>, RefRW<ColorOverride>>().WithEntityAccess())
            {
                colorTransition.ValueRW.ElapsedTime += SystemAPI.Time.DeltaTime;

                if (colorTransition.ValueRO.ElapsedTime >= animationCurve.ValueRO.Duration)
                {
                    color.ValueRW.Value = colorTransition.ValueRO.TargetColor;

                    if (animationCurve.ValueRO.ShouldDestroyOnEnd)//TODO: Destroy
                        SystemAPI.SetComponentEnabled<ColorTransition>(entity, false);
                }
                else
                    color.ValueRW.Value = math.lerp(
                        colorTransition.ValueRO.BaseColor,
                        colorTransition.ValueRO.TargetColor,
                        AnimationCurveUtils.EvaluateCurve(animationCurve.ValueRO.KeyframesBlob, colorTransition.ValueRO.ElapsedTime / animationCurve.ValueRO.Duration)
                    );
            }
        }
    }
}

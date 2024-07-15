using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECScape
{
    public class AnimatedManaAuthoring : MonoBehaviour
    {
        private class Baker : Baker<AnimatedManaAuthoring>
        {
            public override void Bake(AnimatedManaAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new AnimatedMana
                {
                    ElapsedTime = 0f,
                    TotalTime = 0f,
                    InPosition = new float3(),
                    OutPosition = new float3(),
                    Phase = 0,
                    StartPosition = new float3()
                });
            }
        }
    }

    public partial struct AnimatedMana : IComponentData
    {
        public float ElapsedTime;
        public float TotalTime;
        public float3 InPosition;
        public float3 OutPosition;
        public int Phase;
        public float3 StartPosition;
    }
}

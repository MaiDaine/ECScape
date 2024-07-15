using Unity.Entities;
using UnityEngine;

namespace ECScape
{
    public class PowerGenerationAuthoring : MonoBehaviour
    {
        [SerializeField] private float Strength = 1.0f;
        private class Baker : Baker<PowerGenerationAuthoring>
        {
            public override void Bake(PowerGenerationAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new PowerGenerationData { Strength = authoring.Strength });
            }
        }
    }

    public struct PowerGenerationData : IComponentData
    {
        public float Strength;
    }
}

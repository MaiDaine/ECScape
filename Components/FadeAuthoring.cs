using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace ECScape
{
    public class FadeAuthoring : MonoBehaviour
    {
        [SerializeField] private Material Material;

        private class Baker : Baker<FadeAuthoring>
        {
            public override void Bake(FadeAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponentObject(entity, new Fade
                {
                    ElapsedTime = 0f,
                    Material = authoring.Material
                });
                AddComponent(entity, new FadeInOverrideTime { Value = 0f });
                AddComponent(entity, new FadeInOverrideColor
                { Value = new float4(authoring.Material.color.r, authoring.Material.color.g, authoring.Material.color.b, authoring.Material.color.a) });
            }
        }
    }

    public class Fade : IComponentData
    {
        public float ElapsedTime;
        public Material Material;
    }

    [MaterialProperty("_CustomTime")]
    public struct FadeInOverrideTime : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_CustomColor")]
    public struct FadeInOverrideColor : IComponentData
    {
        public float4 Value;
    }
}

using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace ECScape
{
    public class TileAuthoring : MonoBehaviour
    {
        [ColorUsageAttribute(true, true)]
        [SerializeField] private Color _initColor;

        private class Baker : Baker<TileAuthoring>
        {
            public override void Bake(TileAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new TileData
                {
                    AccumulatedPower = 0.0f
                });
                AddComponent(entity, new GridColorOverride { Value = new float4(authoring._initColor.r, authoring._initColor.g, authoring._initColor.b, authoring._initColor.a) });
            }
        }
    }

    public struct TileData : IComponentData
    {
        public float AccumulatedPower;
    }

    [MaterialProperty("_GridColor")]
    public struct GridColorOverride : IComponentData
    {
        public float4 Value;
    }
}

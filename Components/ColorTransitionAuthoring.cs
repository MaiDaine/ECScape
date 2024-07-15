using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace ECScape
{
    public class ColorTransitionAuthoring : MonoBehaviour
    {
        [SerializeField] private bool StartEnabled = false;
        [SerializeField] private Color TargetColor;

        private class Baker : Baker<ColorTransitionAuthoring>
        {
            public override void Bake(ColorTransitionAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                Color goColor = authoring.gameObject.GetComponent<MeshRenderer>().sharedMaterial.color;

                AddComponent(entity, new ColorTransition
                {
                    BaseColor = new float4(goColor.r, goColor.g, goColor.b, goColor.a),
                    ElapsedTime = 0f,
                    TargetColor = new float4(authoring.TargetColor.r, authoring.TargetColor.g, authoring.TargetColor.b, authoring.TargetColor.a)
                });
                AddComponent(entity, new ColorOverride { Value = new float4(goColor.r, goColor.g, goColor.b, goColor.a) });
                SetComponentEnabled<ColorTransition>(entity, authoring.StartEnabled);
            }
        }
    }

    public struct ColorTransition : IComponentData, IEnableableComponent
    {
        public float4 BaseColor;
        public float ElapsedTime;
        public float4 TargetColor;  
    }

    [MaterialProperty("_Color")]
    public struct ColorOverride : IComponentData
    {
        public float4 Value;
    }
}

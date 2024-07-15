using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace ECScape
{
    [RequireMatchingQueriesForUpdate]
    public partial class FadeSystem : SystemBase
    {
        private Dictionary<Material, BatchMaterialID> _materialMapping;

        //From EntityComponentSystemSamples\GraphicsSamples\URPSamples\Assets\SampleScenes\4. API Examples\MaterialMeshChange\SceneAssets\MaterialChangerAuthoring.cs
        private void RegisterMaterial(EntitiesGraphicsSystem hybridRendererSystem, Material material)
        {
            // Only register each mesh once, so we can also unregister each mesh just once
            if (!_materialMapping.ContainsKey(material))
                _materialMapping[material] = hybridRendererSystem.RegisterMaterial(material);
        }
        private void UnregisterMaterials()
        {
            // Can't call this from OnDestroy(), so we can't do this on teardown
            EntitiesGraphicsSystem hybridRenderer = World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            if (hybridRenderer == null)
                return;

            foreach (KeyValuePair<Material, BatchMaterialID> kv in _materialMapping)
                hybridRenderer.UnregisterMaterial(kv.Value);
        }

        protected override void OnStartRunning()
        {
            EntitiesGraphicsSystem hybridRenderer = World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();

            _materialMapping = new Dictionary<Material, BatchMaterialID>();

            Entities
                .WithoutBurst()
                .ForEach((in Fade fade) => { RegisterMaterial(hybridRenderer, fade.Material); }).Run();
        }


        protected override void OnUpdate()
        {
            float dT = SystemAPI.Time.DeltaTime;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            Entities
                .WithoutBurst()
                .ForEach((Entity entity, Fade fade, ref FadeInOverrideTime fadeInOverride, ref CustomAnimationCurve curve, ref MaterialMeshInfo mmi) =>
                {
                    if (fade.ElapsedTime < curve.Duration)
                    {
                        fade.ElapsedTime += dT;
                        fadeInOverride.Value = math.lerp(0f, 1f, AnimationCurveUtils.EvaluateCurve(curve.KeyframesBlob, fade.ElapsedTime / curve.Duration));
                    }
                    else
                    {
                        mmi.MaterialID = _materialMapping[fade.Material];

                        ecb.RemoveComponent<Fade>(entity);
                        ecb.RemoveComponent<FadeInOverrideTime>(entity);
                        ecb.RemoveComponent<CustomAnimationCurve>(entity);
                    }

                }).Run();

            ecb.Playback(EntityManager);
        }
    }
}


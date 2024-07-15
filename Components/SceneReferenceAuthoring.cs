using Unity.Entities.Serialization;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace ECScape
{
    //from EntitiesSamples/Assets/Streaming/SceneManagement/1. SceneLoading/SceneReferenceAuthoring.cs
    public class SceneReferenceAuthoring : MonoBehaviour
    {
#if UNITY_EDITOR
        public SceneAsset scene;

        class Baker : Baker<SceneReferenceAuthoring>
        {
            public override void Bake(SceneReferenceAuthoring authoring)
            {
                DependsOn(authoring.scene);

                if (authoring.scene != null)
                {
                    Entity entity = GetEntity(TransformUsageFlags.None);
                    
                    AddComponent(entity, new SceneReference { Value = new EntitySceneReference(authoring.scene) });
                }
            }
        }
#endif
    }

    public struct SceneReference : IComponentData
    {
        public EntitySceneReference Value;
    }
}
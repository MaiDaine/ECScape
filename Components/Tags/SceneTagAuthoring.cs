using Unity.Entities;
using UnityEngine;

namespace ECScape
{
    public class SceneTagAuthoring : MonoBehaviour
    {
        [SerializeField] private bool _isSceneTag00 = false;
        [SerializeField] private bool _isSceneTag01 = false;
        [SerializeField] private bool _isSceneTag02 = false;
        [SerializeField] private bool _isSceneTag03 = false;
        [SerializeField] private bool _isSceneTag04 = false;
        [SerializeField] private bool _isSceneTag05 = false;

        private class Baker : Baker<SceneTagAuthoring>
        {
            public override void Bake(SceneTagAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                if (authoring._isSceneTag00)
                    AddComponent(entity, new SceneTag00 { });
                else if (authoring._isSceneTag01)
                    AddComponent(entity, new SceneTag01 { });
                else if (authoring._isSceneTag02)
                    AddComponent(entity, new SceneTag02 { });
                else if (authoring._isSceneTag03)
                    AddComponent(entity, new SceneTag03 { });
                else if (authoring._isSceneTag04)
                    AddComponent(entity, new SceneTag04 { });
                else if (authoring._isSceneTag05)
                    AddComponent(entity, new SceneTag05 { });
            }
        }
    }

    //I think the correct way to do this is to have a single SceneTag component for detection logic
    //Then all scene system start disabled and we use the previous system to enable the next one
    //But this is easier to work with for prototyping
    #region SceneTag
    public struct SceneTag00 : IComponentData { }
    public struct SceneTag01 : IComponentData { }
    public struct SceneTag02 : IComponentData { }
    public struct SceneTag03 : IComponentData { }
    public struct SceneTag04 : IComponentData { }
    public struct SceneTag05 : IComponentData { }
    #endregion
}

using Unity.Entities;
using UnityEngine;

namespace ECScape
{
    public class TagAuthoring : MonoBehaviour
    {
        [SerializeField] private bool AddFacingTextTag = false;
        [SerializeField] private bool GateTag = false;
        [SerializeField] private bool AddPlayerTag = false;

        [SerializeField] private bool AddLinearForward = false;
        [SerializeField] private bool AddManaAttractorTag = false;
        [SerializeField] private bool AddManaTag = false;
        [SerializeField] private bool AddManaColliderTag = false;
        [SerializeField] private bool AddManaReceiver = false;

        private class Baker : Baker<TagAuthoring>
        {
            public override void Bake(TagAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                if (authoring.AddFacingTextTag)
                    AddComponent(entity, new FacingText { });
                if (authoring.GateTag)
                    AddComponent(entity, new GateTag { });
                if (authoring.AddPlayerTag)
                    AddComponent(entity, new PlayerTag { });

                if (authoring.AddLinearForward)
                    AddComponent(entity, new LinearForwardTag { });
                if (authoring.AddManaAttractorTag)
                    AddComponent(entity, new ManaAttractorTag { });
                if (authoring.AddManaTag)
                    AddComponent(entity, new ManaTag { });
                if (authoring.AddManaColliderTag)
                    AddComponent(entity, new ManaColliderTag { });
                if (authoring.AddManaReceiver)
                    AddComponent(entity, new ManaReceiver { Value = 0.0f });
            }
        }
    }

    public struct FacingText : IComponentData { }
    public struct GateTag : IComponentData { }
    public struct PlayerTag : IComponentData { }

    #region ManaTags
    public struct LinearForwardTag : IComponentData { }
    public struct ManaAttractorTag : IComponentData { }
    public struct ManaTag : IComponentData { }
    public struct ManaColliderTag : IComponentData { }
    public struct ManaReceiver : IComponentData { public float Value; }
    #endregion
}
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ECScape
{
    public class DebugInteractibleAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject TextPrefab;

        public class Baker : Baker<DebugInteractibleAuthoring>
        {
            public override void Bake(DebugInteractibleAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
                Entity textEntity = GetEntity(authoring.TextPrefab, TransformUsageFlags.Dynamic);

                AddComponent(entity, new DebugInteractible { TextEntity = textEntity });
            }
        }
    }

    public struct DebugInteractible : IComponentData
    {
        public Entity TextEntity;
    }
}

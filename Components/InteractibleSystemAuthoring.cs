using Unity.Entities;
using UnityEngine;

namespace ECScape
{
    public class InteractibleSystemAuthoring : MonoBehaviour
    {
        [SerializeField] private InteractibleSystemType SystemPropertyModifiderIndex;
        [SerializeField] private GameObject TextPrefab;

        private class Baker : Baker<InteractibleSystemAuthoring>
        {
            public override void Bake(InteractibleSystemAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                if (authoring.TextPrefab != null)
                {
                    Entity textEntity = GetEntity(authoring.TextPrefab, TransformUsageFlags.Dynamic);

                    AddComponent(entity, new InteractibleSystemData
                    {
                        SystemPropertyModifiderIndex = (int)authoring.SystemPropertyModifiderIndex,
                        TextEntity = textEntity
                    });
                }
            }
        }
    }

    public struct InteractibleSystemData : IComponentData
    {
        public int SystemPropertyModifiderIndex;
        public Entity TextEntity;
    }
}

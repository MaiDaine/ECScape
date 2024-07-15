using Unity.Entities;
using UnityEngine;

namespace ECScape
{
    public class ManaWellAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject ManaPrefab;
        [SerializeField] private int SpawnCount;
        [SerializeField] private SpawnPattern SpawnPattern;

        private class Baker : Baker<ManaWellAuthoring>
        {
            public override void Bake(ManaWellAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new ManaWell
                {
                    ManaPrefab = GetEntity(authoring.ManaPrefab, TransformUsageFlags.Dynamic),
                    ElapsedTime = 0f,
                    SpawnCount = 0,
                    SpawnPattern = authoring.SpawnPattern,
                    TotalSpawnCount = authoring.SpawnCount,
                });

                AddComponent(entity, new RequirePower { HasPower = false});
            }
        }
    }

    public enum SpawnPattern
    {
        Portal,
        Gate
    }

    public partial struct ManaWell : IComponentData
    {
        public Entity ManaPrefab;
        public float ElapsedTime;
        public int SpawnCount;
        public SpawnPattern SpawnPattern;
        public int TotalSpawnCount;
    }

    public struct RequirePower : IComponentData
    {
        public bool HasPower;
    }
}

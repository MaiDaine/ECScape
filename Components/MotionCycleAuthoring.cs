using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECScape
{
    public class MotionCycleAuthoring : MonoBehaviour
    {
        [SerializeField] private Vector3 EndOffset;
        [SerializeField] private Vector3 StartOffset;
        [SerializeField] private float StartTime = 0.5f;

        private class Baker : Baker<MotionCycleAuthoring>
        {
            public override void Bake(MotionCycleAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                float3 position = authoring.gameObject.transform.position;

                AddComponent(entity, new MotionCycle
                {
                    Direction = 1,
                    ElapsedTime = authoring.StartTime,
                    End = position + new float3(authoring.EndOffset.x, authoring.EndOffset.y, authoring.EndOffset.z),
                    EndOffset = authoring.EndOffset,
                    Start = position + new float3(authoring.StartOffset.x, authoring.StartOffset.y, authoring.StartOffset.z),
                    StartOffset = authoring.StartOffset
                });
            }
        }
    }

    //TODO merge with AnimatedMana
    public struct MotionCycle : IComponentData
    {
        public int Direction;
        public float ElapsedTime;
        public float3 End;
        public float3 EndOffset;
        public float3 Start;
        public float3 StartOffset;
    }
}

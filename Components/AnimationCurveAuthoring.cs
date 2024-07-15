using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECScape
{
    public class AnimationCurveAuthoring : MonoBehaviour
    {
        [SerializeField] private AnimationCurve Curve;
        [SerializeField] private float Duration;
        [SerializeField] private bool ShouldDestroyOnEnd;

        private class Baker : Baker<AnimationCurveAuthoring>
        {
            public override void Bake(AnimationCurveAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new CustomAnimationCurve
                {
                    Duration = authoring.Duration,
                    KeyframesBlob = AnimationCurveUtils.SerializeCurve(authoring.Curve),
                    ShouldDestroyOnEnd = authoring.ShouldDestroyOnEnd,
                });
            }
        }
    }

    public struct CustomAnimationCurve : IComponentData
    {
        public float Duration;
        public BlobAssetReference<KeyframeBlobArray> KeyframesBlob;
        public bool ShouldDestroyOnEnd;
    }

    public struct KeyframeBlobArray
    {
        public BlobArray<KeyframeData> Keyframes;
    }

    public struct KeyframeData
    {
        public float Time;
        public float Value;
        public float InTangent;
        public float OutTangent;
    }

    public static class AnimationCurveUtils
    {
        public static BlobAssetReference<KeyframeBlobArray> SerializeCurve(AnimationCurve curve)
        {
            using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
            {
                ref KeyframeBlobArray keyframeBlobArray = ref blobBuilder.ConstructRoot<KeyframeBlobArray>();
                BlobBuilderArray<KeyframeData> keyframes = blobBuilder.Allocate(ref keyframeBlobArray.Keyframes, curve.length);

                for (int i = 0; i < curve.length; i++)
                {
                    keyframes[i] = new KeyframeData
                    {
                        Time = curve.keys[i].time,
                        Value = curve.keys[i].value,
                        InTangent = curve.keys[i].inTangent,
                        OutTangent = curve.keys[i].outTangent,
                    };
                }

                return blobBuilder.CreateBlobAssetReference<KeyframeBlobArray>(Allocator.Persistent);//TODO dispose
            }
        }

        public static float EvaluateCurve(BlobAssetReference<KeyframeBlobArray> curveBlob, float time)
        {
            ref var keyframes = ref curveBlob.Value.Keyframes;

            time = math.clamp(time, 0f, 1f);

            for (int i = 0; i < keyframes.Length - 1; i++)
            {
                if (time >= keyframes[i].Time && time < keyframes[i + 1].Time)
                {
                    float t = (time - keyframes[i].Time) / (keyframes[i + 1].Time - keyframes[i].Time);

                    return math.lerp(keyframes[i].Value, keyframes[i + 1].Value, t);
                }
            }

            return keyframes[keyframes.Length - 1].Value;
        }
    }
}

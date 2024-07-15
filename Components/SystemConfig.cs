using Unity.Entities;
using UnityEngine;

namespace ECScape
{
    public struct SystemConfig : IComponentData
    {
        public Entity AnalasedEntity;
        public Entity InteractibleSystem;
        public bool LoadNextScene;
    }

    public enum InteractibleSystemType
    {
        None = 0,
        PowerGeneration = 1,
        MoveCycle = 2,
        Gravity = 3,
    }

    public struct SystemStrength : IComponentData
    {
        public float Value;
    }
}
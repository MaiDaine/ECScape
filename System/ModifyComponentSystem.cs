using System.Linq;
using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ECScape
{
    [UpdateAfter(typeof(InteractibleSystemGroup))]
    public partial struct ModifyComponentSystem : ISystem
    {
        private Entity _configEntity;
        private Entity _copyEntity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SystemConfig>();

            _configEntity = Entity.Null;
            _copyEntity = Entity.Null;
        }

        //[BurstCompile] TextMesh
        public void OnUpdate(ref SystemState state)
        {
            if (_configEntity == Entity.Null)
                _configEntity = _configEntity = SystemAPI.GetSingletonEntity<SystemConfig>();

            SystemConfig systemConfig = SystemAPI.GetComponent<SystemConfig>(_configEntity);

            //Copy
            if (Input.GetKey(KeyCode.Q))
            {
                if (systemConfig.AnalasedEntity != Entity.Null)
                    _copyEntity = systemConfig.AnalasedEntity;
            }
            //Paste
            else if (Input.GetKey(KeyCode.E))
            {
                Entity entity = SystemAPI.GetComponent<SystemConfig>(_configEntity).AnalasedEntity;

                if (entity != Entity.Null)
                    CopyComponents(ref state, _copyEntity, entity);

                if (SystemAPI.HasComponent<ColorTransition>(entity))
                {
                    ColorTransition colorTransition = SystemAPI.GetComponent<ColorTransition>(entity);

                    colorTransition.ElapsedTime = 0;
                    SystemAPI.SetComponent(entity, colorTransition);
                    SystemAPI.SetComponentEnabled<ColorTransition>(entity, true);
                }
            }
        }

        //TODO: When done dynamically i haven't found a way to copy the values of the components and we would a way to handle specific cases
        //[BurstCompile] TextMesh
        private void CopyComponents(ref SystemState state, Entity source, Entity target)
        {
            EntityManager entityManager = state.EntityManager;
            DebugInteractible textEntity = state.EntityManager.GetComponentData<DebugInteractible>(target);
            TextMesh textMesh = state.EntityManager.GetComponentObject<TextMesh>(textEntity.TextEntity);
            string text = "";

            if (entityManager.HasComponent<PowerGenerationData>(target))
                text += "A";
            else if (entityManager.HasComponent<PowerGenerationData>(source))
            {
                entityManager.AddComponentData(target, entityManager.GetComponentData<PowerGenerationData>(source));
                text += "A";
            }

            if (entityManager.HasComponent<MotionCycle>(target))
                text += "B";
            else if (entityManager.HasComponent<MotionCycle>(source))
            {
                MotionCycle sourceComponent = entityManager.GetComponentData<MotionCycle>(source);
                LocalTransform targetTransform = entityManager.GetComponentData<LocalTransform>(target);

                entityManager.AddComponentData(target, new MotionCycle
                {
                    Direction = sourceComponent.Direction,
                    ElapsedTime = 0.5f,
                    End = targetTransform.Position + sourceComponent.EndOffset,
                    EndOffset = sourceComponent.EndOffset,
                    Start = targetTransform.Position + sourceComponent.StartOffset,
                    StartOffset = sourceComponent.StartOffset
                });
                text += "B";
            }

            if (entityManager.HasComponent<ManaAttractorTag>(target))
                text += "C";
            else if (entityManager.HasComponent<ManaAttractorTag>(source))
            {
                entityManager.AddComponent<ManaAttractorTag>(target);
                text += "C";
            }

            textMesh.text = text;
        }
    }
}

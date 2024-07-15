using DOTS_TPC;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using Unity.Scenes;
using UnityEngine;

namespace ECScape
{
    public class UIBindings : MonoBehaviour
    {
        [SerializeField] private Material DebugMaterial;
        [SerializeField] private TextMeshProUGUI Helper;
        [SerializeField] private HelperTextsSO HelperTexts;
        [SerializeField] private TextMeshProUGUI InfoClippy;
        [SerializeField] private HelperTextsSO InfoClippyTexts;
        [SerializeField] private PopupController PopupController;

        [SerializeField] private EntitySceneReference FirstSubscene;

        private Entity _config;
        private Entity _currentSystem;
        private InteractibleSystemType _currentSystemType;
        private EntityManager _entityManager;
        private int _helperIndex = 0;
        private bool _initComplete = false;
        private int _sceneIndex = 0;
        private EntityQuery _sceneManagementQuery;
        private bool _systemPopupActive = false;
        private List<SystemHandle> _systems;
        private float _timerText = 0f;

        public void Start()
        {
            Application.targetFrameRate = 60;
            InfoClippy.text = InfoClippyTexts.HelperTexts[0];
            Helper.text = HelperTexts.HelperTexts[0];
            Cursor.visible = false;

            World world = World.DefaultGameObjectInjectionWorld;
            _entityManager = world.EntityManager;

            _currentSystem = Entity.Null;
            _currentSystemType = InteractibleSystemType.None;
            _sceneManagementQuery = _entityManager.CreateEntityQuery(typeof(SceneReference));

            SceneSystem.LoadSceneAsync(World.DefaultGameObjectInjectionWorld.Unmanaged, FirstSubscene);
        }

        //TODO Needs cleaner way to init
        //Turns out SubScene can only autoload in editor mode so this is a placeholder
        public void InitScene()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            _entityManager = world.EntityManager;
            _config = _entityManager.CreateEntityQuery(typeof(VisualizedRaycast)).GetSingletonEntity();
            _entityManager.AddComponentData(_config,
                new SystemConfig
                {
                    AnalasedEntity = Entity.Null,
                    LoadNextScene = false,
                    InteractibleSystem = Entity.Null,
                });
            _entityManager.AddComponent<PlayerInputEnable>(_config);
            _entityManager.SetComponentData(_config, new PlayerInputEnable() { State = true });

            _systems = new List<SystemHandle>
            {
                world.GetExistingSystem<PowerGenerationSystem>(),
                world.GetExistingSystem<MotionCycleSystem>(),
                world.GetExistingSystem<ManaInteractionSystem>(),
            };
            _systems.ForEach(system =>
            {
                _entityManager.AddComponent(system, typeof(SystemStrength));
                _entityManager.GetComponentDataRW<SystemStrength>(system).ValueRW.Value = 1f;
            });
            _entityManager.GetComponentDataRW<SystemStrength>(_systems[0]).ValueRW.Value = 0.25f;
            _initComplete = true;
        }

        public void Update()
        {
            if (!_initComplete)
            {
                if (_entityManager.CreateEntityQuery(typeof(VisualizedRaycast)).CalculateEntityCount() > 0)
                    InitScene();
                return;
            }

            SystemConfig config = _entityManager.GetComponentData<SystemConfig>(_config);

            UpdateHelperTexts(config);

            if (config.LoadNextScene)
            {
                _sceneManagementQuery.TryGetSingletonEntity<SceneReference>(out Entity sceneHolderEntity);
                if (sceneHolderEntity != Entity.Null)
                {
                    _sceneIndex++;
                    EntitySceneReference scene = _entityManager.GetComponentData<SceneReference>(sceneHolderEntity).Value;

                    SceneSystem.LoadSceneAsync(World.DefaultGameObjectInjectionWorld.Unmanaged, scene);
                    _entityManager.DestroyEntity(sceneHolderEntity);
                    if (_helperIndex == (int)TutorialState.NearInteractibleSystem
                        || _helperIndex == (int)TutorialState.CopyPaste
                        || _sceneIndex == 4
                        || _sceneIndex == 5)
                        UpdateHelperTexts(config, true);
                    config.LoadNextScene = false;
                    _entityManager.SetComponentData(_config, config);
                }
            }

            if (_helperIndex == (int)TutorialState.LoreTransitionSpawn || _helperIndex == (int)TutorialState.GatePowered)
            {
                _timerText += Time.deltaTime;
                if (_timerText > 4f)
                    UpdateHelperTexts(config, true);
            }

            if (config.InteractibleSystem != Entity.Null || _currentSystem != Entity.Null)
                UpdateCurrentSystemInteraction(ref config);

            if (config.InteractibleSystem != Entity.Null)
            {
                if (_helperIndex >= (int)TutorialState.LoreTransitionSpawn && Input.GetKeyDown(KeyCode.F))
                    ChangePopupVisibility(!_systemPopupActive);
            }
            else if (_systemPopupActive)
                ChangePopupVisibility(false);
        }

        public void ChangePopupVisibility(bool state)
        {
            PopupController.gameObject.SetActive(state);
            _systemPopupActive = state;
            _entityManager.SetComponentData(_config, new PlayerInputEnable() { State = !state });
            Cursor.visible = state;

            if (state)
            {
                PopupController.UpdateText($"{(char)('A' + (int)_currentSystemType - 1)}");
                PopupController.UpdateSlider(_entityManager.GetComponentDataRW<SystemStrength>(_systems[(int)_currentSystemType - 1]).ValueRO.Value);
            }
        }

        public void OnSliderChange(float value)
        {
            _entityManager.GetComponentDataRW<SystemStrength>(_systems[(int)_currentSystemType - 1]).ValueRW.Value = value;
        }

        //TODO event based
        private void UpdateHelperTexts(SystemConfig config, bool forceNext = false)
        {
            if (_helperIndex == (int)TutorialState.None)
            {
                if (config.InteractibleSystem != Entity.Null)
                    _helperIndex++;
            }
            else if (forceNext)
                _helperIndex++;
            else
                return;
            Helper.text = HelperTexts.HelperTexts[math.clamp(_helperIndex, 0, 3)];
            InfoClippy.text = InfoClippyTexts.HelperTexts[_helperIndex];
        }

        //Only called when the system is interactible
        private void UpdateCurrentSystemInteraction(ref SystemConfig systemConfig)
        {
            if (_currentSystem == Entity.Null)
                _currentSystem = systemConfig.InteractibleSystem;

            InteractibleSystemData interactibleSystem = _entityManager.GetComponentData<InteractibleSystemData>(_currentSystem);

            _currentSystemType = (InteractibleSystemType)interactibleSystem.SystemPropertyModifiderIndex;
            if (systemConfig.InteractibleSystem == Entity.Null)
            {
                _currentSystem = Entity.Null;
                ChangePopupVisibility(false);
            }
        }
    }

    public enum TutorialState
    {
        None = 0,
        NearInteractibleSystem = 1,
        LoreTransitionSpawn = 2,
        CopyPaste = 3,
        Emitter = 4,
        Cross = 5,
        GatePowered = 6,
        Final = 7,
    }
}

using System;
using UnityEngine;
using MortarGame.Config;
using MortarGame.Gameplay;
using MortarGame.Enemies;
using MortarGame.Effects;

namespace MortarGame.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Config")]
        [Tooltip("Preset JSON filename located under StreamingAssets")] 
        public string presetFileName = "preset_M3_RutLui_121946.json";

        public MissionConfig Config { get; private set; }

        [Header("State")]
        public int baseHP;
        public float timeRemaining;
        public bool isRunning;
        public int score = 0;
        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnScoreDelta;

        public EnemyManager enemyManager;
        public AmmoManager ammoManager;

        [Header("Attempted Ammo Tracking")]
        [Tooltip("Stores the last ammo type the player attempted to fire so quiz rewards can match it.")]
        public AmmoType lastAttemptedAmmoType = AmmoType.HE;

        [Header("Score Popups")]
        [Tooltip("World-space score popup color (e.g., +10)")]
        public Color scorePopupColor = new Color(1f, 0.95f, 0.15f);
        [Tooltip("World-space score popup lifetime (seconds)")]
        public float scorePopupLifetime = 1.3f;
        [Tooltip("World-space score popup rise distance (meters)")]
        public float scorePopupRiseDistance = 1.5f;

        public event Action OnWin;
        public event Action OnLose;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadConfig();
            InitializeState();
        }

        private void LoadConfig()
        {
            Config = MissionConfig.LoadFromStreamingAssets(presetFileName);
            if (Config == null)
            {
                Debug.LogError("GameManager: Missing config, using fallback defaults.");
                Config = new MissionConfig
                {
                    missionId = "M3_RutLui_121946",
                    timerSec = 480,
                    baseHP = 4,
                    mortar = new MissionConfig.MortarSettings
                    {
                        rangeMaxM = 40f, reloadSec = 2f, coarseStepM = 20f, fineStepM = 5f,
                        baseSpreadM = 2.5f, spotterSpreadM = 1.0f, flightTimeMin = 1.2f, flightTimeMax = 2.4f
                    },
                    streakRewards = new MissionConfig.StreakRewards { smokeAt = 0, hePlusAt = 3 },
                    enemy = new MissionConfig.EnemySettings { patrolSpeedMps = 1.7f, speedBuffOnWrongAnswer = 0.1f, speedBuffDurationSec = 10f },
                    waves = new[] { "W0", "W1", "W2", "W3", "W4", "Final" },
                    scenarioMode = "Static",
                    staticScenario = new MissionConfig.StaticScenarioSettings
                    {
                        minRandomCount = 1,
                        maxRandomCount = 3,
                        randomRadius = 10f,
                        pointsPerRandom = 10,
                        pointsPerObjective = 50,
                        pointsForFinalBase = 200,
                        objectiveCount = 3,
                        tanksPerObjective = 1,
                        distanceMinFactor = 0.6f,
                        distanceMaxFactor = 0.9f,
                        angleSpreadDeg = 25f,
                        secondsBetweenObjectives = 10f,
                        randomMeshResourceNames = null,
                        randomPrefabResourceNames = null
                    }
                };
            }
        }

        private void InitializeState()
        {
            baseHP = Config.baseHP;
            timeRemaining = Config.timerSec;
            isRunning = true;
            score = 0;

            if (ammoManager == null)
            {
                ammoManager = FindObjectOfType<AmmoManager>();
                if (ammoManager == null)
                {
                    var go = new GameObject("AmmoManager");
                    ammoManager = go.AddComponent<AmmoManager>();
                }
            }

            if (enemyManager == null)
            {
                enemyManager = FindObjectOfType<EnemyManager>();
                if (enemyManager == null)
                {
                    var go = new GameObject("EnemyManager");
                    enemyManager = go.AddComponent<EnemyManager>();
                }
            }

            // Scenario selection: disable waves and run static scenario if configured
            if (Config != null && !string.IsNullOrEmpty(Config.scenarioMode) && Config.scenarioMode.Equals("Static", StringComparison.OrdinalIgnoreCase))
            {
                var wm = FindObjectOfType<MortarGame.Waves.WaveManager>();
                if (wm) wm.enabled = false;

                var director = FindObjectOfType<MortarGame.Gameplay.StaticScenarioDirector>();
                if (director == null)
                {
                    var directorGO = new GameObject("StaticScenarioDirector");
                    director = directorGO.AddComponent<MortarGame.Gameplay.StaticScenarioDirector>();
                }

                // Apply config to director if available
                var ss = Config.staticScenario;
                if (ss != null && director != null)
                {
                    director.objectiveCount = ss.objectiveCount;
                    director.tanksPerObjective = ss.tanksPerObjective;
                    director.distanceMinFactor = ss.distanceMinFactor;
                    director.distanceMaxFactor = ss.distanceMaxFactor;
                    director.angleSpreadDeg = ss.angleSpreadDeg;
                    director.secondsBetweenObjectives = ss.secondsBetweenObjectives;

                    director.minRandomCount = ss.minRandomCount;
                    director.maxRandomCount = ss.maxRandomCount;
                    director.randomRadius = ss.randomRadius;
                    director.pointsPerRandom = ss.pointsPerRandom;
                    director.pointsPerObjective = ss.pointsPerObjective;
                    director.pointsForFinalBase = ss.pointsForFinalBase;
                    director.randomPointsMin = ss.pointsRandomMin;
                    director.randomPointsMax = ss.pointsRandomMax;

                    // Load prefabs and meshes from Resources if names provided
                    if (ss.randomPrefabResourceNames != null && ss.randomPrefabResourceNames.Length > 0)
                    {
                        var list = new System.Collections.Generic.List<GameObject>();
                        var nameToPrefab = new System.Collections.Generic.Dictionary<string, GameObject>();
                        foreach (var name in ss.randomPrefabResourceNames)
                        {
                            if (string.IsNullOrEmpty(name)) continue;
                            var p = Resources.Load<GameObject>(name);
                            if (p != null)
                            {
                                list.Add(p);
                                nameToPrefab[name] = p;
                            }
                            else Debug.LogWarning($"StaticScenario: Prefab not found in Resources: {name}");
                        }
                        director.randomPrefabs = list.ToArray();
                        // Apply per-prefab points ranges if configured
                        if (ss.prefabPointsRanges != null)
                        {
                            foreach (var pr in ss.prefabPointsRanges)
                            {
                                if (pr == null || string.IsNullOrEmpty(pr.resourceName)) continue;
                                if (nameToPrefab.TryGetValue(pr.resourceName, out var pf))
                                {
                                    director.SetPrefabPointsRange(pf, pr.min, pr.max);
                                }
                                else
                                {
                                    Debug.LogWarning($"StaticScenario: Points range specified for unknown prefab resource: {pr.resourceName}");
                                }
                            }
                        }
                    }
                    if (ss.randomMeshResourceNames != null && ss.randomMeshResourceNames.Length > 0)
                    {
                        var listM = new System.Collections.Generic.List<Mesh>();
                        var nameToMesh = new System.Collections.Generic.Dictionary<string, Mesh>();
                        foreach (var name in ss.randomMeshResourceNames)
                        {
                            if (string.IsNullOrEmpty(name)) continue;
                            var m = Resources.Load<Mesh>(name);
                            if (m != null)
                            {
                                listM.Add(m);
                                nameToMesh[name] = m;
                            }
                            else Debug.LogWarning($"StaticScenario: Mesh not found in Resources: {name}");
                        }
                        director.randomMeshes = listM.ToArray();
                        // Apply per-mesh points ranges if configured
                        if (ss.meshPointsRanges != null)
                        {
                            foreach (var mr in ss.meshPointsRanges)
                            {
                                if (mr == null || string.IsNullOrEmpty(mr.resourceName)) continue;
                                if (nameToMesh.TryGetValue(mr.resourceName, out var mesh))
                                {
                                    director.SetMeshPointsRange(mesh, mr.min, mr.max);
                                }
                                else
                                {
                                    Debug.LogWarning($"StaticScenario: Points range specified for unknown mesh resource: {mr.resourceName}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (!isRunning) return;
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                EvaluateEndConditionsOnTimeout();
            }
        }

        public void DamageBase(int amount)
        {
            baseHP -= amount;
            if (baseHP <= 0)
            {
                baseHP = 0;
                Lose();
            }
        }

        public void Win()
        {
            if (!isRunning) return;
            isRunning = false;
            OnWin?.Invoke();
            Debug.Log("WIN: Withdrawal route secured.");
        }

        public void Lose()
        {
            if (!isRunning) return;
            isRunning = false;
            OnLose?.Invoke();
            Debug.Log("LOSE: Base falls.");
        }

        private void EvaluateEndConditionsOnTimeout()
        {
            // Placeholder: In final implementation, check priority targets remaining
            // For now, we assume if base not dead, it's a win on timeout
            if (baseHP > 0)
                Win();
            else
                Lose();
        }

        public void AddScore(int points)
        {
            score += points;
            OnScoreChanged?.Invoke(score);
            OnScoreDelta?.Invoke(points);
            Debug.Log($"Score +{points} => {score}");
        }

        public void AddScoreAtWorldPosition(int points, Vector3 worldPos)
        {
            // Update score and HUD
            AddScore(points);
            // Spawn world-space popup
            FloatingScorePopup.Spawn(worldPos, points, scorePopupColor, scorePopupLifetime, scorePopupRiseDistance);
        }

    }
}
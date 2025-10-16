using System;
using UnityEngine;
using MortarGame.Config;
using MortarGame.Gameplay;
using MortarGame.Enemies;

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

        public EnemyManager enemyManager;
        public AmmoManager ammoManager;

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
                    timerSec = 420,
                    baseHP = 4,
                    mortar = new MissionConfig.MortarSettings
                    {
                        rangeMaxM = 40f, reloadSec = 2f, coarseStepM = 20f, fineStepM = 5f,
                        baseSpreadM = 2.5f, spotterSpreadM = 1.0f, flightTimeMin = 1.2f, flightTimeMax = 2.4f
                    },
                    streakRewards = new MissionConfig.StreakRewards { smokeAt = 3, hePlusAt = 5 },
                    enemy = new MissionConfig.EnemySettings { patrolSpeedMps = 1.7f, speedBuffOnWrongAnswer = 0.1f, speedBuffDurationSec = 10f },
                    waves = new[] { "W0", "W1", "W2", "W3", "W4", "Final" }
                };
            }
        }

        private void InitializeState()
        {
            baseHP = Config.baseHP;
            timeRemaining = Config.timerSec;
            isRunning = true;

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
    }
}
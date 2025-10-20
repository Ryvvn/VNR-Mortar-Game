using System;
using System.IO;
using UnityEngine;

namespace MortarGame.Config
{
    [Serializable]
    public class MissionConfig
    {
        public string missionId;
        public int timerSec;
        public int baseHP;
        public MortarSettings mortar;
        public StreakRewards streakRewards;
        public EnemySettings enemy;
        public string[] waves;
        // Scenario mode: "Waves" (default) or "Static"
        public string scenarioMode = "Waves";

        // Optional: settings for Static scenario mode, including random meshes/prefabs and scoring
        public StaticScenarioSettings staticScenario;
        
        [Serializable]
        public class MortarSettings
        {
            public float rangeMaxM;
            public float reloadSec;
            public float coarseStepM;
            public float fineStepM;
            public float baseSpreadM;
            public float spotterSpreadM;
            public float flightTimeMin;
            public float flightTimeMax;
        }

        [Serializable]
        public class StreakRewards
        {
            public int smokeAt;
            public int hePlusAt;
        }

        [Serializable]
        public class EnemySettings
        {
            public float patrolSpeedMps;
            public float speedBuffOnWrongAnswer;
            public float speedBuffDurationSec;
        }

        [Serializable]
        public class StaticScenarioSettings
        {
            // Resource names under a Resources/ folder
            public string[] randomMeshResourceNames;
            public string[] randomPrefabResourceNames;
            // Optional per-resource point ranges (use resourceName paths like "Props/Crate")
            [Serializable]
            public class PointsRange { public string resourceName; public int min; public int max; }
            public PointsRange[] prefabPointsRanges;
            public PointsRange[] meshPointsRanges;
            // spawn counts and placement
            public int minRandomCount = 1;
            public int maxRandomCount = 3;
            public float randomRadius = 10f;
            // scoring
            public int pointsPerRandom = 10;
            public int pointsRandomMin = 0;
            public int pointsRandomMax = 0;
            public int pointsPerObjective = 50;
            public int pointsForFinalBase = 200;
            // pacing and placement
            public int objectiveCount = 3;
            public int tanksPerObjective = 1;
            public float distanceMinFactor = 0.6f;
            public float distanceMaxFactor = 0.9f;
            public float angleSpreadDeg = 25f;
            public float secondsBetweenObjectives = 10f;
        }

        public static MissionConfig LoadFromStreamingAssets(string fileName)
        {
            try
            {
                var path = Path.Combine(Application.streamingAssetsPath, fileName);
                if (!File.Exists(path))
                {
                    Debug.LogError($"MissionConfig file not found: {path}");
                    return null;
                }
                var json = File.ReadAllText(path);
                var config = JsonUtility.FromJson<MissionConfig>(json);
                return config;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load MissionConfig: {ex}");
                return null;
            }
        }
    }
}
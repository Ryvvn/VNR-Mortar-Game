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
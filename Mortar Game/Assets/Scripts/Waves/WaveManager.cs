using System.Collections;
using UnityEngine;
using MortarGame.Core;

namespace MortarGame.Waves
{
    public class WaveManager : MonoBehaviour
    {
        public Transform leftLaneSpawn;
        public Transform rightLaneSpawn;
        public GameObject enemyPrefab;
        public GameObject staticTargetPrefab;
        public Transform centerLaneSpawn;
        public GameObject tankPrefab;
        [Tooltip("Tank HP override. If <=0, uses prefab's value")] public float tankMaxHP = 240f;
        public int currentWave = 0;
        [Tooltip("Seconds until next wave; 0 while a wave is active")] public float nextWaveCountdown = 0f;

        private void Start()
        {
            StartCoroutine(RunTimeline());
        }

        private IEnumerator RunTimeline()
        {
            // Scale wave gaps so total timeline matches mission timer
            float dtScale = 1f;
            if (MortarGame.Core.GameManager.Instance != null && MortarGame.Core.GameManager.Instance.Config != null)
            {
                dtScale = MortarGame.Core.GameManager.Instance.Config.timerSec / 480f; // base design total = 480s
            }
            
            // W0 (06:30–06:00): Tutorial + 1 sample quiz, demo shot at Static Target #1.
            SpawnStaticTarget(leftLaneSpawn);
            yield return WaitWithCountdown(30f * dtScale);
            if (MortarGame.Core.GameManager.Instance == null || !MortarGame.Core.GameManager.Instance.isRunning) yield break;
            
            // W1 (06:00–05:00): 2 semi-mobile squads on left lane
            currentWave = 1;
            SpawnSquad(leftLaneSpawn, 2);
            yield return WaitWithCountdown(60f * dtScale);
            if (MortarGame.Core.GameManager.Instance == null || !MortarGame.Core.GameManager.Instance.isRunning) yield break;
            
            // W2: 1 Static Target #2 (obstacle), 2 squads on right lane
            currentWave = 2;
            SpawnStaticTarget(rightLaneSpawn);
            SpawnSquad(rightLaneSpawn, 2);
            yield return WaitWithCountdown(60f * dtScale);
            if (MortarGame.Core.GameManager.Instance == null || !MortarGame.Core.GameManager.Instance.isRunning) yield break;
            
            // W3: 3 squads split into two flanks
            currentWave = 3;
            SpawnSquad(leftLaneSpawn, 2);
            SpawnSquad(rightLaneSpawn, 1);
            yield return WaitWithCountdown(60f * dtScale);
            if (MortarGame.Core.GameManager.Instance == null || !MortarGame.Core.GameManager.Instance.isRunning) yield break;
            
            // W4: Pace +10%; adjust enemy base speed globally (placeholder via prefab tweak)
            currentWave = 4;
            AdjustEnemyBaseSpeed(1.1f);
            SpawnSquad(leftLaneSpawn, 2);
            yield return WaitWithCountdown(90f * dtScale);
            if (MortarGame.Core.GameManager.Instance == null || !MortarGame.Core.GameManager.Instance.isRunning) yield break;
            
            // W5: Heavy armor (tanks) entering on two lanes
            currentWave = 5;
            SpawnTank(leftLaneSpawn);
            SpawnTank(rightLaneSpawn);
            yield return WaitWithCountdown(90f * dtScale);
            if (MortarGame.Core.GameManager.Instance == null || !MortarGame.Core.GameManager.Instance.isRunning) yield break;
            
            // Final: Light truck + 1 static MG nest (placeholder: spawn a stronger enemy and a static target)
            currentWave = 6;
            SpawnStaticTarget(leftLaneSpawn);
            SpawnSquad(leftLaneSpawn, 1);
            yield return WaitWithCountdown(90f * dtScale);
            if (MortarGame.Core.GameManager.Instance == null || !MortarGame.Core.GameManager.Instance.isRunning) yield break;
        }

        private void SpawnSquad(Transform spawn, int count)
        {
            if (!enemyPrefab || !spawn) return;
            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(enemyPrefab, spawn.position + new Vector3(i * 1.5f, 0, 0), spawn.rotation);
                var ec = go.GetComponent<MortarGame.Enemies.EnemyController>();
                if (ec) ec.baseSpeed = GameManager.Instance.Config.enemy.patrolSpeedMps;
                GameManager.Instance.enemyManager.Register(ec);
            }
        }

        private void SpawnStaticTarget(Transform spawn)
        {
            if (!staticTargetPrefab || !spawn) return;
            Instantiate(staticTargetPrefab, spawn.position, spawn.rotation);
        }

        private void SpawnTank(Transform spawn)
        {
            if (!tankPrefab || !spawn) return;
            var go = Instantiate(tankPrefab, spawn.position, spawn.rotation);
            var ec = go.GetComponent<MortarGame.Enemies.EnemyController>();
            if (ec)
            {
                ec.baseSpeed = GameManager.Instance.Config.enemy.patrolSpeedMps * 0.6f; // tanks move slower
                if (tankMaxHP > 0f)
                {
                    ec.maxHP = tankMaxHP;
                    ec.currentHP = ec.maxHP;
                }
            }
            GameManager.Instance.enemyManager.Register(ec);
        }

        private void AdjustEnemyBaseSpeed(float multiplier)
        {
            // Placeholder: in a full system you would adjust existing enemies' baseSpeed
        }

        private IEnumerator WaitWithCountdown(float seconds)
        {
            nextWaveCountdown = seconds;
            while (nextWaveCountdown > 0f)
            {
                // Stop countdown if the game has ended
                if (MortarGame.Core.GameManager.Instance == null || !MortarGame.Core.GameManager.Instance.isRunning)
                {
                    yield break;
                }
                nextWaveCountdown -= Time.deltaTime;
                yield return null;
            }
            nextWaveCountdown = 0f;
        }
    }
}
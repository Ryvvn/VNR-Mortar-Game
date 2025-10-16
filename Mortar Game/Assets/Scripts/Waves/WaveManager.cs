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

        private void Start()
        {
            StartCoroutine(RunTimeline());
        }

        private IEnumerator RunTimeline()
        {
            // W0 (06:30–06:00): Tutorial + 1 sample quiz, demo shot at Static Target #1.
            SpawnStaticTarget(leftLaneSpawn);
            yield return new WaitForSeconds(30f);

            // W1 (06:00–05:00): 2 semi-mobile squads on left lane
            SpawnSquad(leftLaneSpawn, 2);
            yield return new WaitForSeconds(60f);

            // W2: 1 Static Target #2 (obstacle), 2 squads on right lane
            SpawnStaticTarget(rightLaneSpawn);
            SpawnSquad(rightLaneSpawn, 2);
            yield return new WaitForSeconds(60f);

            // W3: 3 squads split into two flanks
            SpawnSquad(leftLaneSpawn, 2);
            SpawnSquad(rightLaneSpawn, 1);
            yield return new WaitForSeconds(60f);

            // W4: Pace +10%; adjust enemy base speed globally (placeholder via prefab tweak)
            AdjustEnemyBaseSpeed(1.1f);
            SpawnSquad(leftLaneSpawn, 2);
            yield return new WaitForSeconds(90f);

            // Final: Light truck + 1 static MG nest (placeholder: spawn a stronger enemy and a static target)
            SpawnStaticTarget(leftLaneSpawn);
            SpawnSquad(leftLaneSpawn, 1);
            yield return new WaitForSeconds(90f);
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

        private void AdjustEnemyBaseSpeed(float multiplier)
        {
            // Placeholder: in a full system you would adjust existing enemies' baseSpeed
        }
    }
}
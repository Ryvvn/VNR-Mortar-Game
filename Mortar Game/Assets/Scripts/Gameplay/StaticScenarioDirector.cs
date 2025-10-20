using UnityEngine;
using System.Collections;
using MortarGame.Core;
using MortarGame.Enemies;
using MortarGame.Targets;

namespace MortarGame.Gameplay
{
    public class StaticScenarioDirector : MonoBehaviour
    {
        [Header("Prefabs (optional; will auto-fill from WaveManager if null)")]
        public GameObject staticTargetPrefab;
        public GameObject tankPrefab;

        [Header("Random Props Catalog")]
        [Tooltip("Drag prefabs or mesh-based objects here to be spawned randomly")]
        public GameObject[] randomPrefabs;
        [Tooltip("Min and max number of random props per objective")]
        public int minRandomCount = 1;
        public int maxRandomCount = 3;
        [Tooltip("Points per random prop destroyed")]
        public int pointsPerRandom = 10;
        [Tooltip("Placement radius around the objective for random props")]
        public float randomRadius = 10f;

        [Header("Random Mesh Catalog")]
        [Tooltip("Alternatively, provide Mesh assets to spawn generic destructible prop objects")]
        public Mesh[] randomMeshes;
        [Tooltip("Material to use for spawned meshes (optional; falls back to staticTargetPrefab's material)")]
        public Material randomMeshMaterial;

        [Header("Scenario Settings")]
        [Tooltip("Number of static objectives before spawning the enemy base")] public int objectiveCount = 3;
        [Tooltip("Static tanks spawned per objective as cover/decoys")] public int tanksPerObjective = 1;
        [Tooltip("Minimum fraction of mortar range for the first objective (0-1)")] public float distanceMinFactor = 0.6f;
        [Tooltip("Maximum fraction of mortar range for the last objective (0-1)")] public float distanceMaxFactor = 0.9f;
        [Tooltip("Angle spread for placement left/right around the mortar forward (degrees)")] public float angleSpreadDeg = 25f;
        [Tooltip("Gap between objectives (seconds), scales with mission timer")] public float secondsBetweenObjectives = 10f;

        [Header("Scoring")]
        public int pointsPerObjective = 50;
        public int pointsForFinalBase = 200;
        [Tooltip("If set, each spawned random prop will get a random point value in this range")] public int randomPointsMin = 0;
        public int randomPointsMax = 0;


        private Transform _mortarTransform;
        private MortarGame.Targets.Target _currentTarget;
        private MortarGame.Targets.Target _baseTarget;

        private IEnumerator Start()
        {
            // Acquire prefabs from WaveManager if not assigned
            var wm = FindObjectOfType<MortarGame.Waves.WaveManager>();
            if (wm != null)
            {
                if (!staticTargetPrefab) staticTargetPrefab = wm.staticTargetPrefab;
                if (!tankPrefab) tankPrefab = wm.tankPrefab;
            }

            var mc = FindObjectOfType<MortarGame.Weapons.MortarController>();
            _mortarTransform = mc ? mc.transform : transform;

            yield return RunScenario();
        }

        private IEnumerator RunScenario()
        {
            var gm = MortarGame.Core.GameManager.Instance;
            if (gm == null || !gm.isRunning) yield break;

            float dtScale = gm.Config != null ? (gm.Config.timerSec / 480f) : 1f;

            // Spawn sequential static objectives
            for (int i = 0; i < objectiveCount && gm.isRunning; i++)
            {
                float tFactor = Mathf.Lerp(distanceMinFactor, distanceMaxFactor, objectiveCount > 1 ? (float)i / (objectiveCount - 1) : 0f);
                Vector3 pos = ComputeSpawnPos(tFactor, i);

                SpawnObjectiveAt(pos);
                SpawnRandomPropsAround(pos);

                // Wait until objective is destroyed
                yield return new WaitUntil(() => _currentTarget == null || _currentTarget.currentHP <= 0f);

                // Small pacing gap
                float gap = Mathf.Max(0f, secondsBetweenObjectives * dtScale);
                float t = 0f;
                while (t < gap && gm.isRunning)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }

            // Final: spawn the enemy base (strong static target) within range
            Vector3 basePos = ComputeSpawnPos(Mathf.Clamp(distanceMaxFactor + 0.05f, 0.1f, 0.98f), 999);
            SpawnFinalBaseAt(basePos);
            SpawnRandomPropsAround(basePos);

            bool baseDown = false;
            if (_baseTarget != null)
            {
                _baseTarget.OnDestroyed += () => { baseDown = true; };
                while (!baseDown && gm.isRunning)
                    yield return null;
            }

            if (gm.isRunning)
            {
                // award points for final base with world-space popup
                gm.Win();
            }
        }

        private void SpawnObjectiveAt(Vector3 pos)
        {
            var gm = MortarGame.Core.GameManager.Instance;
            if (gm == null || staticTargetPrefab == null) return;

            var go = Instantiate(staticTargetPrefab, pos, Quaternion.identity);
            _currentTarget = go.GetComponent<MortarGame.Targets.Target>();
        }

        private void SpawnFinalBaseAt(Vector3 pos)
        {
            var gm = MortarGame.Core.GameManager.Instance;
            if (gm == null || staticTargetPrefab == null) return;

            var go = Instantiate(staticTargetPrefab, pos, Quaternion.identity);
            var target = go.GetComponent<MortarGame.Targets.Target>();
            if (target)
            {
                target.maxHP *= 2f; // tougher base
                target.currentHP = target.maxHP;
                _baseTarget = target;
            }

            // Add a couple static tanks guarding the base
            for (int k = 0; k < 2; k++)
            {
                Vector3 offset = Quaternion.Euler(0f, (k * 40f) - 20f, 0f) * Vector3.forward * 10f;
                var tank = Instantiate(tankPrefab, pos + offset, Quaternion.identity);
                var ec = tank ? tank.GetComponent<MortarGame.Enemies.EnemyController>() : null;
                if (ec)
                {
                    ec.baseSpeed = 0f;
                    MortarGame.Core.GameManager.Instance.enemyManager.Register(ec);
                }
            }
        }

        private void SpawnRandomPropsAround(Vector3 center)
        {
            int prefabCount = randomPrefabs != null ? randomPrefabs.Length : 0;
            int meshCount = randomMeshes != null ? randomMeshes.Length : 0;
            if (prefabCount == 0 && meshCount == 0) return;

            int count = UnityEngine.Random.Range(minRandomCount, Mathf.Max(minRandomCount, maxRandomCount + 1));
            for (int i = 0; i < count; i++)
            {
                Vector2 ring = UnityEngine.Random.insideUnitCircle.normalized * (randomRadius * UnityEngine.Random.Range(0.5f, 1f));
                Vector3 pos = center + new Vector3(ring.x, 0f, ring.y);
                Quaternion rot = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

                GameObject go = null;
                GameObject usedPrefab = null;
                Mesh usedMesh = null;
                int pick = UnityEngine.Random.Range(0, prefabCount + meshCount);
                if (pick < prefabCount)
                {
                    var prefab = randomPrefabs[pick];
                    if (!prefab) continue;
                    usedPrefab = prefab;
                    go = Instantiate(prefab, pos, rot);
                }
                else
                {
                    var mesh = randomMeshes[pick - prefabCount];
                    if (!mesh) continue;
                    usedMesh = mesh;
                    go = new GameObject("PropMesh");
                    go.transform.SetPositionAndRotation(pos, rot);
                    var mf = go.AddComponent<MeshFilter>();
                    mf.sharedMesh = mesh;
                    var mr = go.AddComponent<MeshRenderer>();
                    if (randomMeshMaterial != null)
                    {
                        mr.sharedMaterial = randomMeshMaterial;
                    }
                    else if (staticTargetPrefab)
                    {
                        var mr2 = staticTargetPrefab.GetComponentInChildren<MeshRenderer>();
                        if (mr2) mr.sharedMaterial = mr2.sharedMaterial;
                    }
                    var bc = go.AddComponent<BoxCollider>();
                    bc.center = mesh.bounds.center;
                    bc.size = mesh.bounds.size;
                }

                if (!go) continue;

                var pr = go.GetComponent<PointsReceiver>();
                if (pr == null) pr = go.AddComponent<PointsReceiver>();

                int perSpawnPoints = pointsPerRandom;
                Vector2Int range;
                if (usedPrefab != null && _prefabPointsRanges.TryGetValue(usedPrefab, out range) && range.y >= range.x && range.y > 0)
                {
                    perSpawnPoints = UnityEngine.Random.Range(range.x, range.y + 1);
                }
                else if (usedMesh != null && _meshPointsRanges.TryGetValue(usedMesh, out range) && range.y >= range.x && range.y > 0)
                {
                    perSpawnPoints = UnityEngine.Random.Range(range.x, range.y + 1);
                }
                else if (randomPointsMax >= randomPointsMin && randomPointsMax > 0)
                {
                    perSpawnPoints = UnityEngine.Random.Range(randomPointsMin, randomPointsMax + 1);
                }
                pr.pointsOnDestroyed = perSpawnPoints;
                pr.OnDestroyed += () => { var gm = MortarGame.Core.GameManager.Instance; if (gm) gm.AddScoreAtWorldPosition(pr.pointsOnDestroyed, pr.transform.position); };

                var ec = go.GetComponent<MortarGame.Enemies.EnemyController>();
                if (ec) ec.baseSpeed = 0f;
            }
        }

        private Vector3 ComputeSpawnPos(float distanceFactor, int index)
        {
            var gm = MortarGame.Core.GameManager.Instance;
            float rangeMax = gm != null && gm.Config != null ? gm.Config.mortar.rangeMaxM : 100f;
            float dist = Mathf.Clamp(rangeMax * distanceFactor, 10f, rangeMax * 0.98f);

            // cycle angles: left, center, right
            float angleSel = (index % 3);
            float angle = angleSel == 0 ? -angleSpreadDeg : (angleSel == 1 ? 0f : angleSpreadDeg);
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 dir = rot * (_mortarTransform ? _mortarTransform.forward : Vector3.forward);
            return (_mortarTransform ? _mortarTransform.position : Vector3.zero) + dir.normalized * dist;
        }
        private System.Collections.Generic.Dictionary<GameObject, Vector2Int> _prefabPointsRanges = new System.Collections.Generic.Dictionary<GameObject, Vector2Int>();
        private System.Collections.Generic.Dictionary<Mesh, Vector2Int> _meshPointsRanges = new System.Collections.Generic.Dictionary<Mesh, Vector2Int>();
        public void SetPrefabPointsRange(GameObject prefab, int min, int max)
        {
            if (prefab == null) return;
            _prefabPointsRanges[prefab] = new Vector2Int(min, max);
        }
        public void SetMeshPointsRange(Mesh mesh, int min, int max)
        {
            if (mesh == null) return;
            _meshPointsRanges[mesh] = new Vector2Int(min, max);
        }
    }
}
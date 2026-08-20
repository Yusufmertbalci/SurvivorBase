using System.Collections.Generic;
using UnityEngine;
using Game.Difficulty; // DifficultyManager

namespace Game.Enemies
{
    /// <summary>
    /// Enemy spawner: decides WHEN and WHERE to spawn and instantiates enemy prefabs. When a
    /// DifficultyManager is present it drives the current spawn interval, active-enemy cap, and the
    /// HP/damage multipliers applied to each newly spawned enemy; otherwise it falls back to its own
    /// serialized values. Spawn placement and enemy-count tracking are unchanged. No pooling/waves.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Transform enemies will chase. Assign the Player here.")]
        [SerializeField] private Transform target;

        [Tooltip("The enemy prefab to spawn. Must have an EnemyChase component.")]
        [SerializeField] private GameObject enemyPrefab;

        [Header("Spawn Timing (fallback if no DifficultyManager)")]
        [Tooltip("Seconds between spawn attempts. Used only when no DifficultyManager is in the scene.")]
        [SerializeField] private float spawnInterval = 2f;

        [Header("Spawn Placement (around the target, on the X/Z plane)")]
        [Tooltip("Enemies never spawn closer than this to the target.")]
        [SerializeField] private float minSpawnDistance = 8f;

        [Tooltip("Enemies never spawn farther than this from the target.")]
        [SerializeField] private float maxSpawnDistance = 12f;

        [Header("Limits (fallback if no DifficultyManager)")]
        [Tooltip("Max enemies alive at once. Used only when no DifficultyManager is in the scene.")]
        [SerializeField] private int maxActiveEnemies = 30;

        // Tracks spawned enemies so the active count stays accurate even if some are destroyed later.
        private readonly List<GameObject> _activeEnemies = new List<GameObject>();

        private float _timer;

        private void Update()
        {
            // Frame-rate independent timing. Time.deltaTime is 0 during the upgrade pause, so
            // spawning naturally pauses too.
            _timer += Time.deltaTime;
            if (_timer < CurrentSpawnInterval)
                return;

            _timer = 0f;
            TrySpawnEnemy();
        }

        // Current spawn interval from the DifficultyManager, or the serialized fallback if absent.
        private float CurrentSpawnInterval =>
            DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentSpawnInterval : spawnInterval;

        // Current active-enemy cap from the DifficultyManager, or the serialized fallback if absent.
        private int CurrentMaxActiveEnemies =>
            DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentMaxActiveEnemies : maxActiveEnemies;

        private void TrySpawnEnemy()
        {
            if (target == null || enemyPrefab == null)
                return;

            // Drop destroyed enemies (null in Unity) so the active count is accurate.
            _activeEnemies.RemoveAll(enemy => enemy == null);

            if (_activeEnemies.Count >= CurrentMaxActiveEnemies)
                return;

            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            AssignTarget(enemy);
            ApplyDifficulty(enemy);
            _activeEnemies.Add(enemy);
        }

        // Applies the current difficulty HP/damage multipliers ONCE, at spawn, through the enemy's own
        // components (never touching their private fields directly). No per-frame scaling, so nothing
        // compounds. Does nothing if there is no DifficultyManager.
        private void ApplyDifficulty(GameObject enemy)
        {
            if (DifficultyManager.Instance == null)
                return;

            float hpMultiplier = DifficultyManager.Instance.CurrentHpMultiplier;
            float damageMultiplier = DifficultyManager.Instance.CurrentDamageMultiplier;

            if (enemy.TryGetComponent(out EnemyHealth health))
                health.ApplyDifficultyMultiplier(hpMultiplier);

            if (enemy.TryGetComponent(out EnemyAttack attack))
                attack.ApplyDifficultyMultiplier(damageMultiplier);
        }

        private Vector3 GetRandomSpawnPosition()
        {
            // Random point on a ring around the target. Random.Range(min, max) guarantees the
            // distance is never below minSpawnDistance.
            float angle = Random.value * Mathf.PI * 2f;
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

            float x = target.position.x + Mathf.Cos(angle) * distance;
            float z = target.position.z + Mathf.Sin(angle) * distance;

            // Use the prefab's authored height so the enemy rests correctly on the plane,
            // independent of the target's Y.
            float y = enemyPrefab.transform.position.y;

            return new Vector3(x, y, z);
        }

        private void AssignTarget(GameObject enemy)
        {
            EnemyChase chase = enemy.GetComponent<EnemyChase>();
            if (chase == null)
            {
                Debug.LogWarning(
                    $"{nameof(EnemySpawner)}: Spawned prefab has no EnemyChase component; " +
                    "it won't chase the target.", enemy);
                return;
            }

            // Hand the spawned enemy its target via EnemyChase's public setter.
            chase.SetTarget(target);
        }
    }
}
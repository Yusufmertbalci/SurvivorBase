using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Basic enemy spawner: periodically instantiates an enemy prefab at a random point on a ring
    /// around the target (Player), between a minimum and maximum distance, up to a maximum count.
    ///
    /// No object pooling, waves, or difficulty scaling - this is the simple base for a future
    /// spawning system. Does not reference the Main Camera.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Transform enemies will chase. Assign the Player here.")]
        [SerializeField] private Transform target;

        [Tooltip("The enemy prefab to spawn. Must have an EnemyChase component.")]
        [SerializeField] private GameObject enemyPrefab;

        [Header("Spawn Timing")]
        [Tooltip("Seconds between spawn attempts.")]
        [SerializeField] private float spawnInterval = 2f;

        [Header("Spawn Placement (around the target, on the X/Z plane)")]
        [Tooltip("Enemies never spawn closer than this to the target.")]
        [SerializeField] private float minSpawnDistance = 8f;

        [Tooltip("Enemies never spawn farther than this from the target.")]
        [SerializeField] private float maxSpawnDistance = 12f;

        [Header("Limits")]
        [Tooltip("Maximum number of enemies allowed alive at once.")]
        [SerializeField] private int maxActiveEnemies = 30;

        // Tracks spawned enemies so the active count stays accurate even if some are destroyed later.
        private readonly List<GameObject> _activeEnemies = new List<GameObject>();

        private float _timer;

        private void Update()
        {
            // Frame-rate independent interval timing.
            _timer += Time.deltaTime;
            if (_timer < spawnInterval)
                return;

            _timer = 0f;
            TrySpawnEnemy();
        }

        private void TrySpawnEnemy()
        {
            if (target == null || enemyPrefab == null)
                return;

            // Drop destroyed enemies (null in Unity) so the active count is accurate.
            _activeEnemies.RemoveAll(enemy => enemy == null);

            if (_activeEnemies.Count >= maxActiveEnemies)
                return;

            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            AssignTarget(enemy);
            _activeEnemies.Add(enemy);
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
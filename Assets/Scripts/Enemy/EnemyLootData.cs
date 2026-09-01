using UnityEngine;
using Game.Base; // ResourceType (reuse the existing enum)

namespace Game.Enemies
{
    /// <summary>
    /// STATIC per-enemy-type loot configuration (ScriptableObject). Defines whether an enemy can drop
    /// BONUS run-resource loot, the chance, the resource type, and the min/max amount. Data-driven, so
    /// new enemy types (elites, bosses, rarer drops) just use different assets - EnemyHealth doesn't
    /// change. No runtime state lives here.
    ///
    /// Create via: Assets > Create > SurvivorBase > Enemy Loot Data.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyLootData", menuName = "SurvivorBase/Enemy Loot Data")]
    public class EnemyLootData : ScriptableObject
    {
        [Tooltip("If false, this enemy never drops resource loot.")]
        [SerializeField] private bool canDropLoot = true;

        [Tooltip("Chance (0-1) to drop loot on death. 0.3 = 30%.")]
        [Range(0f, 1f)]
        [SerializeField] private float dropChance = 0.3f;

        [Tooltip("Which run resource this enemy drops.")]
        [SerializeField] private ResourceType resourceType = ResourceType.Wood;

        [Tooltip("Minimum amount dropped (inclusive).")]
        [SerializeField] private int minAmount = 5;

        [Tooltip("Maximum amount dropped (inclusive).")]
        [SerializeField] private int maxAmount = 15;

        [Tooltip("Physical pickup prefab spawned when loot drops (must have a ResourceLoot component).")]
        [SerializeField] private GameObject lootPrefab;

        public ResourceType Type => resourceType;
        public GameObject LootPrefab => lootPrefab;

        /// <summary>Rolls whether loot should drop this death (enabled + prefab present + chance).</summary>
        public bool RollShouldDrop()
        {
            return canDropLoot && lootPrefab != null && Random.value <= dropChance;
        }

        /// <summary>Random amount within [minAmount, maxAmount] (inclusive), never below 1.</summary>
        public int RollAmount()
        {
            int min = Mathf.Max(1, Mathf.Min(minAmount, maxAmount));
            int max = Mathf.Max(min, maxAmount);
            return Random.Range(min, max + 1); // +1 because the int Range max is exclusive
        }
    }
}
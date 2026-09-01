using Game.Loot; // ResourceLoot
using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Simple enemy health: takes damage from the player's attacks and destroys the GameObject when
    /// health reaches zero. On death it drops ONE XP crystal at its position; that crystal awards XP
    /// when the player collects it. EnemyHealth no longer awards XP directly - the crystal pickup is
    /// the single reward source, which prevents any double reward.
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        [Tooltip("Starting and maximum health.")]
        [SerializeField] private float maxHealth = 100f;

        [Tooltip("Current health. Set to maxHealth on spawn; shown here so it can be watched at runtime.")]
        [SerializeField] private float currentHealth;

        [Header("Loot")]
        [Tooltip("XP crystal prefab dropped on death. Its own Inspector holds the XP reward values.")]
        [SerializeField] private GameObject xpCrystalPrefab;

        [Tooltip("Optional BONUS run-resource loot config. Leave empty for enemies that drop no resources.")]
        [SerializeField] private EnemyLootData lootData;

        // Guards against Die() running more than once, so exactly ONE crystal drops per enemy.
        private bool isDead;

        // Guards against applying the difficulty multiplier more than once (no exponential stacking).
        private bool _difficultyApplied;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        /// <summary>
        /// Applies a difficulty HP multiplier ONCE at spawn: scales this instance's max health (the
        /// prefab's base value is untouched) and refills to the new max. Guarded so it never stacks.
        /// </summary>
        public void ApplyDifficultyMultiplier(float multiplier)
        {
            if (_difficultyApplied || multiplier <= 0f)
                return;

            _difficultyApplied = true;
            maxHealth *= multiplier;
            currentHealth = maxHealth;
        }

        /// <summary>
        /// Applies damage to this enemy. Non-positive damage is ignored, health never goes below
        /// zero, and reaching zero triggers Die().
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isDead)
                return;

            if (damage <= 0f)
                return;

            currentHealth -= damage;
            if (currentHealth < 0f)
                currentHealth = 0f;

            Debug.Log($"{name} took {damage} damage. Remaining HP: {currentHealth}/{maxHealth}.", this);

            if (currentHealth <= 0f)
                Die();
        }

        /// <summary>
        /// Handles death. Drops exactly one XP crystal (the isDead guard prevents repeats) and then
        /// removes the enemy. XP is NOT awarded here - it is awarded only when the player collects the
        /// crystal, so there is a single reward source and no double reward.
        /// </summary>
        private void Die()
        {
            isDead = true;
            DropXpCrystal();
            DropResourceLoot();
            Destroy(gameObject);
        }

        private void DropXpCrystal()
        {
            if (xpCrystalPrefab == null)
            {
                Debug.LogWarning($"{name}: No XP crystal prefab assigned on EnemyHealth; nothing dropped.", this);
                return;
            }

            Instantiate(xpCrystalPrefab, transform.position, Quaternion.identity);
        }

        /// <summary>
        /// Optional BONUS run-resource loot, independent of the XP crystal. Rolls the enemy's loot
        /// config and, on success, spawns ONE ResourceLoot pickup with a random amount. Collecting it
        /// adds to the RUN inventory (never the base pool). The isDead guard means this runs once, so
        /// loot never drops twice. Enemies with no lootData simply drop nothing here.
        /// </summary>
        private void DropResourceLoot()
        {
            if (lootData == null || !lootData.RollShouldDrop())
                return;

            int amount = lootData.RollAmount();
            if (amount <= 0)
                return;

            // Small horizontal offset so the loot doesn't perfectly overlap the XP crystal.
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            GameObject go = Instantiate(lootData.LootPrefab, transform.position + offset, Quaternion.identity);

            if (go.TryGetComponent(out ResourceLoot loot))
                loot.Initialize(lootData.Type, amount);
            else
                Debug.LogWarning($"{name}: Loot prefab has no ResourceLoot component; loot won't be collectable.", go);
        }
    }
}
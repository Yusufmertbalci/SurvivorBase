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

        // Guards against Die() running more than once, so exactly ONE crystal drops per enemy.
        private bool isDead;

        private void Awake()
        {
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
    }
}
using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Simple enemy health: takes damage from the player's attacks and destroys the GameObject when
    /// health reaches zero. Deliberately minimal - no death effects, XP, loot, or pooling yet.
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        [Tooltip("Starting and maximum health.")]
        [SerializeField] private float maxHealth = 100f;

        [Tooltip("Current health. Set to maxHealth on spawn; shown here so it can be watched at runtime.")]
        [SerializeField] private float currentHealth;

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
        /// Handles death. For now it simply removes the enemy from the scene.
        /// Death effects, XP, and drops will hook in here later.
        /// </summary>
        private void Die()
        {
            Destroy(gameObject);
        }
    }
}
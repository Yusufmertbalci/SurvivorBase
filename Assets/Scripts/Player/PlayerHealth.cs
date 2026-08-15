using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Player health. Takes damage from enemy attacks and clamps at zero.
    /// Player death / Game Over is a separate feature - for now health simply stays at 0.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Tooltip("Starting and maximum health.")]
        [SerializeField] private int maxHealth = 100;

        [Tooltip("Current health. Set to maxHealth on start; shown here so it can be watched at runtime.")]
        [SerializeField] private int currentHealth;

        // Read-only accessors for future use (e.g. a health bar). No behavior of their own.
        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        /// <summary>
        /// Applies damage to the Player. Non-positive damage is ignored and health never goes below
        /// zero. Player death is intentionally not handled yet - health just stays at 0.
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (damage <= 0)
                return;

            currentHealth -= damage;
            if (currentHealth < 0)
                currentHealth = 0;

            Debug.Log($"Player took {damage} damage. Remaining HP: {currentHealth}/{maxHealth}.", this);
        }
    }
}
using System;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Player health. Takes damage from enemy attacks and clamps at zero. When health reaches zero
    /// it enters a dead state and raises the Died event exactly once. It deliberately does NOT
    /// contain any Game Over logic - that is handled by a separate listener (PlayerDeathHandler),
    /// keeping detection and response cleanly separated.
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

        /// <summary>Raised once, the moment the player dies. Listeners handle the run-ending.</summary>
        public event Action Died;

        /// <summary>True once the player has died. Used to ignore any further damage.</summary>
        public bool IsDead => isDead;

        private bool isDead;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        /// <summary>
        /// Run upgrade: increases maximum health by the given amount and heals by the same amount.
        /// Ignored once dead. This is a temporary run upgrade - it resets naturally when the scene
        /// reloads for a new run, because the Player is recreated with its serialized maxHealth.
        /// </summary>
        public void IncreaseMaxHealth(int amount)
        {
            if (amount <= 0 || isDead)
                return;

            maxHealth += amount;
            currentHealth += amount;
            Debug.Log($"[Upgrade] Max HP +{amount}. New max: {maxHealth}, current: {currentHealth}.", this);
        }

        /// <summary>
        /// Applies damage to the Player. Ignored once dead (prevents further damage and repeated
        /// death events) and for non-positive damage. Health never goes below zero.
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (isDead)
                return;

            if (damage <= 0)
                return;

            currentHealth -= damage;
            if (currentHealth < 0)
                currentHealth = 0;

            Debug.Log($"Player took {damage} damage. Remaining HP: {currentHealth}/{maxHealth}.", this);

            if (currentHealth <= 0)
                Die();
        }

        /// <summary>
        /// Marks the player dead and announces it once. Health stays at 0; the actual run-ending
        /// response (stopping the player, showing Game Over) is handled by listeners, not here.
        /// </summary>
        private void Die()
        {
            isDead = true;
            Debug.Log("Player has died.", this);
            Died?.Invoke();
        }
    }
}
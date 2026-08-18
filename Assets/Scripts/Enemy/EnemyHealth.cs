using UnityEngine;
using Game.Progression; // RunProgression / PermanentProgression

namespace Game.Enemies
{
    /// <summary>
    /// Simple enemy health: takes damage from the player's attacks and destroys the GameObject when
    /// health reaches zero. On death it awards XP once to both progression systems (temporary Run XP
    /// and permanent Survivor XP). No death effects, loot, or pooling yet.
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        [Tooltip("Starting and maximum health.")]
        [SerializeField] private float maxHealth = 100f;

        [Tooltip("Current health. Set to maxHealth on spawn; shown here so it can be watched at runtime.")]
        [SerializeField] private float currentHealth;

        [Header("XP Rewards")]
        [Tooltip("Run XP granted to the CURRENT run when this enemy dies (temporary progression).")]
        [SerializeField] private int runXpReward = 25;

        [Tooltip("Permanent XP granted when this enemy dies (survives death).")]
        [SerializeField] private int permanentXpReward = 10;

        // Guards against Die() running more than once so XP is awarded exactly once per enemy.
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
        /// Handles death. Awards XP exactly once (the isDead guard prevents repeats), then removes
        /// the enemy. Run XP feeds the temporary current-run progression; Permanent XP feeds the
        /// persistent Survivor progression that survives death. The two are fully independent.
        /// </summary>
        private void Die()
        {
            isDead = true;

            if (RunProgression.Instance != null)
                RunProgression.Instance.AddXp(runXpReward);

            if (PermanentProgression.Instance != null)
                PermanentProgression.Instance.AddXp(permanentXpReward);

            Destroy(gameObject);
        }
    }
}
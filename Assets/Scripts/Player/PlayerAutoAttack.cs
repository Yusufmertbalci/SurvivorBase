using UnityEngine;
using Game.Enemies; // EnemyChase lives here

namespace Game.Player
{
    /// <summary>
    /// Automatic player attack foundation for a survivor-style game. With no player input it finds
    /// the closest enemy in range, waits for the cooldown, and performs an attack, repeatedly.
    ///
    /// Targeting and attack timing live here; the actual damage is applied in PerformAttack() by
    /// calling TakeDamage() on the target's EnemyHealth. The damage/kill logging lives in EnemyHealth.
    /// </summary>
    public class PlayerAutoAttack : MonoBehaviour
    {
        [Header("Attack Settings")]
        [Tooltip("Radius (world units) within which enemies can be targeted and attacked.")]
        [SerializeField] private float attackRange = 5f;

        [Tooltip("Seconds between attacks.")]
        [SerializeField] private float attackCooldown = 1f;

        [Tooltip("Damage dealt to the target's EnemyHealth on each attack.")]
        [SerializeField] private float damage = 10f;

        // Reused buffer for the non-allocating physics query (mobile-friendly: no per-query GC).
        private const int MaxCollidersDetected = 32;
        private readonly Collider[] _hitBuffer = new Collider[MaxCollidersDetected];

        // The enemy we're currently attacking. Held until it dies or leaves range (a "sticky" target),
        // so we don't run a physics query every frame while a valid target already exists.
        private Transform _currentTarget;

        // Counts down to zero; an attack is allowed when it reaches zero.
        private float _cooldownTimer;

        private void Update()
        {
            // Frame-rate independent cooldown.
            _cooldownTimer -= Time.deltaTime;

            // Only search when we don't already have a valid target (no per-frame search while fighting).
            if (!IsTargetValid(_currentTarget))
                _currentTarget = FindClosestEnemy();

            // Nothing in range: nothing to do this frame.
            if (_currentTarget == null)
                return;

            if (_cooldownTimer <= 0f)
            {
                PerformAttack(_currentTarget);
                _cooldownTimer = attackCooldown;
            }
        }

        /// <summary>
        /// A target is valid if it still exists (not destroyed) and is within attack range.
        /// Cheap distance check only - no physics query.
        /// </summary>
        private bool IsTargetValid(Transform candidate)
        {
            if (candidate == null)
                return false;

            float sqrDistance = (candidate.position - transform.position).sqrMagnitude;
            return sqrDistance <= attackRange * attackRange;
        }

        /// <summary>
        /// Finds the closest valid enemy inside attackRange using a non-allocating overlap query.
        /// A valid enemy is any collider whose GameObject has an EnemyChase component.
        /// Returns null when none are in range.
        /// </summary>
        private Transform FindClosestEnemy()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, _hitBuffer);

            Transform closest = null;
            float closestSqrDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];

                // Valid enemy = has an EnemyChase component. This filter also naturally excludes
                // the player's own collider, the ground plane, and anything else in range.
                if (!hit.TryGetComponent(out EnemyChase _))
                    continue;

                float sqrDistance = (hit.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closest = hit.transform;
                }
            }

            return closest;
        }

        /// <summary>
        /// The attack event: applies damage to the target's EnemyHealth. The targeting and cooldown
        /// logic above is unchanged. TryGetComponent allocates nothing and does no scene-wide search.
        /// </summary>
        private void PerformAttack(Transform enemy)
        {
            if (enemy.TryGetComponent(out EnemyHealth health))
                health.TakeDamage(damage);
        }

        // Development-only visualization of the attack range. Drawn when the Player is selected in
        // the Scene view. Has no effect on gameplay.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.35f, 0.35f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
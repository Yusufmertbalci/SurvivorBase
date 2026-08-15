using UnityEngine;
using Game.Player; // PlayerHealth lives here

namespace Game.Enemies
{
    /// <summary>
    /// Makes an enemy attack its target (the Player) when the target is within attack range.
    ///
    /// Reuses the target that EnemyChase already holds (assigned by the spawner) instead of doing a
    /// second Player lookup, and gates EnemyChase's movement so the enemy stops advancing while it is
    /// in range. All attack logic lives here; EnemyChase stays a pure movement component.
    ///
    /// Range/damage/cooldown are per-instance, so future enemy types just use different values here.
    /// </summary>
    [RequireComponent(typeof(EnemyChase))]
    public class EnemyAttack : MonoBehaviour
    {
        [Tooltip("The enemy stops chasing and attacks when the target is within this distance.")]
        [SerializeField] private float attackRange = 2f;

        [Tooltip("Damage dealt to the Player per attack.")]
        [SerializeField] private int attackDamage = 10;

        [Tooltip("Seconds between attacks.")]
        [SerializeField] private float attackCooldown = 1f;

        private EnemyChase _chase;
        private PlayerHealth _playerHealth;
        private float _cooldownTimer;

        private void Awake()
        {
            _chase = GetComponent<EnemyChase>();
        }

        private void Update()
        {
            // Frame-rate independent cooldown.
            _cooldownTimer -= Time.deltaTime;

            // Reuse the target EnemyChase is already chasing (assigned by the spawner).
            Transform target = _chase.Target;
            if (target == null)
            {
                // No/destroyed target: don't leave the enemy frozen.
                _chase.SetMovementEnabled(true);
                return;
            }

            float sqrDistance = (target.position - transform.position).sqrMagnitude;
            bool inRange = sqrDistance <= attackRange * attackRange;

            // Stop chasing while in range; resume when the target leaves range.
            _chase.SetMovementEnabled(!inRange);

            if (inRange && _cooldownTimer <= 0f)
            {
                AttackPlayer(target);
                _cooldownTimer = attackCooldown;
            }
        }

        private void AttackPlayer(Transform target)
        {
            // Cache the Player's health the first time we need it (the target is the Player).
            if (_playerHealth == null)
                target.TryGetComponent(out _playerHealth);

            if (_playerHealth != null)
                _playerHealth.TakeDamage(attackDamage);
        }

        // Development-only visualization of the attack range, drawn when the enemy is selected.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
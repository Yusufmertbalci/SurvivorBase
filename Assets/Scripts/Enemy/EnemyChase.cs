using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Basic enemy chase behavior: moves toward a target across the flat X/Z plane and
    /// smoothly turns to face its movement direction (Y-axis rotation only, stays upright).
    ///
    /// No Rigidbody, NavMesh, or pathfinding - this is the deliberately simple base that
    /// future enemy AI will build on. The enemy's Y position is never changed while chasing.
    /// </summary>
    public class EnemyChase : MonoBehaviour
    {
        [Tooltip("The Transform to chase. Assign the Player here.")]
        [SerializeField] private Transform target;

        [Tooltip("Movement speed in world units per second.")]
        [SerializeField] private float moveSpeed = 2.5f;

        [Tooltip("How fast the enemy turns to face its movement direction, in degrees per second.")]
        [SerializeField] private float rotationSpeed = 720f;

        // Squared distance below which the enemy is treated as 'arrived'.
        // Prevents normalizing a near-zero vector (NaN) and jittering on top of the target.
        private const float ArrivalThresholdSqr = 0.0001f;

        // When false, the enemy holds position (still faces the target). Toggled by EnemyAttack
        // so the enemy stops advancing once it is within attack range.
        private bool _canMove = true;

        private void Start()
        {
            if (target == null)
            {
                Debug.LogWarning(
                    $"{nameof(EnemyChase)}: No target assigned. " +
                    "Assign the Player to the Target field in the Inspector.", this);
            }
        }

        /// <summary>
        /// Assigns the Transform this enemy should chase. Used by the spawner to hand each
        /// spawned enemy its target at runtime.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// The Transform this enemy is chasing. Exposed so sibling components (e.g. EnemyAttack)
        /// can reuse the same target instead of maintaining a second Player reference.
        /// </summary>
        public Transform Target => target;

        /// <summary>
        /// Enables or disables chase movement. EnemyAttack turns this off while the target is in
        /// attack range so the enemy stops advancing, and back on when the target leaves range.
        /// </summary>
        public void SetMovementEnabled(bool value) => _canMove = value;

        private void Update()
        {
            if (target == null)
                return;

            // Direction to the target, flattened onto the X/Z plane so Y never changes.
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < ArrivalThresholdSqr)
                return;

            Vector3 direction = toTarget.normalized;

            // Frame-rate independent movement on the X/Z plane only.
            // Skipped while movement is disabled (in attack range) so the enemy stops advancing.
            if (_canMove)
                transform.position += direction * (moveSpeed * Time.deltaTime);

            // Smoothly rotate to face the movement direction. Direction has y = 0, so this is a
            // pure yaw (Y-axis) rotation and the enemy stays upright.
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }
}
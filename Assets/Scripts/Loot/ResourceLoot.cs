using System.Collections;
using UnityEngine;
using Game.Base;      // ResourceType
using Game.Player;    // PlayerHealth identifies the player
using Game.Resources; // RunResourceInventory

namespace Game.Loot
{
    /// <summary>
    /// A physical RUN-resource pickup dropped by a dying enemy. It sits on the ground until the player
    /// comes within magnetRadius, then slides toward the player and is collected once within
    /// pickupDistance, adding its resource to RunResourceInventory. It NEVER touches BaseResourceManager,
    /// PermanentProgression, or RunProgression - pure bonus run income.
    ///
    /// Same magnet philosophy as XPCrystal (player cached via PlayerHealth, MoveTowards, single-reward
    /// guard) but intentionally slower, and with hysteresis: it starts pulling within magnetRadius and
    /// STOPS (staying where it is) if the player runs beyond magnetBreakRadius, re-activating if they
    /// return. Collection uses an explicit pickupDistance check, so entering the radius alone never
    /// collects - the loot must travel here first. OnTriggerEnter is kept as a contact fallback.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ResourceLoot : MonoBehaviour
    {
        [Tooltip("Resource type granted on pickup (normally set at runtime by the enemy via Initialize).")]
        [SerializeField] private ResourceType resourceType = ResourceType.Wood;

        [Tooltip("Amount granted on pickup (normally set at runtime by the enemy via Initialize).")]
        [SerializeField] private int amount = 10;

        [Header("Magnet")]
        [Tooltip("The player must come within this distance for the loot to START moving toward them.")]
        [SerializeField] private float magnetRadius = 3f;

        [Tooltip("The magnet stops when the player moves farther than this distance.")]
        [SerializeField] private float magnetBreakRadius = 5f;

        [Tooltip("How fast the loot slides toward the player once magnetized (units/sec). Slower than XP crystals.")]
        [SerializeField] private float magnetSpeed = 6f;

        [Tooltip("Collect once the loot is within this distance of the player. Prevents jitter/overshoot.")]
        [SerializeField] private float pickupDistance = 0.5f;

        [Header("Optional pickup feedback")]
        [Tooltip("Seconds to shrink before destroying. 0 = destroy instantly.")]
        [SerializeField] private float collectShrinkDuration = 0.08f;

        // Shared, lazily-resolved player reference - one lookup total, reused by every loot pickup.
        private static PlayerHealth _player;

        private bool _magnetActive;
        private bool _collected;

        private void Awake()
        {
            // Same physics as XPCrystal: trigger + kinematic, gravity-free Rigidbody so it stays put
            // and detects the player without touching the player's setup.
            if (TryGetComponent(out Collider col))
                col.isTrigger = true;

            if (!TryGetComponent(out Rigidbody body))
                body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        /// <summary>Sets the resource type and amount at spawn. Called by EnemyHealth on the drop.</summary>
        public void Initialize(ResourceType type, int lootAmount)
        {
            resourceType = type;
            amount = lootAmount;
        }

        private void Update()
        {
            if (_collected)
                return;

            PlayerHealth player = ResolvePlayer();
            if (player == null || player.IsDead)
                return; // no target, or the run ended - stay put, don't collect

            Vector3 playerPosition = player.transform.position;
            float sqrToPlayer = (playerPosition - transform.position).sqrMagnitude;

            if (!_magnetActive)
            {
                // Activate when the player comes within the (smaller) start radius.
                if (sqrToPlayer <= magnetRadius * magnetRadius)
                    _magnetActive = true;
            }
            else
            {
                // Break the magnet when the player moves beyond the (larger) break radius. The gap
                // between magnetRadius and magnetBreakRadius prevents rapid on/off toggling at the edge.
                if (sqrToPlayer > magnetBreakRadius * magnetBreakRadius)
                    _magnetActive = false;
            }

            if (!_magnetActive)
                return; // stopped: stay exactly where we are until the player returns

            // Slide toward the player. Scaled time, so it pauses with the game if ever paused.
            transform.position = Vector3.MoveTowards(
                transform.position, playerPosition, magnetSpeed * Time.deltaTime);

            // Collect only once close enough - the loot must physically travel here first.
            float sqrNow = (playerPosition - transform.position).sqrMagnitude;
            if (sqrNow <= pickupDistance * pickupDistance)
                Collect();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Contact fallback (e.g. the player walks directly onto the loot). Single-reward guarded.
            if (_collected)
                return;
            if (!other.TryGetComponent(out PlayerHealth player))
                return;
            if (player.IsDead)
                return;

            Collect();
        }

        /// <summary>Single award + removal path. Runs exactly once thanks to _collected.</summary>
        private void Collect()
        {
            if (_collected)
                return;

            _collected = true;

            // RUN inventory only - never BaseResourceManager. Fires ResourcesChanged, so the run HUD updates.
            if (RunResourceInventory.Instance != null)
                RunResourceInventory.Instance.Add(resourceType, amount);

            // Stop further trigger hits while the pickup is removed.
            if (TryGetComponent(out Collider col))
                col.enabled = false;

            if (collectShrinkDuration > 0f)
                StartCoroutine(ShrinkAndDestroy());
            else
                Destroy(gameObject);
        }

        private IEnumerator ShrinkAndDestroy()
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            // Unscaled time so the tiny pop still plays even if the game is paused.
            while (elapsed < collectShrinkDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Clamp01(elapsed / collectShrinkDuration);
                transform.localScale = startScale * k;
                yield return null;
            }

            Destroy(gameObject);
        }

        private static PlayerHealth ResolvePlayer()
        {
            // Unity's overloaded == treats a destroyed player as null, so this re-resolves after a
            // scene reload. FindAnyObjectByType runs only when the cache is empty (shared by all loot).
            if (_player == null)
                _player = FindAnyObjectByType<PlayerHealth>();
            return _player;
        }
    }
}
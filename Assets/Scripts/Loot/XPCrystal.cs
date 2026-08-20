using System.Collections;
using UnityEngine;
using Game.Progression; // RunProgression / PermanentProgression
using Game.Player;      // PlayerHealth identifies the player

namespace Game.Loot
{
    /// <summary>
    /// A collectible XP crystal dropped by a dying enemy.
    ///
    /// It sits idle where it dropped until the player comes within pickupRadius; then it flies toward
    /// the player ("magnet") and is collected on contact via OnTriggerEnter. On collection it awards
    /// Run XP and Permanent XP through the EXISTING progression APIs, EXACTLY ONCE, then destroys
    /// itself. The reward architecture is unchanged - only how the crystal reaches the player is new.
    ///
    /// Player lookup is done ONCE and cached statically (shared by every crystal), so there are no
    /// per-crystal or per-frame scene searches. Magnet movement uses scaled time, so it naturally
    /// freezes during the upgrade-screen pause. Physics is unchanged: a trigger collider plus a
    /// kinematic, gravity-free Rigidbody, so the crystal never pushes the Player and the Player's own
    /// setup is untouched.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class XPCrystal : MonoBehaviour
    {
        [Header("Reward (configurable per crystal / enemy type)")]
        [Tooltip("Run XP granted on pickup (temporary run progression).")]
        [SerializeField] private int runXpReward = 25;

        [Tooltip("Permanent XP granted on pickup (survives death).")]
        [SerializeField] private int permanentXpReward = 10;

        [Header("Magnet")]
        [Tooltip("The player must come within this distance for the crystal to start flying to them.")]
        [SerializeField] private float pickupRadius = 3f;

        [Tooltip("How fast the crystal flies toward the player once magnetized (units/second).")]
        [SerializeField] private float magnetSpeed = 8f;

        [Header("Optional pickup feedback")]
        [Tooltip("Seconds to shrink before the crystal is destroyed. 0 = destroy instantly.")]
        [SerializeField] private float collectShrinkDuration = 0.08f;

        [Tooltip("Scale multiplier applied once when the crystal magnetizes (1 = no pop).")]
        [SerializeField] private float magnetizePopScale = 1.2f;

        // Shared, lazily-resolved player reference - one lookup total, reused by every crystal.
        private static PlayerHealth _player;

        // Simple state: idle -> magnet active -> collected. _collected keeps the single-reward guard.
        private bool _magnetActive;
        private bool _collected;

        private void Awake()
        {
            // Make pickup reliable regardless of minor prefab misconfiguration.
            if (TryGetComponent(out Collider col))
                col.isTrigger = true;

            if (!TryGetComponent(out Rigidbody body))
                body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;   // don't fall or get pushed
            body.useGravity = false;
        }

        private void Update()
        {
            if (_collected)
                return;

            PlayerHealth player = ResolvePlayer();

            // No player, or the run has ended: stay idle. This prevents the magnet from collecting
            // onto a dead/frozen player and minting accidental XP.
            if (player == null || player.IsDead)
                return;

            Vector3 playerPosition = player.transform.position;

            // Activate the magnet once the player is inside the radius; it stays active afterwards
            // even if the player moves back out.
            if (!_magnetActive)
            {
                float sqrDistance = (playerPosition - transform.position).sqrMagnitude;
                if (sqrDistance <= pickupRadius * pickupRadius)
                    Magnetize();
            }

            if (_magnetActive)
            {
                // Scaled time: freezes with Time.timeScale = 0 during the upgrade pause. Collection
                // itself still happens through OnTriggerEnter when the crystal reaches the player.
                transform.position = Vector3.MoveTowards(
                    transform.position, playerPosition, magnetSpeed * Time.deltaTime);
            }
        }

        private void Magnetize()
        {
            _magnetActive = true;

            // Tiny optional pop so the pull reads clearly. No particles/VFX/sound.
            if (magnetizePopScale > 0f && !Mathf.Approximately(magnetizePopScale, 1f))
                transform.localScale *= magnetizePopScale;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected)
                return;

            // Only the player collects. Identify by PlayerHealth (no tag setup, no scene-wide search).
            // (Use GetComponentInParent if the Player's collider ever lives on a child object.)
            if (!other.TryGetComponent(out PlayerHealth player))
                return;

            // Don't award onto a dead player (run has ended).
            if (player.IsDead)
                return;

            Collect();
        }

        /// <summary>
        /// The single collection + reward path. Runs exactly once (guarded by _collected), so even if
        /// several trigger callbacks arrive, XP is awarded only one time.
        /// </summary>
        private void Collect()
        {
            _collected = true;

            // Award through the existing progression APIs only - never modify their fields directly.
            if (RunProgression.Instance != null)
                RunProgression.Instance.AddXp(runXpReward);
            if (PermanentProgression.Instance != null)
                PermanentProgression.Instance.AddXp(permanentXpReward);

            // Disable the collider immediately so no further trigger events can occur while removing.
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

            // Unscaled time so the tiny pop still plays even if the pickup triggered a level-up pause.
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
            // scene reload. FindAnyObjectByType runs only when the cache is empty (shared across all
            // crystals) - never every frame once resolved.
            if (_player == null)
                _player = FindAnyObjectByType<PlayerHealth>();
            return _player;
        }
    }
}
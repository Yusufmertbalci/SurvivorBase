using System.Collections;
using UnityEngine;
using Game.Progression; // RunProgression / PermanentProgression
using Game.Player;      // PlayerHealth identifies the player

namespace Game.Loot
{
    /// <summary>
    /// A collectible XP crystal dropped by a dying enemy. When the player walks over it, it awards
    /// Run XP and Permanent XP through the EXISTING progression APIs (never touching their fields),
    /// then destroys itself. XP is awarded EXACTLY ONCE.
    ///
    /// Physics: trigger callbacks require a Rigidbody on one of the two colliders. The Player has no
    /// Rigidbody, so this crystal ensures its own collider is a trigger and that it has a kinematic,
    /// gravity-free Rigidbody - that makes OnTriggerEnter fire against the Player's plain collider
    /// while the crystal stays put, and leaves the Player untouched. The crystal has no Update loop,
    /// so it is idle and cheap until it is collected.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class XPCrystal : MonoBehaviour
    {
        [Header("Reward (configurable per crystal / enemy type)")]
        [Tooltip("Run XP granted on pickup (temporary run progression).")]
        [SerializeField] private int runXpReward = 25;

        [Tooltip("Permanent XP granted on pickup (survives death).")]
        [SerializeField] private int permanentXpReward = 10;

        [Header("Optional pickup feedback")]
        [Tooltip("Seconds to shrink before the crystal is destroyed. 0 = destroy instantly.")]
        [SerializeField] private float collectShrinkDuration = 0.08f;

        // Ensures XP is awarded only once, even across multiple overlap frames or re-entry.
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

        private void OnTriggerEnter(Collider other)
        {
            if (_collected)
                return;

            // Only the player collects. Identify the player by its PlayerHealth component - no tag
            // setup and no scene-wide search required. (Use GetComponentInParent instead if the
            // Player's collider ever lives on a child object.)
            if (!other.TryGetComponent(out PlayerHealth _))
                return;

            _collected = true;

            // Award through the existing progression APIs only - never modify their fields directly.
            if (RunProgression.Instance != null)
                RunProgression.Instance.AddXp(runXpReward);
            if (PermanentProgression.Instance != null)
                PermanentProgression.Instance.AddXp(permanentXpReward);

            Collect();
        }

        private void Collect()
        {
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

            // Unscaled time so the tiny pop still plays even if the pickup triggers a level-up pause.
            while (elapsed < collectShrinkDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Clamp01(elapsed / collectShrinkDuration);
                transform.localScale = startScale * k;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
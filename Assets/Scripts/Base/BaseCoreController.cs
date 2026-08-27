using UnityEngine;
using Game.Player; // PlayerMovement identifies the player

namespace Game.Base
{
    /// <summary>
    /// Scene-side view/interaction for the persistent BaseCoreManager. It:
    ///  - spawns the correct level visual prefab (and swaps it on upgrade / on scene reload), and
    ///  - acts as the interaction trigger: while the player is inside and an upgrade is available, it
    ///    tells BaseCoreInteractionUI to show the prompt.
    ///
    /// A scene component is required because BaseCoreManager is DontDestroyOnLoad and must not own a
    /// scene-bound visual (that would leak into GameScene). The manager owns the level/data; this
    /// renders it. Put this on the BaseCore object with a (trigger) collider for the interaction range.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BaseCoreController : MonoBehaviour
    {
        [Tooltip("Where the level visual prefab is spawned (defaults to this transform).")]
        [SerializeField] private Transform visualAnchor;

        private GameObject _visual;
        private bool _playerInRange;

        private void Awake()
        {
            if (visualAnchor == null)
                visualAnchor = transform;

            // Interaction trigger (like BuildSlot): trigger collider + kinematic Rigidbody so it fires
            // against the player's plain collider without changing the player.
            if (TryGetComponent(out Collider col))
                col.isTrigger = true;

            if (!TryGetComponent(out Rigidbody body))
                body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void Start()
        {
            if (BaseCoreManager.Instance != null)
                BaseCoreManager.Instance.BaseCoreChanged += HandleBaseCoreChanged;

            SpawnVisual();       // reconstruct the current level's visual on (re)load
            UpdateInteraction();
        }

        private void OnDestroy()
        {
            if (BaseCoreManager.Instance != null)
                BaseCoreManager.Instance.BaseCoreChanged -= HandleBaseCoreChanged;
        }

        private void HandleBaseCoreChanged()
        {
            SpawnVisual();       // swap to the new level's visual
            UpdateInteraction(); // the upgrade may no longer be available
        }

        private void SpawnVisual()
        {
            BaseCoreManager mgr = BaseCoreManager.Instance;
            if (mgr == null)
                return;

            // Only one Base Core visual should exist at a time.
            if (_visual != null)
                Destroy(_visual);

            BaseCoreLevelData data = mgr.CurrentLevelData;
            GameObject prefab = data != null ? data.VisualPrefab : null;
            if (prefab != null)
                _visual = Instantiate(prefab, visualAnchor.position, visualAnchor.rotation, visualAnchor);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerMovement _))
                return;

            _playerInRange = true;
            UpdateInteraction();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerMovement _))
                return;

            _playerInRange = false;
            UpdateInteraction();
        }

        private void UpdateInteraction()
        {
            if (BaseCoreInteractionUI.Instance == null || BaseCoreManager.Instance == null)
                return;

            bool available = _playerInRange && BaseCoreManager.Instance.IsUpgradeAvailable();
            BaseCoreInteractionUI.Instance.SetAvailable(available, BaseCoreManager.Instance.NextLevel);
        }
    }
}
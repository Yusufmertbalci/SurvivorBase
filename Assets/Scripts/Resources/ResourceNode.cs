using UnityEngine;
using Game.Player; // PlayerMovement identifies the player

namespace Game.Resources
{
    /// <summary>
    /// A gatherable resource node (tree, rock, ...), configured by ResourceNodeData. While the player
    /// is inside its trigger and it is Available, it shows the gather prompt; each Gather() removes
    /// AmountPerGather into the RunResourceInventory until the node is Depleted, then hides it.
    ///
    /// Self-contained trigger (kinematic Rigidbody), mirroring BuildSlot, so it detects the player
    /// without changing the player's setup. Run-scoped: it lives and dies with GameScene.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ResourceNode : MonoBehaviour
    {
        public enum NodeState { Available, Depleted }

        [Tooltip("Definition (resource type, totals, amount per gather).")]
        [SerializeField] private ResourceNodeData data;

        [Tooltip("Object hidden when the node depletes (defaults to this GameObject).")]
        [SerializeField] private GameObject visualToHideOnDeplete;

        private int _remaining;
        private NodeState _state = NodeState.Available;
        private bool _playerInRange;

        public ResourceNodeData Data => data;
        public NodeState State => _state;

        private void Awake()
        {
            // Interaction trigger + kinematic Rigidbody so OnTriggerEnter fires against the player.
            if (TryGetComponent(out Collider col))
                col.isTrigger = true;

            if (!TryGetComponent(out Rigidbody body))
                body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            _remaining = data != null ? data.TotalAmount : 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other))
                return;

            _playerInRange = true;
            UpdatePrompt();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other))
                return;

            _playerInRange = false;
            if (ResourceGatheringController.Instance != null)
                ResourceGatheringController.Instance.HideFor(this);
        }

        /// <summary>
        /// Gathers one interaction's worth into the RUN inventory (not the base stockpile). Returns the
        /// amount gathered. No-op once depleted. Called by ResourceGatheringController's button.
        /// </summary>
        public int Gather()
        {
            if (_state == NodeState.Depleted || data == null || _remaining <= 0)
                return 0;

            int amount = Mathf.Min(data.AmountPerGather, _remaining);
            _remaining -= amount;

            if (RunResourceInventory.Instance != null)
                RunResourceInventory.Instance.Add(data.Type, amount);

            if (_remaining <= 0)
                Deplete();
            else
                UpdatePrompt();

            return amount;
        }

        private void Deplete()
        {
            _state = NodeState.Depleted;
            _playerInRange = false;

            if (ResourceGatheringController.Instance != null)
                ResourceGatheringController.Instance.HideFor(this);

            // Depleted for the rest of this run (no respawn in the prototype).
            GameObject toHide = visualToHideOnDeplete != null ? visualToHideOnDeplete : gameObject;
            toHide.SetActive(false);
        }

        private void UpdatePrompt()
        {
            if (ResourceGatheringController.Instance == null)
                return;

            if (_playerInRange && _state == NodeState.Available)
                ResourceGatheringController.Instance.ShowFor(this);
            else
                ResourceGatheringController.Instance.HideFor(this);
        }

        private static bool IsPlayer(Collider other) => other.TryGetComponent(out PlayerMovement _);
    }
}
using UnityEngine;
using Game.Player; // PlayerMovement identifies the base player

namespace Game.Base
{
    /// <summary>
    /// A predefined location where ONE specific building can be constructed. It shows a Build prompt
    /// while the player stands in its trigger and the building is Unlocked, spawns the building prefab
    /// on Build, and reconstructs an already-Built building when the Base scene (re)loads.
    ///
    /// It does NOT own building progression - BuildingManager remains the single source of truth for
    /// Locked/Unlocked/Built. The slot only asks BuildingManager for state and tells it SetBuilt.
    ///
    /// Self-contained physics: it forces its collider to a trigger and adds a kinematic Rigidbody, so
    /// OnTriggerEnter fires against the player's plain collider without changing the player.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BuildSlot : MonoBehaviour
    {
        [Tooltip("The building this slot builds. Each slot maps to exactly one BuildingData.")]
        [SerializeField] private BuildingData buildingData;

        // The spawned building instance (null until built/reconstructed). Prevents duplicate spawns.
        private GameObject _instance;

        public BuildingData BuildingData => buildingData;

        private void Awake()
        {
            if (TryGetComponent(out Collider col))
                col.isTrigger = true;

            if (!TryGetComponent(out Rigidbody body))
                body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void Start()
        {
            // Reconstruction: if this building was already built earlier this session, spawn it now.
            if (buildingData != null && BuildingManager.Instance != null &&
                BuildingManager.Instance.GetState(buildingData) == BuildingState.Built)
            {
                SpawnBuilding();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other) || buildingData == null || BuildingManager.Instance == null)
                return;

            // Only prompt when the building is Unlocked (not Locked, not already Built).
            if (BuildingManager.Instance.GetState(buildingData) == BuildingState.Unlocked &&
                BuildInteractionUI.Instance != null)
            {
                BuildInteractionUI.Instance.ShowFor(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other) || BuildInteractionUI.Instance == null)
                return;

            BuildInteractionUI.Instance.HideFor(this);
        }

        /// <summary>
        /// Constructs the building if it is currently Unlocked. Spawns the prefab, then flips the
        /// BuildingManager state Unlocked -> Built. Ignored if Locked or already Built (no double build).
        /// Called by the Build button via BuildInteractionUI.
        /// </summary>
        public void Build()
        {
            if (buildingData == null || BuildingManager.Instance == null)
                return;

            if (BuildingManager.Instance.GetState(buildingData) != BuildingState.Unlocked)
                return;

            SpawnBuilding();
            BuildingManager.Instance.SetBuilt(buildingData);
        }

        private void SpawnBuilding()
        {
            if (_instance != null)
                return; // already present - never duplicate

            GameObject prefab = buildingData.BuildingPrefab;
            if (prefab == null)
            {
                Debug.LogWarning($"{name}: '{buildingData.DisplayName}' has no Building Prefab assigned; nothing spawned.", this);
                return;
            }

            _instance = Instantiate(prefab, transform.position, transform.rotation, transform);
        }

        private static bool IsPlayer(Collider other) => other.TryGetComponent(out PlayerMovement _);
    }
}

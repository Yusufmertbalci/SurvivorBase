using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Base
{
    /// <summary>Runtime state of a building. Locked/Unlocked are derived from Base Level; Built is future.</summary>
    public enum BuildingState
    {
        Locked,
        Unlocked,
        Built
    }

    /// <summary>
    /// Logical owner of base building state. It knows the building definitions, checks each one's
    /// requirements against the current Base Level, and tracks per-building RUNTIME state (session
    /// only - the ScriptableObject definitions stay static). It does NOT place meshes, spawn workers,
    /// spend resources, or manage scenes/XP.
    ///
    /// Persistent singleton (DontDestroyOnLoad) so building state survives scene changes, matching
    /// BaseProgression. Unlock is derived on demand from the current Base Level, so it always reflects
    /// the latest level; only the future "Built" flag and building level are stored.
    /// </summary>
    public class BuildingManager : MonoBehaviour
    {
        public static BuildingManager Instance { get; private set; }

        [Tooltip("Static building definitions (ScriptableObject assets). Assign all base buildings here.")]
        [SerializeField] private BuildingData[] buildingDefinitions;

        // Runtime, session-only state per building. Never written back into the ScriptableObjects.
        private class BuildingRuntime
        {
            public bool IsBuilt;      // future: true once construction is implemented
            public int CurrentLevel;  // future: raised by upgrades
        }

        private readonly Dictionary<BuildingData, BuildingRuntime> _runtime =
            new Dictionary<BuildingData, BuildingRuntime>();

        /// <summary>Read-only list of building definitions, for UI iteration.</summary>
        public IReadOnlyList<BuildingData> Buildings => buildingDefinitions;

        /// <summary>Raised when building runtime state changes (e.g. a building becomes Built), so UI can refresh.</summary>
        public event Action BuildingsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildRuntimeState();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void BuildRuntimeState()
        {
            _runtime.Clear();
            if (buildingDefinitions == null)
                return;

            foreach (BuildingData def in buildingDefinitions)
            {
                if (def == null || _runtime.ContainsKey(def))
                    continue;

                _runtime[def] = new BuildingRuntime
                {
                    IsBuilt = false,
                    CurrentLevel = def.StartingBuildingLevel
                };
            }
        }

        /// <summary>
        /// Current state of a building: Built once constructed (future), otherwise Unlocked when its
        /// requirements are met at the current Base Level, otherwise Locked.
        /// </summary>
        public BuildingState GetState(BuildingData building)
        {
            if (building == null)
                return BuildingState.Locked;

            if (_runtime.TryGetValue(building, out BuildingRuntime runtime) && runtime.IsBuilt)
                return BuildingState.Built;

            return AreRequirementsMet(building) ? BuildingState.Unlocked : BuildingState.Locked;
        }

        /// <summary>Whether the building's requirements are currently satisfied.</summary>
        public bool IsUnlocked(BuildingData building) => AreRequirementsMet(building);

        /// <summary>Whether the building has been constructed this session.</summary>
        public bool IsBuilt(BuildingData building) =>
            building != null && _runtime.TryGetValue(building, out BuildingRuntime runtime) && runtime.IsBuilt;

        /// <summary>
        /// Marks a building as Built (Unlocked -> Built). No-op if it's already built, which keeps the
        /// same building from being constructed twice. Raises BuildingsChanged so UI refreshes.
        /// This is the ONLY place the Built flag is set; BuildSlot calls it after spawning the prefab.
        /// </summary>
        public void SetBuilt(BuildingData building)
        {
            if (building == null || !_runtime.TryGetValue(building, out BuildingRuntime runtime))
                return;

            if (runtime.IsBuilt)
                return;

            runtime.IsBuilt = true;
            BuildingsChanged?.Invoke();
        }

        /// <summary>Current runtime building level (starts at the definition's StartingBuildingLevel).</summary>
        public int GetCurrentLevel(BuildingData building)
        {
            if (building != null && _runtime.TryGetValue(building, out BuildingRuntime runtime))
                return runtime.CurrentLevel;

            return building != null ? building.StartingBuildingLevel : 0;
        }

        /// <summary>
        /// THE requirement extension point. Today it only checks Base Level. Later, extra requirements
        /// (permanent resources, other buildings built, worker count, Survivor Level, milestones,
        /// quests) get AND-ed in here - no caller needs to change.
        /// </summary>
        private bool AreRequirementsMet(BuildingData building)
        {
            if (building == null)
                return false;

            int baseLevel = BaseProgression.Instance != null ? BaseProgression.Instance.CurrentBaseLevel : 1;
            return baseLevel >= building.RequiredBaseLevel;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Persistent, DATA-DRIVEN unlock framework. It reads HighestRunLevelTracker and the configured
    /// BaseMilestoneData assets, and tracks which building / NPC / system ids are unlocked. All later
    /// base content (Workshop, Watchtower, Lumber Yard, Stoneworks, their NPCs, the raid system, ...)
    /// asks IsBuildingUnlocked / IsNpcUnlocked / IsSystemUnlocked instead of hard-coding level checks.
    ///
    /// Multi-milestone jumps are handled: every milestone whose requirement is at or below the current
    /// highest run level is applied, so jumping from highest 4 to 12 unlocks the Lv5 and Lv10 content
    /// at once. Unlocks are additive and never revert. DontDestroyOnLoad singleton (session-only).
    /// </summary>
    public class MilestoneManager : MonoBehaviour
    {
        public static MilestoneManager Instance { get; private set; }

        [Tooltip("All Base milestone definitions. Order doesn't matter; each is applied when its level is reached.")]
        [SerializeField] private BaseMilestoneData[] milestones;

        private readonly HashSet<string> _unlockedBuildings = new HashSet<string>();
        private readonly HashSet<string> _unlockedNpcs = new HashSet<string>();
        private readonly HashSet<string> _unlockedSystems = new HashSet<string>();

        /// <summary>Raised whenever new content becomes unlocked, so base UI can refresh.</summary>
        public event Action MilestonesChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (HighestRunLevelTracker.Instance != null)
                HighestRunLevelTracker.Instance.Changed -= Evaluate;
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            // Subscribe here so HighestRunLevelTracker.Instance is set (its Awake ran already).
            if (HighestRunLevelTracker.Instance != null)
                HighestRunLevelTracker.Instance.Changed += Evaluate;
            else
                Debug.LogWarning($"{nameof(MilestoneManager)}: No HighestRunLevelTracker found; nothing will unlock.", this);

            Evaluate(); // apply any milestones already reached
        }

        public bool IsBuildingUnlocked(string id) => _unlockedBuildings.Contains(id);
        public bool IsNpcUnlocked(string id) => _unlockedNpcs.Contains(id);
        public bool IsSystemUnlocked(string id) => _unlockedSystems.Contains(id);

        private void Evaluate()
        {
            if (HighestRunLevelTracker.Instance == null || milestones == null)
                return;

            int highest = HighestRunLevelTracker.Instance.HighestRunLevel;
            bool changed = false;

            foreach (BaseMilestoneData milestone in milestones)
            {
                if (milestone == null || milestone.RequiredHighestRunLevel > highest)
                    continue;

                changed |= AddAll(_unlockedBuildings, milestone.UnlockedBuildingIds, "Building", milestone);
                changed |= AddAll(_unlockedNpcs, milestone.UnlockedNpcIds, "NPC", milestone);
                changed |= AddAll(_unlockedSystems, milestone.UnlockedSystemIds, "System", milestone);
            }

            if (changed)
                MilestonesChanged?.Invoke();
        }

        private static bool AddAll(HashSet<string> set, string[] ids, string kind, BaseMilestoneData milestone)
        {
            if (ids == null)
                return false;

            bool added = false;
            foreach (string id in ids)
            {
                if (string.IsNullOrEmpty(id))
                    continue;

                if (set.Add(id)) // Add returns false if it was already present (idempotent)
                {
                    added = true;
                    Debug.Log($"[Milestone] Unlocked {kind}: '{id}' " +
                              $"(from '{milestone.Label}', Lv {milestone.RequiredHighestRunLevel}).");
                }
            }
            return added;
        }
    }
}
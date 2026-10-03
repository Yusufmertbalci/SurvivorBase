using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Base;        // BuildingManager, BuildingData
using Game.Progression; // MilestoneManager (unlock source of truth)

namespace Game.Npc
{
    /// <summary>Result of an assignment attempt - so invalid assignments fail explicitly, never silently.</summary>
    public enum NpcAssignResult
    {
        Success,
        UnknownNpc,
        NpcLocked,
        NpcAlreadyAssigned,
        BuildingMissing,
        BuildingNotBuilt,
        Incompatible,
        BuildingOccupied
    }

    /// <summary>
    /// Reusable NPC assignment core. It knows the NpcData definitions and tracks which NPC is assigned
    /// to which building (one NPC -> one building; one building -> one NPC). Unlock state is NOT owned
    /// here - it is read from MilestoneManager (the milestone data is the source of truth). Built state
    /// is read from BuildingManager. No hard-coded "builder"/"workshop" logic - everything is by id/data.
    ///
    /// Persistent (DontDestroyOnLoad), like the other Base managers, so assignments survive
    /// BaseScene -> GameScene -> BaseScene for the session (no disk save yet). Buildings still function
    /// without an NPC; the NPC is purely an enhancement.
    /// </summary>
    public class NpcManager : MonoBehaviour
    {
        public static NpcManager Instance { get; private set; }

        [Tooltip("All NPC definitions (ScriptableObject assets).")]
        [SerializeField] private NpcData[] npcDefinitions;

        // npcId -> assigned buildingId. Absence of a key means the NPC is unassigned.
        private readonly Dictionary<string, string> _assignments = new Dictionary<string, string>();

        /// <summary>Raised when any assignment changes, so NPC/base UI can refresh.</summary>
        public event Action NpcsChanged;

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
            if (Instance == this)
                Instance = null;
        }

        public NpcData GetNpcData(string npcId)
        {
            if (npcDefinitions == null || string.IsNullOrEmpty(npcId))
                return null;

            foreach (NpcData npc in npcDefinitions)
            {
                if (npc != null && npc.NpcId == npcId)
                    return npc;
            }
            return null;
        }

        /// <summary>Unlock state comes from MilestoneManager - not a second unlock system.</summary>
        public bool IsNpcUnlocked(string npcId) =>
            MilestoneManager.Instance != null && MilestoneManager.Instance.IsNpcUnlocked(npcId);

        public bool IsNpcAssigned(string npcId) => _assignments.ContainsKey(npcId);

        public string GetAssignedBuilding(string npcId) =>
            _assignments.TryGetValue(npcId, out string buildingId) ? buildingId : null;

        public bool IsBuildingOccupied(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
                return false;

            foreach (KeyValuePair<string, string> pair in _assignments)
            {
                if (pair.Value == buildingId)
                    return true;
            }
            return false;
        }

        /// <summary>Which NPC (if any) is assigned to a building - a hook for future bonus consumers.</summary>
        public string GetNpcAssignedToBuilding(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
                return null;

            foreach (KeyValuePair<string, string> pair in _assignments)
            {
                if (pair.Value == buildingId)
                    return pair.Key;
            }
            return null;
        }

        /// <summary>Validates an assignment without performing it (all the assignment rules live here).</summary>
        public NpcAssignResult Validate(string npcId, BuildingData building)
        {
            NpcData npc = GetNpcData(npcId);
            if (npc == null)
                return NpcAssignResult.UnknownNpc;                // RULE: NPC must exist
            if (!IsNpcUnlocked(npcId))
                return NpcAssignResult.NpcLocked;                 // RULE 3: must be unlocked
            if (IsNpcAssigned(npcId))
                return NpcAssignResult.NpcAlreadyAssigned;        // RULE 1/7: one building at a time - Unassign first
            if (building == null)
                return NpcAssignResult.BuildingMissing;           // RULE 4: building must exist
            if (BuildingManager.Instance == null || !BuildingManager.Instance.IsBuilt(building))
                return NpcAssignResult.BuildingNotBuilt;          // RULE 5: must be Built
            if (!npc.IsCompatibleWith(building.BuildingId))
                return NpcAssignResult.Incompatible;              // RULE 6: compatibility
            if (IsBuildingOccupied(building.BuildingId))
                return NpcAssignResult.BuildingOccupied;          // RULE 2: one NPC per building

            return NpcAssignResult.Success;
        }

        public bool CanAssign(string npcId, BuildingData building) =>
            Validate(npcId, building) == NpcAssignResult.Success;

        /// <summary>Assigns an NPC to a building if valid; returns the specific result otherwise.</summary>
        public NpcAssignResult AssignNpc(string npcId, BuildingData building)
        {
            NpcAssignResult result = Validate(npcId, building);
            if (result != NpcAssignResult.Success)
                return result;

            _assignments[npcId] = building.BuildingId;
            Debug.Log($"[NPC] Assigned '{npcId}' to building '{building.BuildingId}'.");
            NpcsChanged?.Invoke();
            return NpcAssignResult.Success;
        }

        /// <summary>Removes an NPC's assignment (the building keeps functioning without it). Returns whether it changed.</summary>
        public bool UnassignNpc(string npcId)
        {
            if (!_assignments.Remove(npcId))
                return false;

            Debug.Log($"[NPC] Unassigned '{npcId}'.");
            NpcsChanged?.Invoke();
            return true;
        }
    }
}
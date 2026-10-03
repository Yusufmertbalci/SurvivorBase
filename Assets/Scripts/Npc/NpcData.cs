using UnityEngine;

namespace Game.Npc
{
    /// <summary>
    /// STATIC definition of a specialist NPC (ScriptableObject). Data-driven and reusable for every
    /// future NPC (Scout, Forester, Mason, ...): identity, which buildings it can work in, and its
    /// configurable bonus multipliers. No runtime/assignment state lives here (that's NpcManager).
    ///
    /// Create via: Assets > Create > SurvivorBase > NPC Data.
    /// </summary>
    [CreateAssetMenu(fileName = "NpcData", menuName = "SurvivorBase/NPC Data")]
    public class NpcData : ScriptableObject
    {
        [Tooltip("Stable unique id, e.g. 'builder'. Matched against BaseMilestoneData NPC unlock ids.")]
        [SerializeField] private string npcId = "npc_id";

        [Tooltip("Human-readable name shown in UI, e.g. 'Builder'.")]
        [SerializeField] private string displayName = "New NPC";

        [TextArea]
        [SerializeField] private string description = "";

        [Tooltip("Building ids this NPC may be assigned to (match BuildingData.BuildingId), e.g. 'workshop'.")]
        [SerializeField] private string[] compatibleBuildingIds;

        [Header("Bonuses (1 = no bonus). Consumed by future systems; not yet wired to gameplay.")]
        [SerializeField] private float constructionSpeedMultiplier = 1f;
        [SerializeField] private float upgradeEfficiencyMultiplier = 1f;
        [SerializeField] private float repairSpeedMultiplier = 1f;

        [Header("Visual (optional placeholder for a future pass)")]
        [SerializeField] private GameObject visualPrefab;

        public string NpcId => npcId;
        public string DisplayName => displayName;
        public string Description => description;
        public GameObject VisualPrefab => visualPrefab;

        public float ConstructionSpeedMultiplier => constructionSpeedMultiplier;
        public float UpgradeEfficiencyMultiplier => upgradeEfficiencyMultiplier;
        public float RepairSpeedMultiplier => repairSpeedMultiplier;

        /// <summary>True if this NPC may be assigned to the given building id.</summary>
        public bool IsCompatibleWith(string buildingId)
        {
            if (compatibleBuildingIds == null || string.IsNullOrEmpty(buildingId))
                return false;

            for (int i = 0; i < compatibleBuildingIds.Length; i++)
            {
                if (compatibleBuildingIds[i] == buildingId)
                    return true;
            }
            return false;
        }
    }
}

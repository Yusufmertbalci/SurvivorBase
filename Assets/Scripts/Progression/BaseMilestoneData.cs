using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// STATIC definition of a Highest-Run-Level milestone: at requiredHighestRunLevel it unlocks the
    /// listed BASE content by string id (buildings, NPCs, systems). Content is referenced by id so new
    /// content is added purely in data - no if-statements per level. No runtime state lives here; the
    /// unlocked set is tracked by MilestoneManager.
    ///
    /// Create via: Assets > Create > SurvivorBase > Base Milestone.
    /// </summary>
    [CreateAssetMenu(fileName = "BaseMilestone", menuName = "SurvivorBase/Base Milestone")]
    public class BaseMilestoneData : ScriptableObject
    {
        [Tooltip("Highest Run Level at which this milestone's content unlocks.")]
        [SerializeField] private int requiredHighestRunLevel = 5;

        [Tooltip("Human-readable label for logs/UI, e.g. 'Workshop & Builder'.")]
        [SerializeField] private string label = "";

        [Tooltip("Building ids unlocked (match BuildingData.BuildingId), e.g. 'workshop'.")]
        [SerializeField] private string[] unlockedBuildingIds;

        [Tooltip("NPC ids unlocked, e.g. 'builder'.")]
        [SerializeField] private string[] unlockedNpcIds;

        [Tooltip("System ids unlocked, e.g. 'base_raid'.")]
        [SerializeField] private string[] unlockedSystemIds;

        public int RequiredHighestRunLevel => requiredHighestRunLevel;
        public string Label => label;
        public string[] UnlockedBuildingIds => unlockedBuildingIds;
        public string[] UnlockedNpcIds => unlockedNpcIds;
        public string[] UnlockedSystemIds => unlockedSystemIds;
    }
}

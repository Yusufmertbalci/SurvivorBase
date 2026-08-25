using System.Collections.Generic;
using UnityEngine;

namespace Game.Base
{
    /// <summary>
    /// STATIC definition of a base building - a ScriptableObject asset. Holds only design-time data
    /// (id, name, requirement, level bounds). It deliberately contains NO runtime state such as
    /// unlocked/built - that lives in BuildingManager, so definitions stay shared and immutable.
    ///
    /// Create assets via: Assets > Create > SurvivorBase > Building Data.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingData", menuName = "SurvivorBase/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [Tooltip("Stable unique id, e.g. 'main_gate'. Used as a key; keep it unique across buildings.")]
        [SerializeField] private string buildingId = "building_id";

        [Tooltip("Human-readable name shown in UI, e.g. 'Main Gate'.")]
        [SerializeField] private string displayName = "New Building";

        [TextArea]
        [SerializeField] private string description = "";

        [Tooltip("Base Level required before this building can be unlocked.")]
        [SerializeField] private int requiredBaseLevel = 1;

        [Tooltip("Building level it starts at once available (future: construction/upgrades).")]
        [SerializeField] private int startingBuildingLevel = 0;

        [Tooltip("Maximum building level (future: upgrades).")]
        [SerializeField] private int maxBuildingLevel = 1;

        [Header("Physical")]
        [Tooltip("Prefab spawned at a BuildSlot when this building is constructed. May be null until built.")]
        [SerializeField] private GameObject buildingPrefab;

        [Header("Cost")]
        [Tooltip("Resources required to construct this building. Editable per building asset.")]
        [SerializeField] private ResourceCost[] constructionCost;

        public string BuildingId => buildingId;
        public string DisplayName => displayName;
        public string Description => description;
        public int RequiredBaseLevel => requiredBaseLevel;
        public int StartingBuildingLevel => startingBuildingLevel;
        public int MaxBuildingLevel => maxBuildingLevel;
        public GameObject BuildingPrefab => buildingPrefab;
        public IReadOnlyList<ResourceCost> ConstructionCost => constructionCost;
    }
}

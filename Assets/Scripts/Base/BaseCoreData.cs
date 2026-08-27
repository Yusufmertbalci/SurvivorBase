using System.Collections.Generic;
using UnityEngine;

namespace Game.Base
{
    /// <summary>
    /// STATIC definition of one Base Core level: the Permanent XP required to upgrade INTO it, the
    /// resource cost to upgrade into it, and the visual prefab shown at that level. The level number
    /// is implicit by index (element 0 = Level 1). No runtime state lives here.
    /// </summary>
    [System.Serializable]
    public class BaseCoreLevelData
    {
        [Tooltip("Permanent XP required to upgrade INTO this level (Level 1 = 0).")]
        [SerializeField] private int requiredPermanentXP;

        [Tooltip("Resources spent to upgrade INTO this level (Level 1 usually empty).")]
        [SerializeField] private ResourceCost[] upgradeCost;

        [Tooltip("Visual prefab shown while the Base Core is at this level.")]
        [SerializeField] private GameObject visualPrefab;

        public int RequiredPermanentXP => requiredPermanentXP;
        public IReadOnlyList<ResourceCost> UpgradeCost => upgradeCost;
        public GameObject VisualPrefab => visualPrefab;
    }

    /// <summary>
    /// The full Base Core progression definition: one BaseCoreLevelData per level, in order.
    /// Create via: Assets > Create > SurvivorBase > Base Core Data. Uses the existing ResourceCost.
    /// </summary>
    [CreateAssetMenu(fileName = "BaseCoreData", menuName = "SurvivorBase/Base Core Data")]
    public class BaseCoreData : ScriptableObject
    {
        [Tooltip("One entry per Base Core level, in order. Element 0 = Level 1, element 1 = Level 2, ...")]
        [SerializeField] private BaseCoreLevelData[] levels;

        /// <summary>Highest level defined (also the level cap).</summary>
        public int MaxLevel => levels != null && levels.Length > 0 ? levels.Length : 1;

        /// <summary>Data for a 1-based level, or null if out of range.</summary>
        public BaseCoreLevelData GetLevel(int level)
        {
            if (levels == null || level < 1 || level > levels.Length)
                return null;

            return levels[level - 1];
        }
    }
}

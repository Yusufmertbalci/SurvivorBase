using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// A simple, Inspector-configurable XP-to-level curve shared by both progression systems, so the
    /// curve logic lives in one place instead of being hardcoded across scripts.
    ///
    /// Values are the cumulative TOTAL XP required to REACH each level:
    ///   index 0 = Level 1 (usually 0), index 1 = Level 2, index 2 = Level 3, ...
    /// Reaching or passing a threshold grants that level, so a single large XP gain can cross several
    /// levels at once. XP beyond the last entry simply caps at the highest defined level.
    /// </summary>
    [System.Serializable]
    public class LevelCurve
    {
        [Tooltip("Cumulative total XP needed to reach each level. Index 0 = Level 1 (usually 0), " +
                 "index 1 = Level 2, and so on.")]
        [SerializeField]
        private int[] cumulativeXpThresholds = { 0, 100, 250, 450, 700 };

        /// <summary>Returns the 1-based level for a given total XP amount.</summary>
        public int GetLevelForXp(int totalXp)
        {
            int level = 1;
            for (int i = 0; i < cumulativeXpThresholds.Length; i++)
            {
                if (totalXp >= cumulativeXpThresholds[i])
                    level = i + 1;
                else
                    break;
            }
            return level;
        }

        /// <summary>
        /// Returns the cumulative total XP required to reach the given 1-based level. Clamped to the
        /// defined range, so a level beyond the curve returns the last (max) threshold. Read-only
        /// helper for UI/HUD - does not change any progression behavior.
        /// </summary>
        public int GetCumulativeXpForLevel(int level)
        {
            if (cumulativeXpThresholds == null || cumulativeXpThresholds.Length == 0)
                return 0;

            int index = Mathf.Clamp(level - 1, 0, cumulativeXpThresholds.Length - 1);
            return cumulativeXpThresholds[index];
        }
    }
}
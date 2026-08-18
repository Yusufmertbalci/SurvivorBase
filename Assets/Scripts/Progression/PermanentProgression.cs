using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// PERMANENT progression that survives player death.
    ///
    /// Permanent XP accumulates across runs and Survivor Level is derived from it. Unlike
    /// RunProgression, this is NEVER reset on death - it persists for the whole game session. (Disk
    /// save/load is a later feature; for now it simply stays alive in memory.)
    ///
    /// Persistent singleton (DontDestroyOnLoad) so it outlives player death and future scene loads,
    /// and is deliberately NOT on the Player GameObject. Future Base/Worker unlock systems query
    /// SurvivorLevel, e.g. if (PermanentProgression.Instance.SurvivorLevel >= requiredLevel).
    /// </summary>
    public class PermanentProgression : MonoBehaviour
    {
        public static PermanentProgression Instance { get; private set; }

        [Tooltip("XP-to-level curve for the permanent Survivor Level.")]
        [SerializeField] private LevelCurve survivorLevelCurve = new LevelCurve();

        private int permanentXp;
        private int survivorLevel = 1;

        public int PermanentXp => permanentXp;

        /// <summary>Permanent Survivor Level. Future unlock systems (workers, base) read this.</summary>
        public int SurvivorLevel => survivorLevel;

        /// <summary>
        /// Cumulative XP required to have reached the current Survivor Level. Read-only helper for the
        /// HUD bar; derived from the same LevelCurve (no duplicated thresholds).
        /// </summary>
        public int CurrentLevelXp => survivorLevelCurve.GetCumulativeXpForLevel(survivorLevel);

        /// <summary>
        /// Cumulative XP required to reach the next Survivor Level (equals CurrentLevelXp at the max
        /// defined level). Read-only helper for the HUD bar.
        /// </summary>
        public int NextLevelXp => survivorLevelCurve.GetCumulativeXpForLevel(survivorLevel + 1);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            survivorLevel = survivorLevelCurve.GetLevelForXp(permanentXp);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Adds Permanent XP and raises Survivor Level, logging each level gained (handles multi-level jumps).</summary>
        public void AddXp(int amount)
        {
            if (amount <= 0)
                return;

            permanentXp += amount;
            Debug.Log($"[Permanent] Gained {amount} Permanent XP. Total Permanent XP: {permanentXp}.");

            int newLevel = survivorLevelCurve.GetLevelForXp(permanentXp);
            while (survivorLevel < newLevel)
            {
                survivorLevel++;
                Debug.Log($"[Permanent] Survivor Level increased to {survivorLevel}!");
            }
        }
    }
}
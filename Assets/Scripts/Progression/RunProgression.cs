using System;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// TEMPORARY, per-run progression.
    ///
    /// Run XP accumulates during the current survival run and Run Level rises with it. This is wiped
    /// by ResetRun() when the player dies, so the next run starts fresh. It is completely INDEPENDENT
    /// from PermanentProgression: resetting the run never touches permanent XP or Survivor Level.
    ///
    /// Scene-scoped singleton (NOT DontDestroyOnLoad) because a run belongs to the current expedition
    /// scene. Accessed via the static Instance so enemies/handlers need no serialized reference.
    /// </summary>
    public class RunProgression : MonoBehaviour
    {
        public static RunProgression Instance { get; private set; }

        [Tooltip("XP-to-level curve for the temporary Run Level.")]
        [SerializeField] private LevelCurve levelCurve = new LevelCurve();

        private int runXp;
        private int runLevel = 1;

        public int RunXp => runXp;
        public int RunLevel => runLevel;

        /// <summary>
        /// Cumulative XP required to have reached the current Run Level. Read-only helper for the HUD
        /// bar; derived from the same LevelCurve (no duplicated thresholds).
        /// </summary>
        public int CurrentLevelXp => levelCurve.GetCumulativeXpForLevel(runLevel);

        /// <summary>
        /// Cumulative XP required to reach the next Run Level (equals CurrentLevelXp at the max
        /// defined level). Read-only helper for the HUD bar.
        /// </summary>
        public int NextLevelXp => levelCurve.GetCumulativeXpForLevel(runLevel + 1);

        /// <summary>
        /// Raised ONCE per Run Level gained, with the new level as the argument. A single AddXp that
        /// crosses several levels fires this multiple times. UpgradeManager subscribes to this.
        /// </summary>
        public event Action<int> OnRunLevelUp;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            runLevel = levelCurve.GetLevelForXp(runXp); // level for 0 XP (usually 1)
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Adds Run XP and raises Run Level, logging each level gained (handles multi-level jumps).</summary>
        public void AddXp(int amount)
        {
            if (amount <= 0)
                return;

            runXp += amount;
            Debug.Log($"[Run] Gained {amount} Run XP. Total Run XP: {runXp}.");

            int newLevel = levelCurve.GetLevelForXp(runXp);
            while (runLevel < newLevel)
            {
                runLevel++;
                Debug.Log($"[Run] Run Level increased to {runLevel}!");
                OnRunLevelUp?.Invoke(runLevel);
            }
        }

        /// <summary>Resets Run XP and Run Level for a fresh run. Called from PlayerDeathHandler on death.</summary>
        public void ResetRun()
        {
            runXp = 0;
            runLevel = levelCurve.GetLevelForXp(0);
            Debug.Log("[Run] Run XP and Run Level reset for a new run.");
        }
    }
}

using UnityEngine;
using Game.Progression; // reads PermanentProgression (never modifies it)

namespace Game.Base
{
    /// <summary>
    /// Permanent Base progression - the development level of the player's home base. This is a
    /// SEPARATE permanent system from PermanentProgression's Survivor Level: its own level, its own
    /// requirements. It never auto-levels; raising the Base Level is a deliberate action (TryLevelUp)
    /// gated by a requirement (currently a Permanent XP threshold). It only READS Permanent XP and
    /// never modifies PermanentProgression, so the two systems stay independent.
    ///
    /// Persistent singleton (DontDestroyOnLoad), like PermanentProgression, so Base Level survives
    /// BaseScene -> GameScene -> death -> BaseScene. Session-only for now (no disk save yet).
    /// Uses a dedicated threshold list, NOT LevelCurve, since Base requirements are their own thing.
    /// </summary>
    public class BaseProgression : MonoBehaviour
    {
        public static BaseProgression Instance { get; private set; }

        [Tooltip("Current Base Level. Starts at 1; raised deliberately via TryLevelUp (never automatically).")]
        [SerializeField] private int currentBaseLevel = 1;

        [Tooltip("Cumulative Permanent XP required to REACH each Base Level. Index 0 = Level 1 (0 XP), " +
                 "index 1 = Level 2, and so on. Separate from Survivor Level requirements.")]
        [SerializeField]
        private int[] baseLevelPermanentXpRequirements = { 0, 100, 300, 600, 1000 };

        public int CurrentBaseLevel => currentBaseLevel;

        /// <summary>Highest Base Level defined by the requirement list.</summary>
        public int MaxBaseLevel =>
            baseLevelPermanentXpRequirements != null && baseLevelPermanentXpRequirements.Length > 0
                ? baseLevelPermanentXpRequirements.Length
                : 1;

        /// <summary>True while a higher Base Level is defined.</summary>
        public bool HasNextLevel => currentBaseLevel < MaxBaseLevel;

        /// <summary>Permanent XP required to reach the next Base Level, or -1 if already at max.</summary>
        public int PermanentXpForNextLevel =>
            HasNextLevel ? baseLevelPermanentXpRequirements[currentBaseLevel] : -1;

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

        /// <summary>
        /// Whether the base can level up right now: a higher level exists AND current Permanent XP
        /// meets its requirement. Reads PermanentProgression; never modifies it.
        /// </summary>
        public bool CanLevelUp()
        {
            if (!HasNextLevel)
                return false;

            PermanentProgression permanent = PermanentProgression.Instance;
            if (permanent == null)
                return false;

            return permanent.PermanentXp >= PermanentXpForNextLevel;
        }

        /// <summary>
        /// Deliberately raises the Base Level by one if CanLevelUp(). Does NOT spend Permanent XP -
        /// the requirement is a cumulative threshold, so Survivor Level is unaffected. Returns whether
        /// a level-up happened. This is the hook a future "Upgrade Base" button will call.
        /// </summary>
        public bool TryLevelUp()
        {
            if (!CanLevelUp())
                return false;

            currentBaseLevel++;
            Debug.Log($"[Base] Base Level increased to {currentBaseLevel}.", this);
            return true;
        }

#if UNITY_EDITOR
        // Editor-only testing hook: right-click the component header in Play mode to force a level-up
        // (ignoring the XP requirement) so you can watch building unlock states change.
        [ContextMenu("DEV: Force Base Level Up")]
        private void DevForceLevelUp()
        {
            if (!HasNextLevel)
                return;

            currentBaseLevel++;
            Debug.Log($"[Base] (DEV) Base Level forced to {currentBaseLevel}.", this);
        }
#endif
    }
}
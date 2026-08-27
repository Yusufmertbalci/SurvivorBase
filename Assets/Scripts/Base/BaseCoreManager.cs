using System;
using UnityEngine;
using Game.Progression; // reads PermanentProgression (never modifies it)

namespace Game.Base
{
    /// <summary>
    /// Owns the current Base Core level and performs upgrades. The Base Core level IS the Base Level:
    /// this is the single authoritative value (BaseProgression forwards to it). Upgrading requires a
    /// Permanent XP threshold (read-only, never consumed) AND a resource cost (spent via the existing
    /// BaseResourceManager). It never auto-upgrades - upgrading is a deliberate action (TryUpgrade).
    ///
    /// Persistent singleton (DontDestroyOnLoad), so the level survives BaseScene -> GameScene ->
    /// BaseScene without reset. It owns level/data only; the scene-side BaseCoreController renders the
    /// visual (a persistent manager must not own scene-bound objects). Raises BaseCoreChanged so the
    /// visual, HUD, and building unlocks refresh.
    /// </summary>
    public class BaseCoreManager : MonoBehaviour
    {
        public static BaseCoreManager Instance { get; private set; }

        [Tooltip("Base Core level definitions (requirements, costs, visuals).")]
        [SerializeField] private BaseCoreData baseCoreData;

        private int _currentLevel = 1;

        /// <summary>Raised whenever the Base Core level changes, so views/UI/unlocks refresh.</summary>
        public event Action BaseCoreChanged;

        public int CurrentLevel => _currentLevel;
        public int MaxLevel => baseCoreData != null ? baseCoreData.MaxLevel : 1;
        public bool IsAtMaxLevel => _currentLevel >= MaxLevel;
        public bool HasNextLevel => _currentLevel < MaxLevel;
        public int NextLevel => _currentLevel + 1;

        public BaseCoreLevelData CurrentLevelData => baseCoreData != null ? baseCoreData.GetLevel(_currentLevel) : null;
        public BaseCoreLevelData NextLevelData => baseCoreData != null ? baseCoreData.GetLevel(_currentLevel + 1) : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _currentLevel = 1; // only the first (surviving) instance runs this, so reloads don't reset it
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>True if current Permanent XP meets the requirement for the next level.</summary>
        public bool IsPermanentXpRequirementMet()
        {
            BaseCoreLevelData next = NextLevelData;
            if (next == null)
                return false;

            PermanentProgression permanent = PermanentProgression.Instance;
            return permanent != null && permanent.PermanentXp >= next.RequiredPermanentXP;
        }

        /// <summary>True if the player can currently afford the next upgrade's resource cost.</summary>
        public bool CanAffordNextUpgrade()
        {
            BaseCoreLevelData next = NextLevelData;
            if (next == null)
                return false;

            return BaseResourceManager.Instance != null &&
                   BaseResourceManager.Instance.CanAfford(next.UpgradeCost);
        }

        /// <summary>
        /// Whether the upgrade prompt should be offered: a next level exists AND its Permanent XP
        /// requirement is met. Resources are NOT part of this - they're checked/shown in the dialog.
        /// </summary>
        public bool IsUpgradeAvailable() => HasNextLevel && IsPermanentXpRequirementMet();

        /// <summary>All conditions including resources.</summary>
        public bool CanUpgradeNow() => IsUpgradeAvailable() && CanAffordNextUpgrade();

        /// <summary>
        /// Attempts the upgrade: re-checks next-level exists, the Permanent XP requirement, and
        /// affordability, then spends the resources, raises the level (= Base Level), and raises
        /// BaseCoreChanged. Permanent XP is NEVER consumed. Returns true only on a real upgrade.
        /// </summary>
        public bool TryUpgrade()
        {
            if (!HasNextLevel)
                return false;

            BaseCoreLevelData next = NextLevelData;
            if (next == null)
                return false;

            // Permanent XP requirement (read-only gate, never spent).
            PermanentProgression permanent = PermanentProgression.Instance;
            if (permanent == null || permanent.PermanentXp < next.RequiredPermanentXP)
                return false;

            // Resource cost: check + deduct atomically (SpendResources is all-or-nothing).
            if (BaseResourceManager.Instance == null ||
                !BaseResourceManager.Instance.SpendResources(next.UpgradeCost))
                return false;

            _currentLevel++;
            Debug.Log($"[BaseCore] Upgraded to Level {_currentLevel} (= Base Level {_currentLevel}).", this);
            BaseCoreChanged?.Invoke();
            return true;
        }

#if UNITY_EDITOR
        // Editor-only testing hook: right-click the component header in Play mode to force an upgrade,
        // ignoring the XP and resource requirements, so you can watch visuals/unlocks change quickly.
        [ContextMenu("DEV: Force Upgrade (ignore requirements)")]
        private void DevForceUpgrade()
        {
            if (!HasNextLevel)
            {
                Debug.Log("[BaseCore] (DEV) Already at max level.");
                return;
            }

            _currentLevel++;
            Debug.Log($"[BaseCore] (DEV) Forced upgrade to Level {_currentLevel}.");
            BaseCoreChanged?.Invoke();
        }
#endif
    }
}

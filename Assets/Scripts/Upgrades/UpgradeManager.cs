using UnityEngine;
using Game.Progression; // RunProgression
using Game.Player;      // PlayerHealth, PlayerAutoAttack

namespace Game.Upgrades
{
    /// <summary>
    /// Survivor-style "choose an upgrade" flow. Listens for Run Level ups via RunProgression's
    /// OnRunLevelUp event, pauses the game, shows the UpgradePanel, applies the chosen upgrade to the
    /// EXISTING player stat scripts, then resumes.
    ///
    /// Multiple level-ups from a single XP gain are queued: the panel stays open (game paused) until
    /// every pending level-up has been spent, so the game never resumes with a level-up still owed.
    ///
    /// This is the ONLY place upgrade logic lives - ProgressionHUD stays display-only, and the
    /// progression systems keep owning their XP/level data. Run upgrades are temporary: they live on
    /// the Player's own components and reset when the scene reloads for a new run.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Scene References")]
        [Tooltip("The UpgradePanel root. Starts hidden; shown on a Run Level up.")]
        [SerializeField] private GameObject upgradePanel;

        [Tooltip("Player's health component (target of the Max HP upgrade).")]
        [SerializeField] private PlayerHealth playerHealth;

        [Tooltip("Player's auto-attack component (target of the Damage and Attack Speed upgrades).")]
        [SerializeField] private PlayerAutoAttack playerAutoAttack;

        [Header("Upgrade Amounts (match your button labels)")]
        [Tooltip("Max HP granted by the Max HP upgrade.")]
        [SerializeField] private int maxHpUpgradeAmount = 20;

        [Tooltip("Damage granted by the Damage upgrade.")]
        [SerializeField] private float damageUpgradeAmount = 20f;

        [Tooltip("Attack-speed percent granted by the Attack Speed upgrade (+15 = +15% faster).")]
        [SerializeField] private float attackSpeedPercentAmount = 15f;

        // Run Level ups still awaiting an upgrade choice (queue for multi-level jumps).
        private int _pendingLevelUps;

        private void Awake()
        {
            // Panel must start hidden during normal gameplay.
            if (upgradePanel != null)
                upgradePanel.SetActive(false);
        }

        private void Start()
        {
            // Subscribe in Start (not OnEnable) so RunProgression.Awake has already set its Instance.
            if (RunProgression.Instance != null)
                RunProgression.Instance.OnRunLevelUp += HandleRunLevelUp;
            else
                Debug.LogWarning($"{nameof(UpgradeManager)}: No RunProgression in the scene; upgrades won't trigger.", this);

            if (upgradePanel == null)
                Debug.LogWarning($"{nameof(UpgradeManager)}: UpgradePanel is not assigned.", this);
            if (playerHealth == null || playerAutoAttack == null)
                Debug.LogWarning($"{nameof(UpgradeManager)}: Player stat references are not fully assigned; some upgrades won't apply.", this);
        }

        private void OnDestroy()
        {
            if (RunProgression.Instance != null)
                RunProgression.Instance.OnRunLevelUp -= HandleRunLevelUp;
        }

        private void HandleRunLevelUp(int newRunLevel)
        {
            _pendingLevelUps++;

            // Open only when the first level-up arrives; further ones just add to the queue while the
            // panel is already open.
            if (_pendingLevelUps == 1)
                OpenPanel();
        }

        private void OpenPanel()
        {
            if (upgradePanel != null)
                upgradePanel.SetActive(true);

            // Pause gameplay. Everything driven by Time.deltaTime freezes; UI button clicks still work.
            Time.timeScale = 0f;
        }

        // --- Button hooks: wire each UpgradePanel button's OnClick to one of these three. ---

        public void ApplyMaxHpUpgrade()
        {
            if (playerHealth != null)
                playerHealth.IncreaseMaxHealth(maxHpUpgradeAmount);
            ConsumeUpgrade();
        }

        public void ApplyDamageUpgrade()
        {
            if (playerAutoAttack != null)
                playerAutoAttack.AddDamage(damageUpgradeAmount);
            ConsumeUpgrade();
        }

        public void ApplyAttackSpeedUpgrade()
        {
            if (playerAutoAttack != null)
                playerAutoAttack.AddAttackSpeedPercent(attackSpeedPercentAmount);
            ConsumeUpgrade();
        }

        /// <summary>
        /// Spends one queued level-up. If more remain, the panel stays open and the game stays paused
        /// for the next choice; otherwise the panel closes and the game resumes.
        /// </summary>
        private void ConsumeUpgrade()
        {
            _pendingLevelUps = Mathf.Max(0, _pendingLevelUps - 1);

            if (_pendingLevelUps > 0)
                return; // another level-up is queued - keep choosing, stay paused

            if (upgradePanel != null)
                upgradePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
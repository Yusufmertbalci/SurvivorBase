using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Progression; // PermanentProgression

namespace Game.Base
{
    /// <summary>
    /// Confirmation dialog for upgrading the Base Core. Shows the next level, the Permanent XP
    /// requirement (vs current, marked red if unmet), the resource cost (vs current, red if short),
    /// and enables Upgrade only when both are satisfied. Confirm re-validates and calls
    /// BaseCoreManager.TryUpgrade(), which spends resources and raises the level. Cancel spends nothing.
    ///
    /// Follows BuildConfirmationUI's philosophy: no progression or resource ownership here.
    /// Scene-scoped singleton (not persistent).
    /// </summary>
    public class BaseCoreUpgradeUI : MonoBehaviour
    {
        public static BaseCoreUpgradeUI Instance { get; private set; }

        [Header("UI")]
        [Tooltip("Root of the confirmation panel. Hidden by default.")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI permanentXpText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI currentResourcesText;
        [Tooltip("Upgrade button - disabled automatically when the XP requirement or resources are unmet.")]
        [SerializeField] private Button confirmButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (panel != null)
                panel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Opens the dialog for the next Base Core level (called by BaseCoreInteractionUI).</summary>
        public void Open()
        {
            BaseCoreManager mgr = BaseCoreManager.Instance;
            if (mgr == null || !mgr.HasNextLevel)
                return;

            BaseCoreLevelData next = mgr.NextLevelData;
            if (next == null)
                return;

            if (titleText != null)
                titleText.text = $"UPGRADE BASE CORE TO LEVEL {mgr.NextLevel}?";

            // Permanent XP requirement (read-only, never spent).
            PermanentProgression perm = PermanentProgression.Instance;
            int currentXp = perm != null ? perm.PermanentXp : 0;
            bool xpMet = currentXp >= next.RequiredPermanentXP;
            if (permanentXpText != null)
            {
                string xpLine = $"Permanent XP: {currentXp} / {next.RequiredPermanentXP}";
                permanentXpText.text = xpMet ? xpLine : $"<color=#FF5A5A>{xpLine}</color>";
            }

            PopulateResources(next.UpgradeCost);

            bool affordable = BaseResourceManager.Instance != null &&
                              BaseResourceManager.Instance.CanAfford(next.UpgradeCost);
            if (confirmButton != null)
                confirmButton.interactable = xpMet && affordable;

            if (panel != null)
                panel.SetActive(true);
        }

        /// <summary>Wire the Cancel button here. Spends nothing, changes nothing.</summary>
        public void OnCancelPressed() => Close();

        /// <summary>
        /// Wire the Upgrade button here. TryUpgrade re-checks level, XP, and resources and spends
        /// atomically, so nothing happens if conditions changed since the dialog opened.
        /// </summary>
        public void OnConfirmPressed()
        {
            if (BaseCoreManager.Instance != null)
                BaseCoreManager.Instance.TryUpgrade();

            Close();
        }

        private void Close()
        {
            if (panel != null)
                panel.SetActive(false);

            if (BaseCoreInteractionUI.Instance != null)
                BaseCoreInteractionUI.Instance.OnUpgradeUIClosed();
        }

        private void PopulateResources(IReadOnlyList<ResourceCost> costs)
        {
            BaseResourceManager rm = BaseResourceManager.Instance;

            StringBuilder costSb = new StringBuilder();
            StringBuilder haveSb = new StringBuilder();

            if (costs != null)
            {
                for (int i = 0; i < costs.Count; i++)
                {
                    ResourceCost cost = costs[i];
                    int current = rm != null ? rm.Get(cost.Type) : 0;
                    bool enough = current >= cost.Amount;

                    string costLine = $"{cost.Amount} {cost.Type}";
                    costSb.AppendLine(enough ? costLine : $"<color=#FF5A5A>{costLine}</color>");
                    haveSb.AppendLine($"{current} {cost.Type}");
                }
            }

            if (costText != null)
                costText.text = costSb.ToString().TrimEnd();
            if (currentResourcesText != null)
                currentResourcesText.text = haveSb.ToString().TrimEnd();
        }
    }
}

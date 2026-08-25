using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Base
{
    /// <summary>
    /// Confirmation dialog for constructing the building at a selected BuildSlot. It shows the cost
    /// and current resources, enables/disables the Confirm button by affordability, and on Confirm it
    /// re-validates, spends resources via BaseResourceManager, then calls BuildSlot.Build().
    ///
    /// It holds no building or resource state of its own - it just orchestrates the existing systems.
    /// Scene-scoped singleton (not persistent): it belongs to the Base scene UI.
    /// </summary>
    public class BuildConfirmationUI : MonoBehaviour
    {
        public static BuildConfirmationUI Instance { get; private set; }

        [Header("UI")]
        [Tooltip("Root of the confirmation panel. Hidden by default.")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI currentResourcesText;
        [Tooltip("The Confirm/Build button - disabled automatically when unaffordable.")]
        [SerializeField] private Button confirmButton;

        private BuildSlot _currentSlot;

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

        /// <summary>Opens the dialog for a slot: shows name, cost, current resources, and affordability.</summary>
        public void Open(BuildSlot slot)
        {
            if (slot == null || slot.BuildingData == null)
                return;

            _currentSlot = slot;
            BuildingData data = slot.BuildingData;

            if (titleText != null)
                titleText.text = $"BUILD {data.DisplayName.ToUpper()}?";

            PopulateResourceText(data);

            bool affordable = BaseResourceManager.Instance != null &&
                              BaseResourceManager.Instance.CanAfford(data.ConstructionCost);
            if (confirmButton != null)
                confirmButton.interactable = affordable;

            if (panel != null)
                panel.SetActive(true);
        }

        /// <summary>Wire the Cancel button here. Spends nothing and changes no state.</summary>
        public void OnCancelPressed() => Close();

        /// <summary>
        /// Wire the Confirm/Build button here. Re-checks state and resources at the moment of
        /// construction (never trusting only the earlier UI check), then spends, spawns, and marks Built.
        /// </summary>
        public void OnConfirmPressed()
        {
            if (_currentSlot == null)
            {
                Close();
                return;
            }

            BuildingData data = _currentSlot.BuildingData;
            if (data == null || BuildingManager.Instance == null)
            {
                Close();
                return;
            }

            // 1-2. Still unlocked / buildable?
            if (BuildingManager.Instance.GetState(data) != BuildingState.Unlocked)
            {
                Close();
                return;
            }

            // 3-4. Re-check and deduct atomically. SpendResources returns false if unaffordable.
            if (BaseResourceManager.Instance == null ||
                !BaseResourceManager.Instance.SpendResources(data.ConstructionCost))
            {
                Close();
                return;
            }

            // 5-6. Spawn the prefab and flip Unlocked -> Built.
            _currentSlot.Build();

            // 7-8. Hide this panel and the build prompt.
            if (BuildInteractionUI.Instance != null)
                BuildInteractionUI.Instance.HideFor(_currentSlot);

            Close();
        }

        private void Close()
        {
            _currentSlot = null;
            if (panel != null)
                panel.SetActive(false);

            // Let the build prompt un-suppress. It reappears only if the player is still in an
            // unlocked slot (Cancel); after a successful build its slot was cleared, so it stays hidden.
            if (BuildInteractionUI.Instance != null)
                BuildInteractionUI.Instance.OnConfirmationClosed();
        }

        private void PopulateResourceText(BuildingData data)
        {
            BaseResourceManager rm = BaseResourceManager.Instance;
            System.Collections.Generic.IReadOnlyList<ResourceCost> costs = data.ConstructionCost;

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
                    // Mark insufficient costs in red (TextMeshPro rich text).
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
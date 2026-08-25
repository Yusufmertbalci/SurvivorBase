using UnityEngine;
using TMPro;

namespace Game.Base
{
    /// <summary>
    /// View for a single building row. Display-only: it just writes the building's name and its
    /// current state (from BuildingManager) into three text fields. It holds no building logic and
    /// never reads/writes BuildingManager, BuildingData, or BaseProgression itself.
    /// </summary>
    public class BuildingRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI buildingNameText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI requirementText;

        /// <summary>Populates the row from a building definition and its current state.</summary>
        public void Bind(BuildingData building, BuildingState state)
        {
            if (building == null)
                return;

            if (buildingNameText != null)
                buildingNameText.text = building.DisplayName;

            switch (state)
            {
                case BuildingState.Unlocked:
                    SetStatus("UNLOCKED", null);
                    break;

                case BuildingState.Built:
                    SetStatus("BUILT", null);
                    break;

                default: // Locked
                    SetStatus("LOCKED", $"Requires Base Level {building.RequiredBaseLevel}");
                    break;
            }
        }

        private void SetStatus(string status, string requirement)
        {
            if (statusText != null)
                statusText.text = status;

            if (requirementText != null)
            {
                bool hasRequirement = !string.IsNullOrEmpty(requirement);
                requirementText.text = hasRequirement ? requirement : string.Empty;
                // Hide the requirement line entirely when there's nothing to show.
                requirementText.gameObject.SetActive(hasRequirement);
            }
        }
    }
}

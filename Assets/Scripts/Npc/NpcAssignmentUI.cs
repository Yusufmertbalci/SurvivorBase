using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Base;        // BuildingManager, BuildingData
using Game.Progression; // MilestoneManager

namespace Game.Npc
{
    /// <summary>
    /// Minimal Base-scene panel to view and toggle ONE NPC's assignment to ONE target building
    /// (configured via the Inspector, so no hard-coded ids). Reuses the existing managers - it only
    /// displays state and calls NpcManager. Refreshes on NPC / building / milestone change events
    /// (no polling). For more NPCs later, add another panel or generalize this into a list.
    /// </summary>
    public class NpcAssignmentUI : MonoBehaviour
    {
        [Header("Config")]
        [Tooltip("The NPC this panel manages (e.g. Builder).")]
        [SerializeField] private NpcData npc;

        [Tooltip("The building to assign the NPC to (e.g. Workshop's BuildingData).")]
        [SerializeField] private BuildingData targetBuilding;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button assignButton;
        [SerializeField] private Button unassignButton;

        private bool _subscribed;

        private void OnEnable()
        {
            TrySubscribe();
            Refresh();
        }

        private void Start()
        {
            TrySubscribe();
            Refresh();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void TrySubscribe()
        {
            if (_subscribed)
                return;

            // All three are persistent managers created in BaseScene; by Start they exist.
            if (NpcManager.Instance == null || BuildingManager.Instance == null || MilestoneManager.Instance == null)
                return;

            NpcManager.Instance.NpcsChanged += Refresh;
            BuildingManager.Instance.BuildingsChanged += Refresh;   // Built state / milestone-driven building unlocks
            MilestoneManager.Instance.MilestonesChanged += Refresh; // NPC unlock
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
                return;

            if (NpcManager.Instance != null) NpcManager.Instance.NpcsChanged -= Refresh;
            if (BuildingManager.Instance != null) BuildingManager.Instance.BuildingsChanged -= Refresh;
            if (MilestoneManager.Instance != null) MilestoneManager.Instance.MilestonesChanged -= Refresh;
            _subscribed = false;
        }

        private void Refresh()
        {
            if (npc == null)
                return;

            string npcId = npc.NpcId;
            NpcManager npcManager = NpcManager.Instance;

            bool unlocked = npcManager != null && npcManager.IsNpcUnlocked(npcId);
            bool assigned = npcManager != null && npcManager.IsNpcAssigned(npcId);
            bool built = targetBuilding != null && BuildingManager.Instance != null &&
                         BuildingManager.Instance.IsBuilt(targetBuilding);
            bool canAssign = npcManager != null && npcManager.CanAssign(npcId, targetBuilding);

            if (statusText != null)
            {
                string status;
                if (!unlocked)
                    status = $"{npc.DisplayName}: Locked";
                else if (assigned)
                    status = $"{npc.DisplayName}: Assigned to {npcManager.GetAssignedBuilding(npcId)}";
                else if (!built)
                    status = $"{npc.DisplayName}: Unlocked - build {(targetBuilding != null ? targetBuilding.DisplayName : "target")} first";
                else
                    status = $"{npc.DisplayName}: Available";
                statusText.text = status;
            }

            if (assignButton != null)
            {
                assignButton.gameObject.SetActive(unlocked && !assigned);
                assignButton.interactable = canAssign;
            }

            if (unassignButton != null)
                unassignButton.gameObject.SetActive(assigned);
        }

        /// <summary>Wire the Assign button's OnClick here.</summary>
        public void OnAssignPressed()
        {
            if (npc != null && NpcManager.Instance != null)
                NpcManager.Instance.AssignNpc(npc.NpcId, targetBuilding); // NpcsChanged -> Refresh
        }

        /// <summary>Wire the Unassign button's OnClick here.</summary>
        public void OnUnassignPressed()
        {
            if (npc != null && NpcManager.Instance != null)
                NpcManager.Instance.UnassignNpc(npc.NpcId); // NpcsChanged -> Refresh
        }
    }
}
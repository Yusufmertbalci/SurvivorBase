using UnityEngine;
using TMPro;

namespace Game.Resources
{
    /// <summary>
    /// Displays the temporary RUN resources, read from RunResourceInventory (NOT BaseResourceManager -
    /// that stays owned by BaseResourceHUD). Refreshes on RunResourceInventory.ResourcesChanged.
    /// Display-only. Lives in GameScene.
    /// </summary>
    public class RunResourceHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI woodText;
        [SerializeField] private TextMeshProUGUI stoneText;

        private bool _subscribed;

        private void OnEnable()
        {
            TrySubscribe();
            Refresh();
        }

        private void Start()
        {
            // Covers the case where the inventory wasn't ready during OnEnable.
            TrySubscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void TrySubscribe()
        {
            if (_subscribed || RunResourceInventory.Instance == null)
                return;

            RunResourceInventory.Instance.ResourcesChanged += Refresh;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
                return;

            if (RunResourceInventory.Instance != null)
                RunResourceInventory.Instance.ResourcesChanged -= Refresh;
            _subscribed = false;
        }

        private void Refresh()
        {
            RunResourceInventory inv = RunResourceInventory.Instance;
            if (inv == null)
                return;

            if (woodText != null)
                woodText.text = $"WOOD: {inv.GetWood()}";
            if (stoneText != null)
                stoneText.text = $"STONE: {inv.GetStone()}";
        }
    }
}

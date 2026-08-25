using UnityEngine;
using TMPro;

namespace Game.Base
{
    /// <summary>
    /// Displays the current base resources (Wood, Stone). Display-only: it reads BaseResourceManager
    /// and refreshes when its ResourcesChanged event fires - no polling, no scene searches.
    /// </summary>
    public class BaseResourceHUD : MonoBehaviour
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
            // Covers the case where the manager wasn't ready yet during OnEnable.
            TrySubscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void TrySubscribe()
        {
            if (_subscribed || BaseResourceManager.Instance == null)
                return;

            BaseResourceManager.Instance.ResourcesChanged += Refresh;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
                return;

            if (BaseResourceManager.Instance != null)
                BaseResourceManager.Instance.ResourcesChanged -= Refresh;
            _subscribed = false;
        }

        private void Refresh()
        {
            BaseResourceManager rm = BaseResourceManager.Instance;
            if (rm == null)
                return;

            if (woodText != null)
                woodText.text = $"WOOD: {rm.GetWood()}";
            if (stoneText != null)
                stoneText.text = $"STONE: {rm.GetStone()}";
        }
    }
}
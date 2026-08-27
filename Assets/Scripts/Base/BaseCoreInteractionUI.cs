using UnityEngine;
using TMPro;

namespace Game.Base
{
    /// <summary>
    /// The upgrade prompt for the Base Core (canvas side). BaseCoreController calls SetAvailable(...)
    /// as the player enters/leaves range and as the Base Core changes; the button's OnClick calls
    /// OnUpgradePressed, which opens BaseCoreUpgradeUI. While that dialog is open the prompt is
    /// suppressed; when it closes the prompt returns only if an upgrade is still available.
    ///
    /// Scene-scoped singleton (not persistent): it belongs to the Base scene UI. UI only - no upgrade
    /// or resource logic here.
    /// </summary>
    public class BaseCoreInteractionUI : MonoBehaviour
    {
        public static BaseCoreInteractionUI Instance { get; private set; }

        [Tooltip("The Upgrade Base Core button root to show/hide. Hidden by default.")]
        [SerializeField] private GameObject buttonRoot;

        [Tooltip("Label on the button, e.g. 'UPGRADE BASE CORE -> LV.2'.")]
        [SerializeField] private TextMeshProUGUI buttonLabel;

        // Player is in range AND an upgrade is available (set by BaseCoreController).
        private bool _available;

        // The upgrade confirmation dialog is open (suppresses the prompt behind it).
        private bool _suppressed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            RefreshButton(); // starts hidden
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Called by BaseCoreController: whether the upgrade prompt should be offered.</summary>
        public void SetAvailable(bool available, int nextLevel)
        {
            _available = available;

            if (available && buttonLabel != null)
                buttonLabel.text = $"UPGRADE BASE CORE \u2192 LV.{nextLevel}";

            RefreshButton();
        }

        /// <summary>Wire the Upgrade Base Core button's OnClick here.</summary>
        public void OnUpgradePressed()
        {
            if (!_available)
                return;

            if (BaseCoreUpgradeUI.Instance == null)
            {
                Debug.LogWarning($"{nameof(BaseCoreInteractionUI)}: No BaseCoreUpgradeUI in the scene.", this);
                return;
            }

            _suppressed = true;
            RefreshButton();
            BaseCoreUpgradeUI.Instance.Open();
        }

        /// <summary>Called by BaseCoreUpgradeUI when its panel closes (Cancel or Upgrade).</summary>
        public void OnUpgradeUIClosed()
        {
            _suppressed = false;
            RefreshButton();
        }

        private void RefreshButton()
        {
            bool show = _available && !_suppressed;
            if (buttonRoot != null)
                buttonRoot.SetActive(show);
        }
    }
}

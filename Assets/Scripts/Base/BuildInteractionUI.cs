using UnityEngine;
using TMPro;

namespace Game.Base
{
    /// <summary>
    /// The single Build prompt for the Base scene. A BuildSlot calls ShowFor(slot) when the player
    /// enters an unlocked slot and HideFor(slot) when they leave; the button's OnClick calls
    /// OnBuildPressed, which opens the build confirmation.
    ///
    /// The prompt is visible only when the player is in an unlocked slot AND the confirmation panel is
    /// not open. So while the confirmation is up the prompt is hidden; after Cancel it reappears if
    /// the player is still in the slot, and after a successful build it stays hidden (the slot is now
    /// Built, so its proximity is cleared via HideFor).
    ///
    /// Scene-scoped singleton (not persistent): it belongs to the Base scene UI.
    /// </summary>
    public class BuildInteractionUI : MonoBehaviour
    {
        public static BuildInteractionUI Instance { get; private set; }

        [Tooltip("The Build button root to show/hide (e.g. the button GameObject). Hidden by default.")]
        [SerializeField] private GameObject buttonRoot;

        [Tooltip("Label on the Build button, e.g. 'BUILD STORAGE'.")]
        [SerializeField] private TextMeshProUGUI buttonLabel;

        // The unlocked slot the player is currently inside (proximity), or null.
        private BuildSlot _currentSlot;

        // True while the confirmation panel is open, which suppresses (hides) the prompt behind it.
        private bool _suppressedByConfirmation;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            RefreshButton(); // starts hidden (no slot yet)
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Shows the Build prompt for a slot (called by BuildSlot on trigger enter).</summary>
        public void ShowFor(BuildSlot slot)
        {
            if (slot == null || slot.BuildingData == null)
                return;

            _currentSlot = slot;

            if (buttonLabel != null)
                buttonLabel.text = $"BUILD {slot.BuildingData.DisplayName.ToUpper()}";

            RefreshButton();
        }

        /// <summary>Clears the prompt for a slot (trigger exit, or after the building is Built).</summary>
        public void HideFor(BuildSlot slot)
        {
            if (_currentSlot != slot)
                return;

            _currentSlot = null;
            RefreshButton();
        }

        /// <summary>
        /// Wire the Build button's OnClick here. It doesn't build directly - it opens the build
        /// confirmation dialog for the current slot and hides this prompt while the panel is open.
        /// Construction happens only after the player confirms (and pays) in BuildConfirmationUI.
        /// </summary>
        public void OnBuildPressed()
        {
            if (_currentSlot == null)
                return;

            if (BuildConfirmationUI.Instance == null)
            {
                Debug.LogWarning($"{nameof(BuildInteractionUI)}: No BuildConfirmationUI in the scene.", this);
                return;
            }

            // Suppress and hide the prompt while the confirmation panel is open.
            _suppressedByConfirmation = true;
            RefreshButton();

            BuildConfirmationUI.Instance.Open(_currentSlot);
        }

        /// <summary>
        /// Called by BuildConfirmationUI when its panel closes (Cancel or Confirm). Stops suppressing
        /// the prompt; it reappears only if the player is still inside an unlocked slot (Cancel). After
        /// a successful build the slot was cleared via HideFor, so the prompt correctly stays hidden.
        /// </summary>
        public void OnConfirmationClosed()
        {
            _suppressedByConfirmation = false;
            RefreshButton();
        }

        private void RefreshButton()
        {
            bool show = _currentSlot != null && !_suppressedByConfirmation;
            if (buttonRoot != null)
                buttonRoot.SetActive(show);
        }
    }
}

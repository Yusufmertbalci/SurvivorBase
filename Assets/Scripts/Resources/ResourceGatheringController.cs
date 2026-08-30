using UnityEngine;
using TMPro;

namespace Game.Resources
{
    /// <summary>
    /// The gather prompt (canvas side). A ResourceNode calls ShowFor/HideFor as the player enters or
    /// leaves an available node; the button's OnClick calls OnGatherPressed, which gathers from the
    /// current node. UI/input only - it holds no resource state. Mirrors BuildInteractionUI.
    ///
    /// Scene-scoped singleton (GameScene UI).
    /// </summary>
    public class ResourceGatheringController : MonoBehaviour
    {
        public static ResourceGatheringController Instance { get; private set; }

        [Tooltip("The Gather button root to show/hide. Hidden by default.")]
        [SerializeField] private GameObject buttonRoot;

        [Tooltip("Label on the Gather button, e.g. 'GATHER TREE'.")]
        [SerializeField] private TextMeshProUGUI buttonLabel;

        private ResourceNode _currentNode;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (buttonRoot != null)
                buttonRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Shows the gather prompt for a node (called by ResourceNode on trigger enter).</summary>
        public void ShowFor(ResourceNode node)
        {
            if (node == null || node.Data == null)
                return;

            _currentNode = node;

            if (buttonLabel != null)
                buttonLabel.text = $"GATHER {node.Data.DisplayName.ToUpper()}";

            if (buttonRoot != null)
                buttonRoot.SetActive(true);
        }

        /// <summary>Hides the prompt, but only if it's currently showing this node.</summary>
        public void HideFor(ResourceNode node)
        {
            if (_currentNode != node)
                return;

            _currentNode = null;

            if (buttonRoot != null)
                buttonRoot.SetActive(false);
        }

        /// <summary>Wire the Gather button's OnClick here.</summary>
        public void OnGatherPressed()
        {
            if (_currentNode != null)
                _currentNode.Gather();
        }
    }
}
using UnityEngine;
using Game.Base; // reuse the existing ResourceType (no second resource-type system)

namespace Game.Resources
{
    /// <summary>
    /// STATIC definition of a gatherable resource node (tree, rock, ...). Data-driven and extensible:
    /// add new node assets (and later new ResourceType values) without touching gameplay code.
    /// Create via: Assets > Create > SurvivorBase > Resource Node Data.
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceNodeData", menuName = "SurvivorBase/Resource Node Data")]
    public class ResourceNodeData : ScriptableObject
    {
        [Tooltip("Name shown in the gather prompt, e.g. 'Tree' or 'Rock'.")]
        [SerializeField] private string displayName = "Resource Node";

        [Tooltip("Which resource this node yields. Reuses the existing ResourceType enum.")]
        [SerializeField] private ResourceType resourceType = ResourceType.Wood;

        [Tooltip("Total amount in the node before it depletes.")]
        [SerializeField] private int totalAmount = 100;

        [Tooltip("Amount gathered per interaction (per tap).")]
        [SerializeField] private int amountPerGather = 25;

        [Tooltip("Optional visual prefab (unused if the node already has its own mesh).")]
        [SerializeField] private GameObject visualPrefab;

        public string DisplayName => displayName;
        public ResourceType Type => resourceType;
        public int TotalAmount => totalAmount;
        public int AmountPerGather => amountPerGather;
        public GameObject VisualPrefab => visualPrefab;
    }
}

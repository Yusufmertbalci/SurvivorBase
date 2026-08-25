using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Base
{
    /// <summary>The resource types the base economy uses. Extend later (Metal, Food, ...).</summary>
    public enum ResourceType
    {
        Wood,
        Stone
    }

    /// <summary>A single resource requirement, e.g. 150 Wood. Serializable for use in BuildingData.</summary>
    [Serializable]
    public struct ResourceCost
    {
        [SerializeField] private ResourceType type;
        [SerializeField] private int amount;

        public ResourceType Type => type;
        public int Amount => amount;
    }

    /// <summary>
    /// Owns the base's resource pool (session-persistent). It is the ONLY system that stores or
    /// changes Wood/Stone - BuildingManager, BuildSlot, and the UI never hold resource state. It
    /// prevents resources from going below zero and raises ResourcesChanged whenever totals change.
    ///
    /// Persistent singleton (DontDestroyOnLoad), like BaseProgression / BuildingManager, so resources
    /// survive BaseScene -> GameScene -> BaseScene. Initialized once (only the first instance runs its
    /// init), so reloading BaseScene never resets the pool. No disk save yet.
    /// </summary>
    public class BaseResourceManager : MonoBehaviour
    {
        public static BaseResourceManager Instance { get; private set; }

        [Header("Starting Resources (prototype balance)")]
        [SerializeField] private int startingWood = 500;
        [SerializeField] private int startingStone = 300;

        private int _wood;
        private int _stone;

        /// <summary>Raised whenever any resource total changes, so the HUD can refresh.</summary>
        public event Action ResourcesChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Runs only on the first (surviving) instance, so reloading BaseScene keeps current totals.
            _wood = startingWood;
            _stone = startingStone;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public int GetWood() => _wood;
        public int GetStone() => _stone;

        /// <summary>Current amount of a specific resource type.</summary>
        public int Get(ResourceType type) => type == ResourceType.Wood ? _wood : _stone;

        /// <summary>True if every cost entry can be paid from current resources.</summary>
        public bool CanAfford(IReadOnlyList<ResourceCost> costs)
        {
            if (costs == null)
                return true;

            for (int i = 0; i < costs.Count; i++)
            {
                if (Get(costs[i].Type) < costs[i].Amount)
                    return false;
            }

            return true;
        }

        /// <summary>Readable alias for CanAfford.</summary>
        public bool HasResources(IReadOnlyList<ResourceCost> costs) => CanAfford(costs);

        /// <summary>
        /// Deducts the full cost if affordable and raises ResourcesChanged; returns whether it spent.
        /// All-or-nothing: nothing is deducted when the player can't afford the whole cost.
        /// </summary>
        public bool SpendResources(IReadOnlyList<ResourceCost> costs)
        {
            if (!CanAfford(costs))
                return false;

            if (costs != null)
            {
                for (int i = 0; i < costs.Count; i++)
                    Subtract(costs[i].Type, costs[i].Amount);
            }

            ResourcesChanged?.Invoke();
            return true;
        }

        /// <summary>Adds resources (for future gathering). Raises ResourcesChanged.</summary>
        public void AddResources(ResourceType type, int amount)
        {
            if (amount <= 0)
                return;

            if (type == ResourceType.Wood)
                _wood += amount;
            else
                _stone += amount;

            ResourcesChanged?.Invoke();
        }

        private void Subtract(ResourceType type, int amount)
        {
            if (type == ResourceType.Wood)
                _wood = Mathf.Max(0, _wood - amount);
            else
                _stone = Mathf.Max(0, _stone - amount);
        }
    }
}

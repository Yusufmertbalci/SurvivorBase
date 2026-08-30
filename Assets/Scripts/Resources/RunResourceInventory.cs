using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Base; // ResourceType

namespace Game.Resources
{
    /// <summary>
    /// Temporary, RUN-scoped resource inventory - the resources gathered during the current
    /// expedition. Completely separate from BaseResourceManager (the persistent base stockpile).
    ///
    /// Scene singleton (NOT DontDestroyOnLoad): it lives and dies with GameScene, so every run starts
    /// empty and its contents are never carried between runs. On a SUCCESSFUL return, RunEndController
    /// deposits it into BaseResourceManager; on death it is simply destroyed with the scene, so the run
    /// resources are lost. It owns no persistent state and knows nothing about scenes or deposits.
    /// </summary>
    public class RunResourceInventory : MonoBehaviour
    {
        public static RunResourceInventory Instance { get; private set; }

        // Amount per resource type. Dictionary keeps it extensible for future ResourceType values.
        private readonly Dictionary<ResourceType, int> _amounts = new Dictionary<ResourceType, int>();

        /// <summary>Raised whenever a run resource amount changes, so a run HUD can refresh.</summary>
        public event Action ResourcesChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public int Get(ResourceType type) => _amounts.TryGetValue(type, out int value) ? value : 0;
        public int GetWood() => Get(ResourceType.Wood);
        public int GetStone() => Get(ResourceType.Stone);

        /// <summary>Adds gathered resources to the run inventory.</summary>
        public void Add(ResourceType type, int amount)
        {
            if (amount <= 0)
                return;

            _amounts[type] = Get(type) + amount;
            ResourcesChanged?.Invoke();
        }

        /// <summary>Empties the run inventory (used after a successful deposit).</summary>
        public void Clear()
        {
            if (_amounts.Count == 0)
                return;

            _amounts.Clear();
            ResourcesChanged?.Invoke();
        }

        /// <summary>Alias for Clear().</summary>
        public void Reset() => Clear();
    }
}
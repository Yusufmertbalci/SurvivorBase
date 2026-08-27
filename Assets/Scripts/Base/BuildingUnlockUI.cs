using System.Collections.Generic;
using UnityEngine;

namespace Game.Base
{
    /// <summary>
    /// Building unlock UI for the Base scene. It READS the existing systems only: it generates one
    /// row per BuildingData from BuildingManager.Buildings, and shows each building's Locked /
    /// Unlocked / Built state (from BuildingManager.GetState) plus its Base Level requirement. It
    /// refreshes automatically when the Base Level changes.
    ///
    /// It never modifies BuildingManager, BuildingData, or BaseProgression - the only write is the
    /// clearly-labelled DEV level-up button, which calls BaseProgression.TryLevelUp() and therefore
    /// still respects the real requirements. No building names/levels are hardcoded here.
    /// </summary>
    public class BuildingUnlockUI : MonoBehaviour
    {
        [Header("Row Generation")]
        [Tooltip("Row prefab with a BuildingRow component (BuildingRow.prefab).")]
        [SerializeField] private BuildingRow rowPrefab;

        [Tooltip("Parent for the generated rows (e.g. BuildingList, with a Vertical Layout Group).")]
        [SerializeField] private Transform rowContainer;

        // Building definition -> its spawned row. Built once; refreshed in place afterwards.
        private readonly List<KeyValuePair<BuildingData, BuildingRow>> _rows =
            new List<KeyValuePair<BuildingData, BuildingRow>>();

        private int _lastBaseLevel = int.MinValue;

        private void Start()
        {
            if (BuildingManager.Instance == null)
            {
                Debug.LogWarning($"{nameof(BuildingUnlockUI)}: No BuildingManager found; can't build the list.", this);
                return;
            }

            if (rowPrefab == null || rowContainer == null)
            {
                Debug.LogWarning($"{nameof(BuildingUnlockUI)}: Assign Row Prefab and Row Container.", this);
                return;
            }

            BuildRows();
            Refresh();

            // Refresh when a building's state changes (e.g. becomes Built) - not just on Base Level.
            BuildingManager.Instance.BuildingsChanged += Refresh;
        }

        private void OnDestroy()
        {
            // BuildingManager persists (DontDestroyOnLoad), so unsubscribe to avoid a dangling handler.
            if (BuildingManager.Instance != null)
                BuildingManager.Instance.BuildingsChanged -= Refresh;
        }

        private void Update()
        {
            // Refresh only when the Base Level actually changes (one int compare per frame).
            BaseProgression baseProgression = BaseProgression.Instance;
            if (baseProgression == null || baseProgression.CurrentBaseLevel == _lastBaseLevel)
                return;

            Refresh();
        }

        private void BuildRows()
        {
            _rows.Clear();

            foreach (BuildingData building in BuildingManager.Instance.Buildings)
            {
                if (building == null)
                    continue;

                BuildingRow row = Instantiate(rowPrefab, rowContainer);
                _rows.Add(new KeyValuePair<BuildingData, BuildingRow>(building, row));
            }
        }

        private void Refresh()
        {
            BuildingManager manager = BuildingManager.Instance;
            if (manager == null)
                return;

            BaseProgression baseProgression = BaseProgression.Instance;
            if (baseProgression != null)
                _lastBaseLevel = baseProgression.CurrentBaseLevel;

            foreach (KeyValuePair<BuildingData, BuildingRow> pair in _rows)
            {
                if (pair.Value == null)
                    continue;

                BuildingState state = manager.GetState(pair.Key);
                pair.Value.Bind(pair.Key, state);
            }
        }

        /// <summary>
        /// DEV / TEST ONLY. Hook the "DEV: LEVEL UP BASE" button's OnClick here. It now drives the
        /// Base Core upgrade via BaseCoreManager.TryUpgrade(), so it only upgrades when the Permanent
        /// XP requirement AND resources are satisfied; otherwise it logs why. There is no separate
        /// Base Level path. Remove this method and the button for release. (For pure UI/visual testing
        /// that ignores requirements, use BaseCoreManager's editor-only "DEV: Force Upgrade" context menu.)
        /// </summary>
        public void OnDevLevelUpPressed()
        {
            BaseCoreManager mgr = BaseCoreManager.Instance;
            if (mgr == null)
            {
                Debug.LogWarning("[DEV] No BaseCoreManager in the scene.");
                return;
            }

            if (!mgr.HasNextLevel)
            {
                Debug.Log("[DEV] Base Core is already at max level.");
                return;
            }

            if (!mgr.IsPermanentXpRequirementMet())
            {
                int required = mgr.NextLevelData != null ? mgr.NextLevelData.RequiredPermanentXP : 0;
                Debug.Log($"[DEV] Base Core upgrade unavailable: Permanent XP requirement not met (need {required}).");
                return;
            }

            if (!mgr.CanAffordNextUpgrade())
            {
                Debug.Log("[DEV] Base Core upgrade unavailable: insufficient resources.");
                return;
            }

            mgr.TryUpgrade();
        }
    }
}

using UnityEngine;

namespace Game.Base
{
    /// <summary>
    /// Adapter that exposes Base Level to existing systems (BuildingManager, BuildingUnlockUI,
    /// BaseHUD). Base Level is now the Base Core Level and is OWNED by BaseCoreManager - this
    /// component simply FORWARDS CurrentBaseLevel to it, so there is a single authoritative value and
    /// no duplicate state. It stores nothing of its own; leveling now happens via BaseCoreManager.
    ///
    /// Kept as a persistent singleton so existing references to BaseProgression.Instance keep working
    /// unchanged across BaseScene / GameScene / BaseScene.
    /// </summary>
    public class BaseProgression : MonoBehaviour
    {
        public static BaseProgression Instance { get; private set; }

        /// <summary>Base Level = Base Core Level. Authoritative source: BaseCoreManager.</summary>
        public int CurrentBaseLevel => BaseCoreManager.Instance != null ? BaseCoreManager.Instance.CurrentLevel : 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}

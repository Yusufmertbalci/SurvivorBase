using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Progression;

namespace Game.Base
{
    /// <summary>
    /// Base scene HUD. Display-only for permanent progression: it READS the persistent
    /// PermanentProgression singleton (the single source of truth) and never stores or duplicates its
    /// data. It also hosts the Start Run button, which loads the gameplay scene for a fresh run.
    ///
    /// The permanent XP text uses the same within-level format as the gameplay ProgressionHUD, so the
    /// two screens agree. Because PermanentProgression is DontDestroyOnLoad, the value shown here is
    /// the same instance that accumulated XP during the last run.
    /// </summary>
    public class BaseHUD : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("Permanent Survivor Level label, e.g. 'LEVEL 3'.")]
        [SerializeField] private TextMeshProUGUI survivorLevelText;

        [Tooltip("Permanent XP progress label, e.g. '420 / 500 XP'.")]
        [SerializeField] private TextMeshProUGUI permanentXpText;

        [Tooltip("Base Level label, e.g. 'BASE LEVEL 2'. Separate from the Survivor Level above.")]
        [SerializeField] private TextMeshProUGUI baseLevelText;

        [Header("Scene Flow")]
        [Tooltip("Name of the gameplay scene to load on Start Run. Must be in Build Settings.")]
        [SerializeField] private string gameSceneName = "GameScene";

        private int _lastPermanentXp = int.MinValue;
        private int _lastSurvivorLevel = int.MinValue;
        private int _lastBaseLevel = int.MinValue;

        private void Start()
        {
            if (PermanentProgression.Instance == null)
                Debug.LogWarning(
                    $"{nameof(BaseHUD)}: No PermanentProgression found. Add a PermanentProgression object " +
                    "to BaseScene so permanent values persist and display.", this);

            if (BaseProgression.Instance == null)
                Debug.LogWarning(
                    $"{nameof(BaseHUD)}: No BaseProgression found. Add a BaseProgression object to " +
                    "BaseScene so the Base Level persists and displays.", this);
        }

        private void Update()
        {
            RefreshPermanent();
            RefreshBaseLevel();
        }

        // Permanent progression (Survivor Level + Permanent XP), read from PermanentProgression.
        private void RefreshPermanent()
        {
            PermanentProgression permanent = PermanentProgression.Instance;
            if (permanent == null)
                return;

            int permanentXp = permanent.PermanentXp;
            int survivorLevel = permanent.SurvivorLevel;

            if (permanentXp == _lastPermanentXp && survivorLevel == _lastSurvivorLevel)
                return;

            _lastPermanentXp = permanentXp;
            _lastSurvivorLevel = survivorLevel;

            if (survivorLevelText != null)
                survivorLevelText.text = $"LEVEL {survivorLevel}";

            // Within-level progress, matching the gameplay HUD's permanent display.
            int xpIntoLevel = permanentXp - permanent.CurrentLevelXp;
            int xpSpan = permanent.NextLevelXp - permanent.CurrentLevelXp;
            if (permanentXpText != null)
                permanentXpText.text = xpSpan > 0 ? $"{xpIntoLevel} / {xpSpan} XP" : "MAX";
        }

        // Base Level, read from BaseProgression (a separate permanent system).
        private void RefreshBaseLevel()
        {
            BaseProgression baseProgression = BaseProgression.Instance;
            if (baseProgression == null)
                return;

            int baseLevel = baseProgression.CurrentBaseLevel;
            if (baseLevel == _lastBaseLevel)
                return;

            _lastBaseLevel = baseLevel;

            if (baseLevelText != null)
                baseLevelText.text = $"BASE LEVEL {baseLevel}";
        }

        /// <summary>
        /// Hook the Start Run button's OnClick to this. Loads the gameplay scene, which creates a
        /// fresh RunProgression, DifficultyManager, Player (base stats), spawner, and HUD.
        /// </summary>
        public void OnStartRunPressed()
        {
            // Ensure normal time in case a previous state left the game paused.
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }
    }
}

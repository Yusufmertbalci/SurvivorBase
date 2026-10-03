using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Progression
{
    /// <summary>
    /// Tracks the HIGHEST Run Level ever reached this session (max across all runs). It ONLY
    /// increases - a later, lower run never lowers it. It grants NO combat bonuses; it exists purely
    /// to gate BASE content unlocks (see MilestoneManager). This keeps the three layers separate:
    /// Run Level (temporary), Highest Run Level (what content exists), Base Level (how developed it is).
    ///
    /// DontDestroyOnLoad singleton, matching the project's other persistent managers (session-only for
    /// now; disk save is a future task). RunProgression is run-scoped and recreated each GameScene, so
    /// this tracker (re)subscribes to RunProgression.OnRunLevelUp on every scene load rather than
    /// modifying RunProgression.
    /// </summary>
    public class HighestRunLevelTracker : MonoBehaviour
    {
        public static HighestRunLevelTracker Instance { get; private set; }

        [Tooltip("Highest run level reached this session. Increases only; shown here for debugging.")]
        [SerializeField] private int highestRunLevel = 0;

        public int HighestRunLevel => highestRunLevel;

        /// <summary>Raised whenever the highest run level increases.</summary>
        public event Action Changed;

        private RunProgression _subscribedRun;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnsubscribeRun();
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            // sceneLoaded doesn't fire for the scene we were created in, so handle it here.
            TrySubscribeToRun();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => TrySubscribeToRun();

        private void TrySubscribeToRun()
        {
            RunProgression run = RunProgression.Instance;
            if (run == _subscribedRun)
                return; // already subscribed to this run (or both null)

            UnsubscribeRun();

            if (run != null)
            {
                run.OnRunLevelUp += HandleRunLevelUp;
                _subscribedRun = run;

                // OnRunLevelUp only fires on INCREASES, so capture the current level immediately.
                ReportRunLevel(run.RunLevel);
            }
        }

        private void UnsubscribeRun()
        {
            if (_subscribedRun != null)
                _subscribedRun.OnRunLevelUp -= HandleRunLevelUp;
            _subscribedRun = null;
        }

        private void HandleRunLevelUp(int newRunLevel) => ReportRunLevel(newRunLevel);

        /// <summary>Records a run level; raises the highest (and fires Changed) only if it's a new max.</summary>
        public void ReportRunLevel(int runLevel)
        {
            if (runLevel <= highestRunLevel)
                return;

            highestRunLevel = runLevel;
            Debug.Log($"[HighestRunLevel] New highest run level: {highestRunLevel}.");
            Changed?.Invoke();
        }
    }
}
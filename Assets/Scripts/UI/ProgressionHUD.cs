using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Progression;

namespace Game.UI
{
    /// <summary>
    /// Gameplay HUD that only DISPLAYS progression - it never stores or calculates progression data,
    /// so the progression systems remain the single source of truth. It drives TWO fully independent
    /// XP displays that are NEVER mixed:
    ///
    ///   PERMANENT (always visible): Survivor Level + Permanent XP bar/text, read ONLY from
    ///   PermanentProgression. Persists across deaths, so it is never hidden by run state.
    ///
    ///   RUN (hidden while Run XP is 0): Run XP bar/text, read ONLY from RunProgression. Appears the
    ///   moment the player earns Run XP and hides again when the run resets on death.
    ///
    /// Progression is read through the static Instances (not serialized references) so the HUD always
    /// reflects the surviving PermanentProgression after a scene reload / Return to Base. UI element
    /// references are serialized for manual assignment. There are no change events on the progression
    /// systems, so this reads their state and refreshes the UI only when a value actually changes.
    ///
    /// Awake/Start also validate the wiring: fill Images are forced to 'Filled' (so a bar can't get
    /// stuck permanently full), and duplicate/cross-wired references are reported, because those are
    /// the usual causes of "one bar shows the other system's XP".
    /// </summary>
    public class ProgressionHUD : MonoBehaviour
    {
        [Header("Level")]
        [Tooltip("Prominent Survivor/Permanent Level label, e.g. 'LEVEL 2'.")]
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Permanent XP (TOP bar - always visible, reads PermanentProgression)")]
        [Tooltip("Filled Image for the Permanent XP bar (Image Type = Filled, Fill Method = Horizontal).")]
        [SerializeField] private Image permanentXpFillImage;

        [Tooltip("Permanent XP progress label, e.g. '60 / 150 XP'.")]
        [SerializeField] private TextMeshProUGUI permanentXpText;

        [Header("Run XP (BOTTOM bar - hidden while Run XP is 0, reads RunProgression)")]
        [Tooltip("Root object of the whole Run XP panel (e.g. RunXPPanel). Toggled on/off with Run XP.")]
        [SerializeField] private GameObject runXpPanel;

        [Tooltip("Filled Image for the Run XP bar (Image Type = Filled, Fill Method = Horizontal).")]
        [SerializeField] private Image runXpFillImage;

        [Tooltip("Run XP progress label, e.g. '50 / 100 XP'.")]
        [SerializeField] private TextMeshProUGUI runXpText;

        [Tooltip("Run Level label, e.g. 'RUN LEVEL 2'. Separate from the permanent Survivor Level above.")]
        [SerializeField] private TextMeshProUGUI runLevelText;

        // Last-seen source values, for change detection so the UI only refreshes when something changes.
        private int _lastRunXp = -1;
        private int _lastPermanentXp = -1;
        private int _lastSurvivorLevel = -1;

        private void Awake()
        {
            // A fill Image only responds to fillAmount when its Type is 'Filled'. Leaving it as
            // 'Simple' is the usual reason a bar looks permanently full - correct it here so the
            // permanent bar can never get stuck full.
            EnsureFilled(permanentXpFillImage, nameof(permanentXpFillImage));
            EnsureFilled(runXpFillImage, nameof(runXpFillImage));
        }

        private void Start()
        {
            if (RunProgression.Instance == null)
                Debug.LogWarning($"{nameof(ProgressionHUD)}: No RunProgression in the scene; the Run XP UI won't update.", this);
            if (PermanentProgression.Instance == null)
                Debug.LogWarning($"{nameof(ProgressionHUD)}: No PermanentProgression in the scene; the Permanent XP UI won't update.", this);

            // Cross-wiring guard: the same UI element assigned to BOTH permanent and run slots makes
            // one system's XP appear on the other bar/text - exactly the reported bug.
            if (permanentXpFillImage != null && permanentXpFillImage == runXpFillImage)
                Debug.LogError($"{nameof(ProgressionHUD)}: The SAME Image is assigned to both the Permanent and Run fill slots. " +
                               "Assign the TOP (permanent) fill to 'Permanent Xp Fill Image' and the BOTTOM (run) fill to 'Run Xp Fill Image'.", this);
            if (permanentXpText != null && permanentXpText == runXpText)
                Debug.LogError($"{nameof(ProgressionHUD)}: The SAME text is assigned to both the Permanent and Run text slots.", this);

            if (permanentXpFillImage == null || permanentXpText == null)
                Debug.LogWarning($"{nameof(ProgressionHUD)}: Permanent XP UI references are not fully assigned.", this);
            if (runXpFillImage == null || runXpText == null || runXpPanel == null || runLevelText == null)
                Debug.LogWarning($"{nameof(ProgressionHUD)}: Run XP UI references are not fully assigned.", this);
        }

        private void Update()
        {
            RunProgression run = RunProgression.Instance;
            PermanentProgression permanent = PermanentProgression.Instance;
            if (run == null || permanent == null)
                return;

            int runXp = run.RunXp;
            int permanentXp = permanent.PermanentXp;
            int survivorLevel = permanent.SurvivorLevel;

            // Nothing changed this frame: skip all UI work.
            if (runXp == _lastRunXp && permanentXp == _lastPermanentXp && survivorLevel == _lastSurvivorLevel)
                return;

            // Confirm the HUD noticed a Survivor (permanent) level change (skip the initial set).
            if (_lastSurvivorLevel != -1 && survivorLevel != _lastSurvivorLevel)
                Debug.Log($"[HUD] Survivor Level changed: {_lastSurvivorLevel} -> {survivorLevel}.");

            _lastRunXp = runXp;
            _lastPermanentXp = permanentXp;
            _lastSurvivorLevel = survivorLevel;

            RefreshLevel(survivorLevel);
            RefreshPermanentXp(permanent); // TOP bar - permanent only
            RefreshRunXp(run);             // BOTTOM bar - run only
        }

        // The prominent level number comes from PERMANENT progression (survives death).
        private void RefreshLevel(int survivorLevel)
        {
            if (levelText != null)
                levelText.text = $"LEVEL {survivorLevel}";
        }

        // Permanent XP bar/text are ALWAYS visible and read ONLY from PermanentProgression.
        private void RefreshPermanentXp(PermanentProgression permanent)
        {
            UpdateXpDisplay(permanent.PermanentXp, permanent.CurrentLevelXp, permanent.NextLevelXp,
                permanentXpFillImage, permanentXpText);
        }

        // Run XP bar/text/level read ONLY from RunProgression, and the whole run panel is hidden
        // whenever Run XP is 0.
        private void RefreshRunXp(RunProgression run)
        {
            bool runVisible = run.RunXp > 0;

            if (runXpPanel != null)
                runXpPanel.SetActive(runVisible);

            // Run Level is separate from the permanent Survivor Level shown by levelText.
            if (runLevelText != null)
                runLevelText.text = $"RUN LEVEL {run.RunLevel}";

            UpdateXpDisplay(run.RunXp, run.CurrentLevelXp, run.NextLevelXp, runXpFillImage, runXpText);
        }

        /// <summary>
        /// Shared display helper: renders within-level progress as a fill (0..1) and "X / Y XP" text,
        /// where X is XP into the current level and Y is the XP span of that level. Shows a full bar
        /// and "MAX" once the highest defined level is reached.
        /// </summary>
        private void UpdateXpDisplay(int totalXp, int currentLevelXp, int nextLevelXp,
            Image fillImage, TextMeshProUGUI text)
        {
            int xpIntoLevel = totalXp - currentLevelXp;
            int xpSpan = nextLevelXp - currentLevelXp;

            if (xpSpan > 0)
            {
                if (fillImage != null)
                    fillImage.fillAmount = Mathf.Clamp01((float)xpIntoLevel / xpSpan);
                if (text != null)
                    text.text = $"{xpIntoLevel} / {xpSpan} XP";
            }
            else
            {
                if (fillImage != null)
                    fillImage.fillAmount = 1f;
                if (text != null)
                    text.text = "MAX";
            }
        }

        private void EnsureFilled(Image image, string fieldName)
        {
            if (image == null)
                return;

            if (image.type != Image.Type.Filled)
            {
                Debug.LogWarning($"{nameof(ProgressionHUD)}: '{fieldName}' was not set to Image Type = Filled; " +
                                 "correcting at runtime so the bar can fill. Please also set it on the asset.", image);
                image.type = Image.Type.Filled;
            }
        }
    }
}
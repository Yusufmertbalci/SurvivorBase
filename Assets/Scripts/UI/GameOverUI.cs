using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI
{
    /// <summary>
    /// Placeholder Game Over UI. Shows/hides a panel (with a "GAME OVER" label and a "Return to Base"
    /// button set up in the scene) and exposes a button handler. Deliberately view-only - it holds no
    /// game logic, so the run/game-state code stays in PlayerDeathHandler.
    ///
    /// No UI package references are needed here: the panel content is built in the Editor, and this
    /// script only toggles the panel and receives the button's OnClick.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Tooltip("Root object of the Game Over panel. Hidden on start, shown on death.")]
        [SerializeField] private GameObject panel;

        [Tooltip("Name of the Base scene to load on Return to Base. Must be in Build Settings.")]
        [SerializeField] private string baseSceneName = "BaseScene";

        private void Awake()
        {
            // Ensure the panel starts hidden regardless of how it was left in the scene.
            if (panel != null)
                panel.SetActive(false);
        }

        /// <summary>Shows the Game Over panel.</summary>
        public void Show()
        {
            if (panel != null)
                panel.SetActive(true);
        }

        /// <summary>Hides the Game Over panel.</summary>
        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        /// <summary>
        /// Wire the "Return to Base" button's OnClick to this method in the Inspector (name unchanged).
        ///
        /// Loads the Base scene. The scene-scoped RunProgression, DifficultyManager, Player, enemies,
        /// and XP crystals are all destroyed with GameScene, so the next run starts fresh, while the
        /// DontDestroyOnLoad PermanentProgression survives - Permanent XP / Survivor Level are kept.
        /// </summary>
        public void OnReturnToBasePressed()
        {
            // Restore normal time in case the game was paused (e.g. by the upgrade screen).
            Time.timeScale = 1f;

            SceneManager.LoadScene(baseSceneName);
        }
    }
}

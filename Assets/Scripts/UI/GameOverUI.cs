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
        /// Wire the "Return to Base" button's OnClick to this method in the Inspector.
        ///
        /// Placeholder for testing: the real Base system does not exist yet, so this reloads the
        /// current scene to start a fresh run. Reloading gives us the correct behavior for free -
        /// the scene-scoped RunProgression and this Game Over UI are recreated fresh, while the
        /// DontDestroyOnLoad PermanentProgression survives, so Permanent XP / Survivor Level are NOT
        /// reset. Later this method will load the actual Base scene instead.
        /// </summary>
        public void OnReturnToBasePressed()
        {
            Debug.Log("Return to Base pressed (placeholder - reloading current scene for a fresh run).");

            // Restore normal time in case the game was paused (e.g. a future Time.timeScale = 0 pause).
            Time.timeScale = 1f;

            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
using UnityEngine;
using Game.UI; // GameOverUI lives here
using Game.Progression; // RunProgression

namespace Game.Player
{
    /// <summary>
    /// Reacts to the player's death by ending the current run: it stops the player's gameplay
    /// components and shows the Game Over UI. This is the run/game-state layer - kept separate from
    /// PlayerHealth (which only detects death) and from GameOverUI (which only displays).
    ///
    /// Attach this to the Player alongside PlayerHealth. Sibling gameplay components (movement,
    /// auto-attack, ...) are found via GetComponent and switched off, so their own scripts are not
    /// modified.
    /// </summary>
    public class PlayerDeathHandler : MonoBehaviour
    {
        [Tooltip("The Game Over UI shown when the run ends.")]
        [SerializeField] private GameOverUI gameOverUI;

        private PlayerHealth _playerHealth;
        private PlayerMovement _movement;
        private PlayerAutoAttack _autoAttack;

        private void Awake()
        {
            _playerHealth = GetComponent<PlayerHealth>();
            _movement = GetComponent<PlayerMovement>();
            _autoAttack = GetComponent<PlayerAutoAttack>();
        }

        private void OnEnable()
        {
            if (_playerHealth != null)
                _playerHealth.Died += HandleDeath;
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
                _playerHealth.Died -= HandleDeath;
        }

        private void HandleDeath()
        {
            // Stop the player from acting. Disabling the components halts their Update loops without
            // touching their code, so no residual movement or attacks occur.
            if (_movement != null)
                _movement.enabled = false;
            if (_autoAttack != null)
                _autoAttack.enabled = false;

            // Reset TEMPORARY run progression (Run XP / Run Level) so the next run starts fresh.
            // Permanent progression (Permanent XP / Survivor Level) and future Base progression are
            // intentionally NOT touched here - they survive death.
            if (RunProgression.Instance != null)
                RunProgression.Instance.ResetRun();

            // Show the Game Over screen.
            if (gameOverUI != null)
                gameOverUI.Show();
            else
                Debug.LogWarning($"{nameof(PlayerDeathHandler)}: No GameOverUI assigned.", this);
        }
    }
}
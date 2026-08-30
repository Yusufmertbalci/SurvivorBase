using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Base;   // ResourceType, BaseResourceManager
using Game.Player; // PlayerHealth

namespace Game.Resources
{
    /// <summary>
    /// Handles a SUCCESSFUL return to base (the "extract" action) - deliberately separate from the
    /// death -> Game Over -> Return flow (GameOverUI), which loads BaseScene WITHOUT depositing so run
    /// resources are lost.
    ///
    /// On a successful return it deposits the RunResourceInventory into the persistent
    /// BaseResourceManager and THEN loads BaseScene. Depositing straight into the persistent manager
    /// before the scene changes means no carrier object and no scene-search are needed. A guard also
    /// refuses to deposit if the player is already dead, so death never deposits by accident.
    /// </summary>
    public class RunEndController : MonoBehaviour
    {
        [Tooltip("Scene to load on a successful return. Must be in Build Settings.")]
        [SerializeField] private string baseSceneName = "BaseScene";

        [Tooltip("Optional - guards against depositing after death (assign the Player's PlayerHealth).")]
        [SerializeField] private PlayerHealth playerHealth;

        /// <summary>
        /// Wire the "Return to Base" (extract) button's OnClick here. Deposits run resources into the
        /// base stockpile and returns to BaseScene. Does nothing if the player is dead.
        /// </summary>
        public void ReturnToBaseSuccessfully()
        {
            // Death loses run resources (that path goes through Game Over, not here).
            if (playerHealth != null && playerHealth.IsDead)
                return;

            DepositRunResources();

            Time.timeScale = 1f;
            SceneManager.LoadScene(baseSceneName);
        }

        private void DepositRunResources()
        {
            RunResourceInventory run = RunResourceInventory.Instance;
            BaseResourceManager basePool = BaseResourceManager.Instance;
            if (run == null || basePool == null)
                return;

            // Deposit every resource type. Future ResourceType values transfer automatically.
            // (Runs once per successful return, so the Enum.GetValues allocation is not a hot path.)
            foreach (ResourceType type in (ResourceType[])Enum.GetValues(typeof(ResourceType)))
                basePool.AddResources(type, run.Get(type));

            run.Clear();
        }
    }
}
using UnityEngine;
using TMPro;
using Game.Player;

namespace Game.UI
{
    /// <summary>
    /// Display-only HUD for the player's CURRENT combat stats: HP, Damage, Attack Speed. It reads the
    /// live values straight from the existing PlayerHealth and PlayerAutoAttack components (the single
    /// source of truth) and refreshes the text only when a value actually changes.
    ///
    /// It stores no stats, applies no upgrades, and never modifies the player - it only reflects what
    /// those components already hold, so run upgrades stay temporary and reset with the Player on a
    /// new run automatically.
    /// </summary>
    public class PlayerStatsHUD : MonoBehaviour
    {
        [Header("Sources (existing player components)")]
        [Tooltip("Player's health component - source of HP (Current/Max).")]
        [SerializeField] private PlayerHealth playerHealth;

        [Tooltip("Player's auto-attack component - source of Damage and Attack Speed.")]
        [SerializeField] private PlayerAutoAttack playerAutoAttack;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI attackSpeedText;

        // Last-shown values, so text (and its string allocation) is only rebuilt when something changes.
        private int _lastCurrentHealth = int.MinValue;
        private int _lastMaxHealth = int.MinValue;
        private float _lastDamage = float.NaN;
        private float _lastAttackSpeed = float.NaN;

        private void Start()
        {
            if (playerHealth == null || playerAutoAttack == null)
                Debug.LogWarning($"{nameof(PlayerStatsHUD)}: Player references are not fully assigned; stats won't display.", this);
            if (hpText == null || damageText == null || attackSpeedText == null)
                Debug.LogWarning($"{nameof(PlayerStatsHUD)}: One or more stat text references are not assigned.", this);
        }

        private void Update()
        {
            if (playerHealth == null || playerAutoAttack == null)
                return;

            int currentHealth = playerHealth.CurrentHealth;
            int maxHealth = playerHealth.MaxHealth;
            float damage = playerAutoAttack.Damage;
            float attackSpeed = playerAutoAttack.AttackSpeedMultiplier;

            // HP: rebuild only when current or max changed.
            if (currentHealth != _lastCurrentHealth || maxHealth != _lastMaxHealth)
            {
                _lastCurrentHealth = currentHealth;
                _lastMaxHealth = maxHealth;
                if (hpText != null)
                    hpText.text = $"HP: {currentHealth} / {maxHealth}";
            }

            if (damage != _lastDamage)
            {
                _lastDamage = damage;
                if (damageText != null)
                    damageText.text = $"DAMAGE: {damage:0.##}";
            }

            if (attackSpeed != _lastAttackSpeed)
            {
                _lastAttackSpeed = attackSpeed;
                if (attackSpeedText != null)
                    attackSpeedText.text = $"ATTACK SPEED: {attackSpeed:0.00}x";
            }
        }
    }
}
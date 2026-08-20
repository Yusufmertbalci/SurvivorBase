using UnityEngine;

namespace Game.Difficulty
{
    /// <summary>
    /// Tracks the current run's survival time and turns it into difficulty values that the spawner
    /// reads: spawn interval, active-enemy cap, and HP/damage multipliers for newly spawned enemies.
    ///
    /// It owns ONLY difficulty math - it does not spawn enemies, award XP, or touch progression.
    /// Scene-scoped singleton (not DontDestroyOnLoad), so a fresh run/scene starts at survival time 0
    /// and base difficulty. Survival time accumulates with Time.deltaTime, which is 0 while the
    /// upgrade screen pauses the game (Time.timeScale = 0), so paused time does NOT count.
    ///
    /// Interval and enemy cap ramp from start to their limits over rampDuration (shaped by an
    /// AnimationCurve) and then hold. HP and damage multipliers keep growing linearly past the ramp
    /// (+growth per rampDuration) so enemies continue getting stronger.
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        [Header("Spawn Interval (seconds)")]
        [Tooltip("Spawn interval at the start of a run.")]
        [SerializeField] private float startingSpawnInterval = 2f;
        [Tooltip("Fastest spawn interval (interval never goes below this).")]
        [SerializeField] private float minSpawnInterval = 0.5f;

        [Header("Active Enemy Cap")]
        [Tooltip("Max simultaneous enemies at the start of a run.")]
        [SerializeField] private int startingMaxActiveEnemies = 5;
        [Tooltip("Hard upper limit on simultaneous enemies (never exceeded).")]
        [SerializeField] private int maximumMaxActiveEnemies = 12;

        [Header("Enemy HP Multiplier")]
        [Tooltip("HP multiplier at the start of a run.")]
        [SerializeField] private float hpMultiplierStart = 1f;
        [Tooltip("HP multiplier added per full ramp duration (keeps growing over time).")]
        [SerializeField] private float hpMultiplierGrowth = 0.75f;

        [Header("Enemy Damage Multiplier")]
        [Tooltip("Damage multiplier at the start of a run.")]
        [SerializeField] private float damageMultiplierStart = 1f;
        [Tooltip("Damage multiplier added per full ramp duration (keeps growing over time).")]
        [SerializeField] private float damageMultiplierGrowth = 0.5f;

        [Header("Ramp")]
        [Tooltip("Seconds of survival for interval/cap to ramp from start to their limits.")]
        [SerializeField] private float rampDuration = 300f;
        [Tooltip("Shapes interval/cap progress vs normalized survival time (X: 0..1 time, Y: 0..1 progress).")]
        [SerializeField] private AnimationCurve difficultyCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private float _survivalTime;

        /// <summary>Seconds of actual (unpaused) gameplay survived this run.</summary>
        public float SurvivalTime => _survivalTime;

        // 0..1 progress for the bounded values (interval, cap), shaped by the curve.
        private float BoundedProgress
        {
            get
            {
                float t = rampDuration > 0f ? Mathf.Clamp01(_survivalTime / rampDuration) : 1f;
                return Mathf.Clamp01(difficultyCurve.Evaluate(t));
            }
        }

        // Unclamped "ramps elapsed" for stats that keep growing past the ramp (HP, damage).
        private float RampsElapsed => rampDuration > 0f ? _survivalTime / rampDuration : 1f;

        public float CurrentSpawnInterval =>
            Mathf.Max(minSpawnInterval, Mathf.Lerp(startingSpawnInterval, minSpawnInterval, BoundedProgress));

        public int CurrentMaxActiveEnemies =>
            Mathf.RoundToInt(Mathf.Lerp(startingMaxActiveEnemies, maximumMaxActiveEnemies, BoundedProgress));

        public float CurrentHpMultiplier => hpMultiplierStart + hpMultiplierGrowth * RampsElapsed;

        public float CurrentDamageMultiplier => damageMultiplierStart + damageMultiplierGrowth * RampsElapsed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            // Scaled deltaTime: excludes the Time.timeScale = 0 upgrade pause. Resets to 0 on reload.
            _survivalTime += Time.deltaTime;
        }
    }
}
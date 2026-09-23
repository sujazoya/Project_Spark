using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Persistent configuration asset for a Project Spark level.
    ///
    /// IMPORTANT:
    /// This ScriptableObject must remain scene-independent.
    /// Scene objects are referenced through IDs and resolved at runtime by
    /// SparkLevelSceneBindings.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkLevelDefinition",
        menuName = "Project Spark/Level Definition",
        order = 10)]
    public sealed class SparkLevelDefinition : ScriptableObject
    {
        // ================================================================
        // ENUMS
        // ================================================================

        public enum CompletionMode
        {
            AllTargets = 0,
            AnyTarget = 1,
            RequiredTargetCount = 2
        }

        public enum LevelFailureMode
        {
            None = 0,
            ShortCircuit = 1,
            Overload = 2,
            InvalidConnection = 3
        }

        // ================================================================
        // IDENTITY
        // ================================================================

        [Header("Identity")]

        [SerializeField]
        private string levelId = "Level_01";

        [SerializeField]
        private string displayName = "Level 01";

        [TextArea(2, 5)]
        [SerializeField]
        private string description;

        // ================================================================
        // LEVEL RULES
        // ================================================================

        [Header("Level Rules")]

        [SerializeField]
        private CompletionMode completionMode =
            CompletionMode.AllTargets;

        [Min(1)]
        [SerializeField]
        private int requiredTargetCount = 1;

        [SerializeField]
        private bool requireClosedReturn = true;

        [SerializeField]
        private bool rejectShortCircuit = true;

        [SerializeField]
        private bool rejectTargetShort = true;

        [SerializeField]
        private bool allowIntermediateConnections = true;

        // ================================================================
        // POWER
        // ================================================================

        [Header("Power")]

        [SerializeField]
        private bool requireMinimumVoltage = true;

        [Min(0f)]
        [SerializeField]
        private float minimumVoltage = 1f;

        [SerializeField]
        private bool allowAnyConfiguredPowerSource = true;

        // ================================================================
        // POWER SOURCES
        // ================================================================

        [Header("Power Sources")]

        [SerializeField]
        private PowerSourceDefinition[] powerSources =
            Array.Empty<PowerSourceDefinition>();

        // ================================================================
        // OBJECTIVES
        // ================================================================

        [Header("Objectives")]

        [SerializeField]
        private SparkLevelTarget[] targets =
            Array.Empty<SparkLevelTarget>();

        // ================================================================
        // FAILURE
        // ================================================================

        [Header("Failure")]

        [SerializeField]
        private LevelFailureMode failureMode =
            LevelFailureMode.None;

        // ================================================================
        // VISUAL OUTPUT IDs
        // ================================================================

        [Header("Visual Outputs")]

        [Tooltip(
            "Scene binding IDs used when the level completes successfully.")]
        [SerializeField]
        private string[] successOutputIds =
            Array.Empty<string>();

        [Tooltip(
            "Scene binding IDs used when the level fails.")]
        [SerializeField]
        private string[] failureOutputIds =
            Array.Empty<string>();

        // ================================================================
        // PROGRESSION
        // ================================================================

        [Header("Progression")]

        [SerializeField]
        private bool unlockNextLevelOnCompletion = true;      



        // ================================================================
// PROGRESSION
// ================================================================

[Header("Progression")]

[Tooltip(
    "If enabled, this level starts unlocked when progression initializes.")]
[SerializeField]
private bool unlockedByDefault;

[Tooltip(
    "If enabled, completing this level automatically unlocks the next level.")]
[SerializeField]
private bool autoUnlockNextLevel = true;

[Tooltip(
    "If enabled, the gameplay manager automatically starts the next level " +
    "after this level is completed.")]
[SerializeField]
private bool autoAdvanceOnCompletion;




        

        // ================================================================
        // PUBLIC PROPERTIES
        // ================================================================

        public string LevelId => levelId;

        public string DisplayName => displayName;

        public string Description => description;

        public CompletionMode CompletionRule => completionMode;

        public CompletionMode CompletionModeValue => completionMode;

        public int RequiredTargetCount =>
            Mathf.Max(1, requiredTargetCount);

        public bool RequireClosedReturn => requireClosedReturn;

        public bool RejectShortCircuit => rejectShortCircuit;

        public bool RejectTargetShort => rejectTargetShort;

        public bool AllowIntermediateConnections =>
            allowIntermediateConnections;

        public bool RequireMinimumVoltage =>
            requireMinimumVoltage;

            public float MinimumVoltage =>
            Mathf.Max(0f, minimumVoltage);

            public bool UnlockedByDefault =>
            unlockedByDefault;

        public bool AutoUnlockNextLevel =>
            autoUnlockNextLevel;

        public bool AllowAnyConfiguredPowerSource =>
            allowAnyConfiguredPowerSource;

        public PowerSourceDefinition[] PowerSources =>
            powerSources;

        public SparkLevelTarget[] Targets =>
            targets;

        public LevelFailureMode FailureMode =>
            failureMode;

        public string[] SuccessOutputIds =>
            successOutputIds;

        public string[] FailureOutputIds =>
            failureOutputIds;

        public bool UnlockNextLevelOnCompletion =>
            unlockNextLevelOnCompletion;

        public bool AutoAdvanceOnCompletion =>
            autoAdvanceOnCompletion;

        // ================================================================
        // CONFIGURATION HELPERS
        // ================================================================

        public int TargetCount
        {
            get
            {
                return targets != null
                    ? targets.Length
                    : 0;
            }
        }

        public int SourceCount =>
        powerSources != null ? powerSources.Length : 0;
        public int PowerSourceCount
        {
            get
            {
                return powerSources != null
                    ? powerSources.Length
                    : 0;
            }
        }

        public bool HasTargets =>
            targets != null &&
            targets.Length > 0;

        public bool HasPowerSources =>
            powerSources != null &&
            powerSources.Length > 0;

        // ================================================================
        // COMPLETION LOGIC
        // ================================================================

        /// <summary>
        /// Determines whether the supplied number of satisfied targets
        /// fulfills this level's configured completion rule.
        /// </summary>
        public bool IsCompletionSatisfied(int satisfiedTargetCount)
        {
            int targetCount = TargetCount;

            if (targetCount <= 0)
                return false;

            satisfiedTargetCount =
                Mathf.Clamp(
                    satisfiedTargetCount,
                    0,
                    targetCount);

            switch (completionMode)
            {
                case CompletionMode.AllTargets:
                    return satisfiedTargetCount >= targetCount;

                case CompletionMode.AnyTarget:
                    return satisfiedTargetCount >= 1;

                case CompletionMode.RequiredTargetCount:
                    return satisfiedTargetCount >=
                           Mathf.Clamp(
                               requiredTargetCount,
                               1,
                               targetCount);

                default:
                    return false;
            }
        }

        // ================================================================
        // VALIDATION
        // ================================================================

        /// <summary>
        /// Validates the persistent asset configuration.
        ///
        /// This method does not resolve scene references.
        /// Scene references are validated later through
        /// SparkLevelSceneBindings.
        /// </summary>
        public bool Validate(out string error)
        {
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(levelId))
            {
                error = "Level ID is empty.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                error = $"Level '{levelId}' has an empty display name.";
                return false;
            }

            if (powerSources == null ||
                powerSources.Length == 0)
            {
                error =
                    $"Level '{levelId}' has no configured power sources.";

                return false;
            }

            for (int i = 0; i < powerSources.Length; i++)
            {
                PowerSourceDefinition source = powerSources[i];

                if (source == null)
                {
                    error =
                        $"Level '{levelId}' has a null power source " +
                        $"at index {i}.";

                    return false;
                }

                if (!source.IsConfigured)
                {
                    error =
                        $"Level '{levelId}' has an invalid power source " +
                        $"at index {i}.";

                    return false;
                }
            }

            if (targets == null ||
                targets.Length == 0)
            {
                error =
                    $"Level '{levelId}' has no configured targets.";

                return false;
            }

            for (int i = 0; i < targets.Length; i++)
            {
                SparkLevelTarget target = targets[i];

                if (target == null)
                {
                    error =
                        $"Level '{levelId}' has a null target " +
                        $"at index {i}.";

                    return false;
                }

                if (!target.IsConfigured())
                {
                    error =
                        $"Level '{levelId}' has an invalid target " +
                        $"at index {i}.";

                    return false;
                }

                if (!target.IsPolarityConfigurationValid())
                {
                    error =
                        $"Level '{levelId}' has an invalid polarity " +
                        $"configuration for target at index {i}.";

                    return false;
                }
            }

            if (completionMode ==
                CompletionMode.RequiredTargetCount)
            {
                if (requiredTargetCount < 1)
                {
                    error =
                        $"Level '{levelId}' requires at least " +
                        $"one required target.";

                    return false;
                }

                if (requiredTargetCount > targets.Length)
                {
                    error =
                        $"Level '{levelId}' requires " +
                        $"{requiredTargetCount} targets, but only " +
                        $"{targets.Length} are configured.";

                    return false;
                }
            }

            return true;
        }

        // ================================================================
        // EDITOR NORMALIZATION
        // ================================================================

#if UNITY_EDITOR

        private void OnValidate()
        {
            SanitizeEditorData();
        }

        /// <summary>
        /// Keeps serialized asset values sane while editing.
        ///
        /// Runtime evaluation should not modify the ScriptableObject.
        /// </summary>
        private void SanitizeEditorData()
        {
            requiredTargetCount =
                Mathf.Max(1, requiredTargetCount);

            minimumVoltage =
                Mathf.Max(0f, minimumVoltage);

            if (powerSources == null)
                powerSources = Array.Empty<PowerSourceDefinition>();

            if (targets == null)
                targets = Array.Empty<SparkLevelTarget>();

            if (successOutputIds == null)
                successOutputIds = Array.Empty<string>();

            if (failureOutputIds == null)
                failureOutputIds = Array.Empty<string>();

            if (powerSources != null)
            {
                for (int i = 0; i < powerSources.Length; i++)
                {
                    if (powerSources[i] != null)
                        powerSources[i].Normalize();
                }
            }

            if (targets != null)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] != null)
                        targets[i].Normalize();
                }
            }
        }

#endif

        // ================================================================
        // POWER SOURCE DEFINITION
        // ================================================================

        [Serializable]
        public sealed class PowerSourceDefinition
        {
            [SerializeField]
            private string sourceName = "Power Source";

            [Tooltip(
                "ID of the positive SparkTerminal in " +
                "SparkLevelSceneBindings.")]
            [SerializeField]
            private string positiveTerminalId;

            [Tooltip(
                "ID of the negative SparkTerminal in " +
                "SparkLevelSceneBindings.")]
            [SerializeField]
            private string negativeTerminalId;

            [Min(0f)]
            [SerializeField]
            private float nominalVoltage = 5f;

            // ------------------------------------------------------------
            // PROPERTIES
            // ------------------------------------------------------------

            public string SourceName => sourceName;

            public string PositiveTerminalId =>
                positiveTerminalId;

            public string NegativeTerminalId =>
                negativeTerminalId;

            public float NominalVoltage =>
                Mathf.Max(0f, nominalVoltage);

            /// <summary>
            /// Checks whether the source contains the IDs required to
            /// resolve its two electrical terminals.
            /// </summary>
            public bool IsConfigured
            {
                get
                {
                    return
                        !string.IsNullOrWhiteSpace(
                            positiveTerminalId) &&
                        !string.IsNullOrWhiteSpace(
                            negativeTerminalId);
                }
            }

            // ------------------------------------------------------------
            // EDITOR NORMALIZATION
            // ------------------------------------------------------------

            public void Normalize()
            {
                if (string.IsNullOrWhiteSpace(sourceName))
                    sourceName = "Power Source";

                nominalVoltage =
                    Mathf.Max(0f, nominalVoltage);
            }
        }
    }
}
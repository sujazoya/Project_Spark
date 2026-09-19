using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Serializable definition of one playable Project Spark level.
    /// Scene references are intentionally stored here so levels can directly
    /// reference terminals/components in the current scene.
    /// </summary>
    [Serializable]
    public sealed class SparkLevelDefinition
    {
        public enum CompletionMode
        {
            AllTargets,
            AnyTarget,
            RequiredTargetCount
        }

        public enum LevelFailureMode
        {
            None,
            ShortCircuit,
            Overload,
            InvalidConnection
        }

        [Header("Identity")]
        [SerializeField] private string levelId = "LEVEL_01";
        [SerializeField] private string displayName = "Level 01";
        [TextArea(2, 5)]
        [SerializeField] private string description;

        [Header("Level Rules")]
        [SerializeField] private CompletionMode completionMode =
            CompletionMode.AllTargets;

        [Min(1)]
        [SerializeField] private int requiredTargetCount = 1;

        [SerializeField] private bool requireClosedReturn = true;
        [SerializeField] private bool rejectShortCircuit = true;
        [SerializeField] private bool rejectTargetShort = true;
        [SerializeField] private bool allowIntermediateConnections = true;

        [Header("Power")]
        [Min(0f)]
        [SerializeField] private float minimumVoltage = 0.01f;

        [SerializeField] private bool allowAnyConfiguredSource = true;

        [Header("Power Sources")]
        [SerializeField] private PowerSourceDefinition[] powerSources;

        [Header("Objectives")]
        [SerializeField] private SparkLevelTarget[] targets;

        [Header("Failure")]
        [SerializeField] private LevelFailureMode failureMode =
            LevelFailureMode.ShortCircuit;

        [Header("Visual Outputs")]
        [SerializeField] private GameObject[] successOutputs;
        [SerializeField] private GameObject[] failureOutputs;

        [Header("Progression")]
        [SerializeField] private bool unlockedByDefault = true;

        [SerializeField] private bool autoUnlockNextLevel = true;

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public string Description => description;

        public CompletionMode CompletionRule => completionMode;

        public int RequiredTargetCount =>
            Mathf.Max(1, requiredTargetCount);

        public bool RequireClosedReturn => requireClosedReturn;
        public bool RejectShortCircuit => rejectShortCircuit;
        public bool RejectTargetShort => rejectTargetShort;
        public bool AllowIntermediateConnections => allowIntermediateConnections;

        public float MinimumVoltage => Mathf.Max(0f, minimumVoltage);

        public bool AllowAnyConfiguredSource => allowAnyConfiguredSource;

        public PowerSourceDefinition[] PowerSources =>
            powerSources;

        public SparkLevelTarget[] Targets =>
            targets;

        public LevelFailureMode FailureMode =>
            failureMode;

        public GameObject[] SuccessOutputs =>
            successOutputs;

        public GameObject[] FailureOutputs =>
            failureOutputs;

        public bool UnlockedByDefault =>
            unlockedByDefault;

        public bool AutoUnlockNextLevel =>
            autoUnlockNextLevel;

        [Serializable]
        public sealed class PowerSourceDefinition
        {
            [SerializeField] private string sourceName = "Power Source";

            [SerializeField] private SparkTerminal positiveTerminal;
            [SerializeField] private SparkTerminal negativeTerminal;

            [Min(0f)]
            [SerializeField] private float nominalVoltage = 5f;

            public string SourceName => sourceName;

            public SparkTerminal PositiveTerminal =>
                positiveTerminal;

            public SparkTerminal NegativeTerminal =>
                negativeTerminal;

            public float NominalVoltage =>
                Mathf.Max(0f, nominalVoltage);

            public bool IsConfigured =>
                positiveTerminal != null &&
                negativeTerminal != null;
        }

        public int TargetCount =>
            targets != null ? targets.Length : 0;

        public int SourceCount =>
            powerSources != null ? powerSources.Length : 0;

        public void SetRuntimeOutputs(bool success, bool failure)
        {
            SetObjects(successOutputs, success);
            SetObjects(failureOutputs, failure);
        }

        private static void SetObjects(
            GameObject[] objects,
            bool state)
        {
            if (objects == null)
                return;

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    objects[i].SetActive(state);
            }
        }

        public bool IsCompletionSatisfied(
            int satisfiedTargets)
        {
            int total = TargetCount;

            if (total <= 0)
                return false;

            switch (completionMode)
            {
                case CompletionMode.AnyTarget:
                    return satisfiedTargets > 0;

                case CompletionMode.RequiredTargetCount:
                    return satisfiedTargets >=
                           Mathf.Min(RequiredTargetCount, total);

                case CompletionMode.AllTargets:
                default:
                    return satisfiedTargets >= total;
            }
        }

        public void Normalize()
        {
            requiredTargetCount =
                Mathf.Max(1, requiredTargetCount);

            minimumVoltage =
                Mathf.Max(0f, minimumVoltage);

            if (string.IsNullOrWhiteSpace(levelId))
                levelId = "LEVEL";

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = levelId;
        }
    }
}
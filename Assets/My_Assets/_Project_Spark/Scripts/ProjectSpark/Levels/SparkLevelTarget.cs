using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Configuration for one level objective.
    ///
    /// IMPORTANT:
    /// This class is serialized inside SparkLevelDefinition.
    /// It must therefore remain scene-independent.
    ///
    /// Actual scene objects are resolved by SparkLevelSceneBindings
    /// using the IDs stored here.
    /// </summary>
    [Serializable]
    public sealed class SparkLevelTarget
    {
        // ================================================================
        // TARGET TYPE
        // ================================================================

        public enum TargetType
        {
            TerminalPowered = 0,
            VoltagePresent = 1,
            ComponentPowered = 2,
            LEDOn = 3,
            ComponentConducting = 4
        }

        // ================================================================
        // IDENTITY
        // ================================================================

        [Header("Identity")]

        [SerializeField]
        private string targetId = "Target_01";

        [SerializeField]
        private string displayName = "Target";

        [TextArea(2, 4)]
        [SerializeField]
        private string description;

        // ================================================================
        // TARGET TYPE
        // ================================================================

        [Header("Target")]

        [SerializeField]
        private TargetType targetType =
            TargetType.TerminalPowered;

        // ================================================================
        // SCENE BINDING IDs
        // ================================================================

        [Header("Scene Binding IDs")]

        [Tooltip(
            "ID of the primary target SparkTerminal in " +
            "SparkLevelSceneBindings.")]
        [SerializeField]
        private string targetTerminalId;

        [Tooltip(
            "ID of the required positive SparkTerminal in " +
            "SparkLevelSceneBindings.")]
        [SerializeField]
        private string requiredPositiveTerminalId;

        [Tooltip(
            "ID of the required negative SparkTerminal in " +
            "SparkLevelSceneBindings.")]
        [SerializeField]
        private string requiredNegativeTerminalId;

        [Tooltip(
            "ID of the target SparkElectricalComponent in " +
            "SparkLevelSceneBindings.")]
        [SerializeField]
        private string targetComponentId;

        // ================================================================
        // ELECTRICAL REQUIREMENTS
        // ================================================================

        [Header("Electrical Requirements")]

        [Min(0f)]
        [SerializeField]
        private float minimumVoltage;

        [Min(0f)]
        [SerializeField]
        private float minimumCurrent;

        [Min(0f)]
        [SerializeField]
        private float minimumPower;

        // ================================================================
        // BEHAVIOUR
        // ================================================================

        [Header("Behaviour")]

        [SerializeField]
        private bool requireElectricalEnabled = true;

        [SerializeField]
        private bool requireConduction;

        [SerializeField]
        private bool inverted;

        // ================================================================
        // PUBLIC PROPERTIES
        // ================================================================

        public string TargetId =>
            targetId;

        public string DisplayName =>
            displayName;

        public string Description =>
            description;

        public TargetType Type =>
            targetType;

        public TargetType TargetTypeValue =>
            targetType;

        // ---------------------------------------------------------------
        // SCENE IDs
        // ---------------------------------------------------------------

        public string TargetTerminalId =>
            targetTerminalId;

        public string RequiredPositiveTerminalId =>
            requiredPositiveTerminalId;

        public string RequiredNegativeTerminalId =>
            requiredNegativeTerminalId;

        public string TargetComponentId =>
            targetComponentId;

        // ---------------------------------------------------------------
        // ELECTRICAL REQUIREMENTS
        // ---------------------------------------------------------------

        public float MinimumVoltage =>
            Mathf.Max(0f, minimumVoltage);

        public float MinimumCurrent =>
            Mathf.Max(0f, minimumCurrent);

        public float MinimumPower =>
            Mathf.Max(0f, minimumPower);

        // ---------------------------------------------------------------
        // BEHAVIOUR
        // ---------------------------------------------------------------

        public bool RequireElectricalEnabled =>
            requireElectricalEnabled;

        public bool RequireConduction =>
            requireConduction;

        public bool Inverted =>
            inverted;

        // ================================================================
        // POLARITY CONFIGURATION
        // ================================================================

        /// <summary>
        /// Returns true when both required polarity terminals are configured.
        /// </summary>
        public bool HasRequiredConnectionPair
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(
                        requiredPositiveTerminalId) &&
                    !string.IsNullOrWhiteSpace(
                        requiredNegativeTerminalId);
            }
        }

        /// <summary>
        /// Returns true when neither required polarity terminal is configured.
        /// </summary>
        public bool HasNoRequiredConnectionPair
        {
            get
            {
                return
                    string.IsNullOrWhiteSpace(
                        requiredPositiveTerminalId) &&
                    string.IsNullOrWhiteSpace(
                        requiredNegativeTerminalId);
            }
        }

        /// <summary>
        /// Returns true when only one side of the required polarity pair
        /// has been configured.
        /// </summary>
        public bool HasPartialRequiredConnectionPair
        {
            get
            {
                bool positiveConfigured =
                    !string.IsNullOrWhiteSpace(
                        requiredPositiveTerminalId);

                bool negativeConfigured =
                    !string.IsNullOrWhiteSpace(
                        requiredNegativeTerminalId);

                return positiveConfigured != negativeConfigured;
            }
        }

        // ================================================================
        // CONFIGURATION VALIDATION
        // ================================================================

        /// <summary>
        /// Checks whether this target has enough configuration to be
        /// evaluated.
        ///
        /// Scene references themselves are NOT resolved here.
        /// </summary>
        public bool IsConfigured()
        {
            bool hasTerminal =
                !string.IsNullOrWhiteSpace(
                    targetTerminalId);

            bool hasComponent =
                !string.IsNullOrWhiteSpace(
                    targetComponentId);

            switch (targetType)
            {
                case TargetType.TerminalPowered:
                case TargetType.VoltagePresent:
                    return hasTerminal;

                case TargetType.ComponentPowered:
                case TargetType.LEDOn:
                case TargetType.ComponentConducting:
                    return hasComponent;

                default:
                    return hasTerminal || hasComponent;
            }
        }

        /// <summary>
        /// Validates the polarity configuration.
        ///
        /// A polarity pair must either be completely configured or
        /// completely absent. A single positive or negative ID is invalid.
        /// </summary>
        public bool IsPolarityConfigurationValid()
        {
            return !HasPartialRequiredConnectionPair;
        }

        // ================================================================
        // NORMALIZATION
        // ================================================================

        /// <summary>
        /// Sanitizes serialized configuration values.
        ///
        /// This method intentionally does NOT resolve or modify scene
        /// references because this object belongs to a ScriptableObject asset.
        /// </summary>
        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(targetId))
                targetId = "Target";

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = targetId;

            minimumVoltage =
                Mathf.Max(0f, minimumVoltage);

            minimumCurrent =
                Mathf.Max(0f, minimumCurrent);

            minimumPower =
                Mathf.Max(0f, minimumPower);

            targetTerminalId =
                NormalizeId(targetTerminalId);

            requiredPositiveTerminalId =
                NormalizeId(requiredPositiveTerminalId);

            requiredNegativeTerminalId =
                NormalizeId(requiredNegativeTerminalId);

            targetComponentId =
                NormalizeId(targetComponentId);
        }

        // ================================================================
        // ID NORMALIZATION
        // ================================================================

        private static string NormalizeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim();
        }

#if UNITY_EDITOR

        // ================================================================
        // EDITOR VALIDATION
        // ================================================================

        private void OnValidate()
        {
            Normalize();
        }

#endif
    }
}
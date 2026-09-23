
using System;
using UnityEngine;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Defines one objective that a level evaluator can validate.
    ///
    /// A target may use:
    ///     - a single terminal
    ///     - a positive/negative terminal pair
    ///     - an electrical component
    ///
    /// RequiredPositiveTerminal / RequiredNegativeTerminal are the
    /// authoritative polarity terminals for targets that require a
    /// correctly wired circuit.
    /// </summary>
    [Serializable]
    public sealed class SparkLevelTarget
    {
        // ================================================================
        // TARGET TYPE
        // ================================================================

        public enum TargetType
        {
            TerminalPowered,
            VoltagePresent,
            ComponentPowered,
            LEDOn,
            ComponentConducting
        }


        // ================================================================
        // IDENTITY
        // ================================================================

        [Header("Identity")]

        [SerializeField]
        private string targetId = "TARGET_01";

        [SerializeField]
        private string displayName = "Power Target";

        [TextArea(1, 4)]
        [SerializeField]
        private string description;


        // ================================================================
        // TARGET TYPE
        // ================================================================

        [Header("Target Type")]

        [SerializeField]
        private TargetType targetType =
            TargetType.TerminalPowered;


        // ================================================================
        // TARGET CONNECTION
        // ================================================================

        [Header("Target Connection")]

        [Tooltip(
            "Optional single terminal used by TerminalPowered and " +
            "VoltagePresent target types.")]
        [SerializeField]
        private SparkTerminal targetTerminal;

        [Tooltip(
            "Terminal that must receive the SOURCE POSITIVE (+) side " +
            "of the circuit. For an LED this is normally the anode.")]
        [SerializeField]
        private SparkTerminal requiredPositiveTerminal;

        [Tooltip(
            "Terminal that must receive the SOURCE NEGATIVE (-) side " +
            "of the circuit. For an LED this is normally the cathode.")]
        [SerializeField]
        private SparkTerminal requiredNegativeTerminal;


        // ================================================================
        // COMPONENT TARGET
        // ================================================================

        [Header("Component Target")]

        [Tooltip(
            "Electrical component evaluated when the target type " +
            "uses component-based validation.")]
        [SerializeField]
        private SparkElectricalComponent targetComponent;


        // ================================================================
        // ELECTRICAL REQUIREMENTS
        // ================================================================

        [Header("Electrical Requirements")]

        [Tooltip(
            "Minimum voltage required for this target.")]
        [Min(0f)]
        [SerializeField]
        private float minimumVoltage = 0.01f;

        [Tooltip(
            "Minimum current required for this target.")]
        [Min(0f)]
        [SerializeField]
        private float minimumCurrent = 0f;

        [Tooltip(
            "Minimum power required for this target.")]
        [Min(0f)]
        [SerializeField]
        private float minimumPower = 0f;


        // ================================================================
        // BEHAVIOR
        // ================================================================

        [Header("Behavior")]

        [Tooltip(
            "When enabled, the terminal/component must be electrically enabled.")]
        [SerializeField]
        private bool requireElectricalEnabled = true;

        [Tooltip(
            "When enabled, a component target must be conducting.")]
        [SerializeField]
        private bool requireConduction;

        [Tooltip(
            "Inverts the final target result.")]
        [SerializeField]
        private bool inverted;


        // ================================================================
        // PUBLIC IDENTITY
        // ================================================================

        public string TargetId =>
            targetId;

        public string DisplayName =>
            displayName;

        public string Description =>
            description;


        // ================================================================
        // PUBLIC TARGET CONFIGURATION
        // ================================================================

        public TargetType Type =>
            targetType;


        // ================================================================
        // SINGLE TERMINAL
        // ================================================================

        /// <summary>
        /// Single-terminal target reference.
        /// Used by TerminalPowered and VoltagePresent.
        /// </summary>
        public SparkTerminal TargetTerminal =>
            targetTerminal;


        // ================================================================
        // POLARITY TERMINALS
        // ================================================================

        /// <summary>
        /// Terminal that must be connected to the source positive side.
        /// </summary>
        public SparkTerminal RequiredPositiveTerminal =>
            requiredPositiveTerminal;


        /// <summary>
        /// Terminal that must be connected to the source negative side.
        /// </summary>
        public SparkTerminal RequiredNegativeTerminal =>
            requiredNegativeTerminal;


        /// <summary>
        /// True when both polarity terminals have been configured.
        /// </summary>
        public bool HasRequiredConnectionPair =>
            requiredPositiveTerminal != null &&
            requiredNegativeTerminal != null;


        /// <summary>
        /// True when neither polarity terminal is configured.
        /// </summary>
        public bool HasNoRequiredConnectionPair =>
            requiredPositiveTerminal == null &&
            requiredNegativeTerminal == null;


        /// <summary>
        /// True when only one polarity terminal has been configured.
        /// </summary>
        public bool HasPartialRequiredConnectionPair =>
            (requiredPositiveTerminal != null) !=
            (requiredNegativeTerminal != null);


        // ================================================================
        // COMPONENT
        // ================================================================

        public SparkElectricalComponent TargetComponent =>
            targetComponent;


        // ================================================================
        // ELECTRICAL REQUIREMENTS
        // ================================================================

        public float MinimumVoltage =>
            Mathf.Max(0f, minimumVoltage);

        public float MinimumCurrent =>
            Mathf.Max(0f, minimumCurrent);

        public float MinimumPower =>
            Mathf.Max(0f, minimumPower);


        // ================================================================
        // BEHAVIOR
        // ================================================================

        public bool RequireElectricalEnabled =>
            requireElectricalEnabled;

        public bool RequireConduction =>
            requireConduction;

        public bool Inverted =>
            inverted;


        // ================================================================
        // VALIDATION
        // ================================================================

        /// <summary>
        /// Evaluates the configured target against its current electrical state.
        /// </summary>
        public bool Evaluate()
        {
            bool result;

            switch (targetType)
            {
                case TargetType.TerminalPowered:
                    result = EvaluateTerminalPowered();
                    break;

                case TargetType.VoltagePresent:
                    result = EvaluateVoltage();
                    break;

                case TargetType.ComponentPowered:
                    result = EvaluateComponentPowered();
                    break;

                case TargetType.LEDOn:
                    result = EvaluateLED();
                    break;

                case TargetType.ComponentConducting:
                    result = EvaluateComponentConducting();
                    break;

                default:
                    result = false;
                    break;
            }

            return inverted ? !result : result;
        }


        // ================================================================
        // TERMINAL POWERED
        // ================================================================

        private bool EvaluateTerminalPowered()
        {
            if (targetTerminal == null)
                return false;

            SparkTerminalElectricalState state =
                targetTerminal.ElectricalState;

            if (requireElectricalEnabled &&
                !targetTerminal.IsElectricalEnabled)
            {
                return false;
            }

            return state.IsPowered &&
                   state.Voltage >= MinimumVoltage &&
                   state.Current >= MinimumCurrent &&
                   state.Power >= MinimumPower;
        }


        // ================================================================
        // VOLTAGE PRESENT
        // ================================================================

        private bool EvaluateVoltage()
        {
            if (targetTerminal == null)
                return false;

            SparkTerminalElectricalState state =
                targetTerminal.ElectricalState;

            if (requireElectricalEnabled &&
                !targetTerminal.IsElectricalEnabled)
            {
                return false;
            }

            return Mathf.Abs(state.Voltage) >= MinimumVoltage;
        }


        // ================================================================
        // COMPONENT POWERED
        // ================================================================

        private bool EvaluateComponentPowered()
        {
            if (targetComponent == null)
                return false;

            if (requireElectricalEnabled &&
                !targetComponent.ElectricalEnabled)
            {
                return false;
            }

            SparkElectricalState state =
                targetComponent.ElectricalState;

            if (state.Voltage < MinimumVoltage)
                return false;

            if (state.Current < MinimumCurrent)
                return false;

            if (state.Power < MinimumPower)
                return false;

            if (requireConduction &&
                state.Conduction != SparkConductionState.Conducting)
            {
                return false;
            }

            return true;
        }


        // ================================================================
        // COMPONENT CONDUCTING
        // ================================================================

        private bool EvaluateComponentConducting()
        {
            if (targetComponent == null)
                return false;

            if (requireElectricalEnabled &&
                !targetComponent.ElectricalEnabled)
            {
                return false;
            }

            return targetComponent.ElectricalState.Conduction ==
                   SparkConductionState.Conducting;
        }


        // ================================================================
        // LED
        // ================================================================

        private bool EvaluateLED()
        {
            if (targetComponent == null)
                return false;

            SparkLED led =
                targetComponent.GetComponent<SparkLED>();

            if (led == null)
            {
                led =
                    targetComponent.GetComponentInChildren<SparkLED>();
            }

            if (led == null)
                return false;

            if (requireElectricalEnabled &&
                !targetComponent.ElectricalEnabled)
            {
                return false;
            }

            return led.IsOn;
        }


        // ================================================================
        // CONFIGURATION HELPERS
        // ================================================================

        /// <summary>
        /// Returns true when the target has enough information for
        /// its selected target type.
        /// </summary>
        public bool IsConfigured()
        {
            switch (targetType)
            {
                case TargetType.TerminalPowered:
                case TargetType.VoltagePresent:
                    return targetTerminal != null;

                case TargetType.ComponentPowered:
                case TargetType.LEDOn:
                case TargetType.ComponentConducting:
                    return targetComponent != null;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Returns true when the target has a complete polarity pair.
        /// </summary>
        public bool IsPolarityConfigurationValid()
        {
            return HasRequiredConnectionPair;
        }


        // ================================================================
        // NORMALIZATION
        // ================================================================

        public void Normalize()
        {
            minimumVoltage =
                Mathf.Max(0f, minimumVoltage);

            minimumCurrent =
                Mathf.Max(0f, minimumCurrent);

            minimumPower =
                Mathf.Max(0f, minimumPower);

            if (string.IsNullOrWhiteSpace(targetId))
            {
                targetId = "TARGET";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = targetId;
            }
        }
    }
}

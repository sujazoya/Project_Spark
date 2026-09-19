using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [Serializable]
    public sealed class SparkLevelTarget
    {
        public enum TargetType
        {
            TerminalPowered,
            VoltagePresent,
            ComponentPowered,
            LEDOn,
            ComponentConducting
        }

        [Header("Identity")]
        [SerializeField] private string targetId = "TARGET_01";

        [SerializeField] private string displayName =
            "Power Target";

        [TextArea(1, 4)]
        [SerializeField] private string description;

        [Header("Target Type")]
        [SerializeField] private TargetType targetType =
            TargetType.TerminalPowered;

        [Header("Terminal Target")]
        [SerializeField] private SparkTerminal targetTerminal;

        [Header("Component Target")]
        [SerializeField] private SparkElectricalComponent targetComponent;

        [Header("Voltage")]
        [Min(0f)]
        [SerializeField] private float minimumVoltage = 0.01f;

        [Header("Current")]
        [Min(0f)]
        [SerializeField] private float minimumCurrent = 0f;

        [Header("Power")]
        [Min(0f)]
        [SerializeField] private float minimumPower = 0f;

        [Header("Behavior")]
        [SerializeField] private bool requireElectricalEnabled = true;

        [SerializeField] private bool requireConduction;

        [SerializeField] private bool inverted;

        public string TargetId => targetId;

        public string DisplayName => displayName;

        public string Description => description;

        public TargetType Type => targetType;

        public SparkTerminal TargetTerminal =>
            targetTerminal;

        public SparkElectricalComponent TargetComponent =>
            targetComponent;

        public float MinimumVoltage =>
            Mathf.Max(0f, minimumVoltage);

        public float MinimumCurrent =>
            Mathf.Max(0f, minimumCurrent);

        public float MinimumPower =>
            Mathf.Max(0f, minimumPower);

        public bool RequireElectricalEnabled =>
            requireElectricalEnabled;

        public bool RequireConduction =>
            requireConduction;

        public bool Inverted =>
            inverted;

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

        private bool EvaluateLED()
        {
            if (targetComponent == null)
                return false;

            SparkLED led =
                targetComponent.GetComponent<SparkLED>();

            if (led == null)
                led = targetComponent.GetComponentInChildren<SparkLED>();

            if (led == null)
                return false;

            return led.IsOn;
        }

        public void Normalize()
        {
            minimumVoltage =
                Mathf.Max(0f, minimumVoltage);

            minimumCurrent =
                Mathf.Max(0f, minimumCurrent);

            minimumPower =
                Mathf.Max(0f, minimumPower);

            if (string.IsNullOrWhiteSpace(targetId))
                targetId = "TARGET";

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = targetId;
        }
    }
}
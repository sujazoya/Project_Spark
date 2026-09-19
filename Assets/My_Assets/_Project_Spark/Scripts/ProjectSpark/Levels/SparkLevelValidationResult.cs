using System;
using UnityEngine;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    public enum SparkLevelValidationState
    {
        Playing,
        Completed,
        WrongConnection,
        ShortCircuit,
        TargetShort,
        Overload,
        SolverFault,
        InvalidConfiguration
    }

    [Serializable]
    public struct SparkLevelValidationResult
    {
        public SparkLevelValidationState state;
        public string message;

        public int satisfiedTargets;
        public int totalTargets;

        public float targetVoltage;
        public float targetCurrent;
        public float targetPower;

        public string activeSourceName;

        public SparkTerminal affectedTerminal;
        public SparkLevelTarget affectedTarget;

        public SparkLevelValidationResult(
            SparkLevelValidationState state,
            string message,
            int satisfiedTargets,
            int totalTargets,
            float targetVoltage,
            float targetCurrent,
            float targetPower,
            string activeSourceName,
            SparkTerminal affectedTerminal,
            SparkLevelTarget affectedTarget)
        {
            this.state = state;
            this.message = message;
            this.satisfiedTargets = satisfiedTargets;
            this.totalTargets = totalTargets;
            this.targetVoltage = targetVoltage;
            this.targetCurrent = targetCurrent;
            this.targetPower = targetPower;
            this.activeSourceName = activeSourceName;
            this.affectedTerminal = affectedTerminal;
            this.affectedTarget = affectedTarget;
        }

        public bool IsFault =>
            state == SparkLevelValidationState.WrongConnection ||
            state == SparkLevelValidationState.ShortCircuit ||
            state == SparkLevelValidationState.TargetShort ||
            state == SparkLevelValidationState.Overload ||
            state == SparkLevelValidationState.SolverFault ||
            state == SparkLevelValidationState.InvalidConfiguration;

        public bool IsTerminalFault =>
            affectedTerminal != null;

        public bool IsTargetFault =>
            affectedTarget != null;
    }
}

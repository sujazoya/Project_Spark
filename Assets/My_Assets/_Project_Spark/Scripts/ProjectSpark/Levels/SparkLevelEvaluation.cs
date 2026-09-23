using System;
using UnityEngine;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    public enum SparkLevelEvaluationStatus
    {
        None,
        Evaluating,
        Incomplete,
        Completed,
        Failed
    }

    public enum SparkLevelFailureReason
    {
        None,
        NoLevel,
        InvalidConfiguration,
        NoValidPowerSource,
        OpenCircuit,
        InsufficientVoltage,
        TargetShortCircuit,
        SourceShortCircuit,
        Overload,
        InvalidTarget,
        InvalidConnection
    }

    [Serializable]
    public struct SparkLevelEvaluation
    {
        [SerializeField]
        private SparkLevelEvaluationStatus status;

        [SerializeField]
        private SparkLevelFailureReason failureReason;

        [SerializeField]
        private bool targetsSatisfied;

        [SerializeField]
        private int satisfiedTargetCount;

        [SerializeField]
        private int totalTargetCount;

        [SerializeField]
        private bool closedReturn;

        [SerializeField]
        private bool targetShorted;

        [SerializeField]
        private bool sourceShorted;

        [SerializeField]
        private bool overloaded;

        [SerializeField]
        private float targetVoltage;

        [SerializeField]
        private float targetCurrent;

        [SerializeField]
        private float targetPower;

        [SerializeField]
        private string activeSource;

        [SerializeField]
        private string message;

        [NonSerialized]
        private SparkTerminal affectedTerminal;

        [NonSerialized]
        private SparkLevelTarget affectedTarget;


        // ------------------------------------------------------------
        // CORE RESULT
        // ------------------------------------------------------------

        public SparkLevelEvaluationStatus Status =>
            status;

        public SparkLevelFailureReason FailureReason =>
            failureReason;

        public bool TargetsSatisfied =>
            targetsSatisfied;

        public int SatisfiedTargetCount =>
            satisfiedTargetCount;

        public int TotalTargetCount =>
            totalTargetCount;

        public bool ClosedReturn =>
            closedReturn;

        public bool TargetShorted =>
            targetShorted;

        public bool SourceShorted =>
            sourceShorted;

        public bool Overloaded =>
            overloaded;


        // ------------------------------------------------------------
        // ELECTRICAL DATA
        // ------------------------------------------------------------

        public float TargetVoltage =>
            targetVoltage;

        public float TargetCurrent =>
            targetCurrent;

        public float TargetPower =>
            targetPower;

        public string ActiveSource =>
            activeSource;


        // ------------------------------------------------------------
        // MESSAGE
        // ------------------------------------------------------------

        public string Message =>
            message;


        // ------------------------------------------------------------
        // AFFECTED OBJECTS
        // ------------------------------------------------------------

        public SparkTerminal AffectedTerminal =>
            affectedTerminal;

        public SparkLevelTarget AffectedTarget =>
            affectedTarget;

        public bool HasAffectedTerminal =>
            affectedTerminal != null;

        public bool HasAffectedTarget =>
            affectedTarget != null;


        // ------------------------------------------------------------
        // STATE HELPERS
        // ------------------------------------------------------------

        public bool IsCompleted =>
            status == SparkLevelEvaluationStatus.Completed;

        public bool IsFailed =>
            status == SparkLevelEvaluationStatus.Failed;

        public bool IsIncomplete =>
            status == SparkLevelEvaluationStatus.Incomplete;

        public bool HasFailureReason =>
            failureReason != SparkLevelFailureReason.None;


        // ------------------------------------------------------------
        // FACTORY
        // ------------------------------------------------------------

        public static SparkLevelEvaluation Create(
            SparkLevelEvaluationStatus status,
            SparkLevelFailureReason failureReason,
            string message)
        {
            return new SparkLevelEvaluation
            {
                status = status,
                failureReason = failureReason,
                message = message ?? string.Empty,

                targetsSatisfied = false,
                satisfiedTargetCount = 0,
                totalTargetCount = 0,

                closedReturn = false,
                targetShorted = false,
                sourceShorted = false,
                overloaded = false,

                targetVoltage = 0f,
                targetCurrent = 0f,
                targetPower = 0f,

                activeSource = string.Empty,

                affectedTerminal = null,
                affectedTarget = null
            };
        }


        // ------------------------------------------------------------
        // ELECTRICAL STATE
        // ------------------------------------------------------------

        public SparkLevelEvaluation WithElectricalState(
            bool isClosedReturn,
            bool isTargetShorted,
            bool isSourceShorted,
            bool isOverloaded,
            float voltage,
            float current,
            float power,
            string source)
        {
            closedReturn = isClosedReturn;
            targetShorted = isTargetShorted;
            sourceShorted = isSourceShorted;
            overloaded = isOverloaded;

            targetVoltage = voltage;
            targetCurrent = current;
            targetPower = power;

            activeSource = source ?? string.Empty;

            return this;
        }


        // ------------------------------------------------------------
        // TARGET STATE
        // ------------------------------------------------------------

        public SparkLevelEvaluation WithTargets(
            bool satisfied,
            int satisfiedCount,
            int totalCount)
        {
            targetsSatisfied = satisfied;

            satisfiedTargetCount =
                Mathf.Max(0, satisfiedCount);

            totalTargetCount =
                Mathf.Max(0, totalCount);

            return this;
        }


        // ------------------------------------------------------------
        // AFFECTED OBJECTS
        // ------------------------------------------------------------

        public SparkLevelEvaluation WithAffectedTerminal(
            SparkTerminal terminal)
        {
            affectedTerminal = terminal;
            return this;
        }

        public SparkLevelEvaluation WithAffectedTarget(
            SparkLevelTarget target)
        {
            affectedTarget = target;
            return this;
        }

        public SparkLevelEvaluation WithAffectedObjects(
            SparkLevelTarget target,
            SparkTerminal terminal)
        {
            affectedTarget = target;
            affectedTerminal = terminal;
            return this;
        }
    }
}
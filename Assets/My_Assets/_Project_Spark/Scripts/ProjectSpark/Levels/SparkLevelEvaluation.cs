using System;
using UnityEngine;

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
        InvalidTarget
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

        public SparkLevelEvaluationStatus Status =>
            status;

        public SparkLevelFailureReason FailureReason =>
            failureReason;

        public bool TargetsSatisfied =>
            targetsSatisfied;

        public bool ClosedReturn =>
            closedReturn;

        public bool TargetShorted =>
            targetShorted;

        public bool SourceShorted =>
            sourceShorted;

        public bool Overloaded =>
            overloaded;

        public float TargetVoltage =>
            targetVoltage;

        public float TargetCurrent =>
            targetCurrent;

        public float TargetPower =>
            targetPower;

        public string ActiveSource =>
            activeSource;

        public string Message =>
            message;

        public bool IsCompleted =>
            status == SparkLevelEvaluationStatus.Completed;

        public bool IsFailed =>
            status == SparkLevelEvaluationStatus.Failed;

        public bool IsIncomplete =>
            status == SparkLevelEvaluationStatus.Incomplete;

        public static SparkLevelEvaluation Create(
            SparkLevelEvaluationStatus status,
            SparkLevelFailureReason failureReason,
            string message)
        {
            return new SparkLevelEvaluation
            {
                status = status,
                failureReason = failureReason,
                message = message
            };
        }

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

    activeSource = source;

    return this;
}

        public SparkLevelEvaluation WithTargets(
            bool satisfied)
        {
            targetsSatisfied = satisfied;
            return this;
        }
    }
}
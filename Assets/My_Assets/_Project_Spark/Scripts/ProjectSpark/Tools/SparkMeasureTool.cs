using ProjectSpark.Gameplay;
using ProjectSpark.Measurement;
using UnityEngine;

namespace ProjectSpark.Tools
{
    [DisallowMultipleComponent]
    public sealed class SparkMeasureTool : SparkTool
    {
        [Header("References")]
        [SerializeField]
        private SparkMeasurementSystem measurementSystem;

        [Header("Measurement")]
        [SerializeField]
        private SparkMeasurementType measurementType =
            SparkMeasurementType.Voltage;

        [Header("Behaviour")]
        [SerializeField]
        private bool measureOnBegin = true;

        [SerializeField]
        private bool measureContinuously = false;

        [SerializeField, Min(0.01f)]
        private float continuousMeasurementInterval = 0.1f;

        private float nextMeasurementTime;

        private SparkTerminal activeTerminal;

        public SparkMeasurementType MeasurementType
        {
            get
            {
                return measurementType;
            }
        }

        public SparkTerminal ActiveTerminal
        {
            get
            {
                return activeTerminal;
            }
        }

        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Measure;
        }

        protected override void Awake()
        {
            base.Awake();

            if (measurementSystem == null)
            {
                measurementSystem =
                    GetComponentInParent<SparkMeasurementSystem>();
            }
        }

        public override bool CanBegin(
            in SparkToolContext context,
            out string reason)
        {
            if (!base.CanBegin(
                    context,
                    out reason))
            {
                return false;
            }

            if (measurementSystem == null)
            {
                reason =
                    "SparkMeasurementSystem is not configured.";

                return false;
            }

            if (!context.HasTarget)
            {
                reason =
                    "No measurement target was hit.";

                return false;
            }

            SparkTerminal terminal =
                GetTerminal(
                    context.TargetHit.Collider);

            if (terminal == null)
            {
                reason =
                    "Hit object does not contain a SparkTerminal.";

                return false;
            }

            if (!terminal.CanAccept(
                    SparkConnectionKind.Probe,
                    out reason))
            {
                return false;
            }

            reason = null;
            return true;
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            SparkTerminal terminal =
                GetTerminal(
                    context.TargetHit.Collider);

            if (terminal == null)
            {
                return SparkResult.Invalid(
                    "Measurement terminal is missing.");
            }

            activeTerminal = terminal;

            nextMeasurementTime =
                Time.unscaledTime;

            if (!measureOnBegin)
            {
                return SparkResult.Success(
                    "Measurement target selected.");
            }

            return PerformMeasurement();
        }

        protected override SparkResult OnUpdate(
            SparkToolContext context)
        {
            if (activeTerminal == null)
            {
                return SparkResult.Unavailable(
                    "Measurement terminal is no longer available.");
            }

            if (!measureContinuously)
            {
                return SparkResult.Success();
            }

            if (Time.unscaledTime <
                nextMeasurementTime)
            {
                return SparkResult.Success();
            }

            nextMeasurementTime =
                Time.unscaledTime +
                continuousMeasurementInterval;

            return PerformMeasurement();
        }

        protected override SparkResult OnEnd(
            SparkToolContext context)
        {
            activeTerminal = null;

            return SparkResult.Success(
                "Measurement completed.");
        }

        protected override SparkResult OnCancel(
            SparkToolContext context)
        {
            activeTerminal = null;

            return SparkResult.Cancelled(
                "Measurement cancelled.");
        }

        private SparkResult PerformMeasurement()
        {
            if (measurementSystem == null)
            {
                return SparkResult.Invalid(
                    "SparkMeasurementSystem is not configured.");
            }

            if (activeTerminal == null)
            {
                return SparkResult.Invalid(
                    "Measurement terminal is missing.");
            }

            SparkResult result =
                measurementSystem.TryMeasureTerminal(
                    activeTerminal,
                    measurementType,
                    out SparkMeasurementReading reading);

            if (!result.Succeeded)
            {
                return result;
            }

            return SparkResult.Success(
                CreateReadingMessage(reading));
        }

        private SparkTerminal GetTerminal(
            Collider collider)
        {
            if (collider == null)
                return null;

            return collider.GetComponentInParent<SparkTerminal>();
        }

        private string CreateReadingMessage(
            SparkMeasurementReading reading)
        {
            if (!reading.Valid)
            {
                return string.IsNullOrEmpty(reading.Reason)
                    ? "Measurement is invalid."
                    : reading.Reason;
            }

            if (float.IsPositiveInfinity(reading.Value))
            {
                return
                    $"{reading.Type}: ∞ {reading.Unit}";
            }

            if (float.IsNegativeInfinity(reading.Value))
            {
                return
                    $"{reading.Type}: -∞ {reading.Unit}";
            }

            if (float.IsNaN(reading.Value))
            {
                return
                    $"{reading.Type}: NaN {reading.Unit}";
            }

            return
                $"{reading.Type}: " +
                $"{reading.Value:G6} " +
                $"{reading.Unit}";
        }

        private void OnDisable()
        {
            activeTerminal = null;
        }
    }
}
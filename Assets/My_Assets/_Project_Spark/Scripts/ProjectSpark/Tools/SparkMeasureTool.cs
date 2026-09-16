using ProjectSpark.Gameplay;
using ProjectSpark.Measurement;
using UnityEngine;

namespace ProjectSpark.Tools
{
    public sealed class SparkMeasureTool : SparkTool
    {
        [Header("Measurement")]
        [SerializeField]
        private SparkMeasurementSystem measurementSystem;

        [SerializeField]
        private SparkMeasurementType measurementType =
            SparkMeasurementType.Voltage;

        public SparkMeasurementReading LastReading
        {
            get;
            private set;
        }

        public SparkMeasurementType MeasurementType
        {
            get { return measurementType; }
        }

        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Measure;
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
                    "Measurement system is not configured.";

                return false;
            }

            if (context.TargetHit.Target == null)
            {
                reason =
                    "No component selected.";

                return false;
            }

            SparkTerminal terminal =
                context.TargetHit.Target
                    .GetComponentInParent<SparkTerminal>();

            if (terminal == null)
            {
                reason =
                    "Target has no measurement terminal.";

                return false;
            }

            reason = null;
            return true;
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            if (measurementSystem == null)
            {
                return SparkResult.Unavailable(
                    "Measurement system is not configured.");
            }

            if (context.TargetHit.Target == null)
            {
                return SparkResult.Invalid(
                    "No component selected.");
            }

            SparkTerminal terminal =
                context.TargetHit.Target
                    .GetComponentInParent<SparkTerminal>();

            if (terminal == null)
            {
                return SparkResult.Invalid(
                    "Target has no measurement terminal.");
            }

            SparkResult result =
                measurementSystem.TryMeasureTerminal(
                    terminal,
                    measurementType,
                    out SparkMeasurementReading reading);

            if (result.Succeeded)
            {
                LastReading =
                    reading;
            }

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Verified facts from the electrical solver. Neither AI nor visual systems
    /// should manufacture or reinterpret these measurements.
    /// </summary>
    [Serializable]
    public sealed class SparkVerifiedElectricalReading
    {
        public string anchorId;
        public float voltageVolts;
        public float currentAmps;
        public float resistanceOhms;
        public SparkElectricalCondition condition = SparkElectricalCondition.Unknown;
        public string explanation;

        public SparkVerifiedElectricalReading Copy() => (SparkVerifiedElectricalReading)MemberwiseClone();
    }

    [Serializable]
    public sealed class SparkVerifiedElectricalFrame
    {
        [Tooltip("Unique level or circuit identifier. Change it whenever the loaded circuit changes.")]
        public string circuitId = "";
        [Tooltip("Optional positive solver revision. Revisions must increase within a circuit.")]
        public int revision;
        public List<SparkVerifiedElectricalReading> readings = new List<SparkVerifiedElectricalReading>();
        [Tooltip("Focus only this component's card after publishing all readings.")]
        public string focusAnchorId;

        [Header("Verified conventional current route")]
        [Tooltip("True only when the solver has verified route state in this frame.")]
        public bool hasVerifiedRoute;
        public bool routeEnergized;
        public string[] orderedRouteAnchors = Array.Empty<string>();

        [Header("Verified fault (optional)")]
        public bool hasVerifiedFault;
        public string faultAnchorId;
        public string verifiedFaultExplanation;

        [Header("Verified tutorial action (optional)")]
        public string verifiedActionId;

        public SparkVerifiedElectricalFrame Copy()
        {
            var copied = (SparkVerifiedElectricalFrame)MemberwiseClone();
            copied.readings = new List<SparkVerifiedElectricalReading>();
            if (readings != null)
                foreach (var reading in readings)
                    copied.readings.Add(reading?.Copy());
            copied.orderedRouteAnchors = orderedRouteAnchors != null
                ? (string[])orderedRouteAnchors.Clone() : Array.Empty<string>();
            return copied;
        }
    }
}

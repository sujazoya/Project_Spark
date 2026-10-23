
using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkCurrentRangeObjective : MonoBehaviour
    {
        [Header("Monitored Terminal")]
        [SerializeField]
        private SparkTerminal terminal;

        [Header("Allowed Current")]
        [SerializeField, Min(0f)]
        private float minimumCurrent = 0.005f;

        [SerializeField, Min(0f)]
        private float maximumCurrent = 0.015f;

        [Header("Runtime")]
        [SerializeField]
        private bool isSatisfied;

        [SerializeField]
        private float measuredCurrent;

        public bool IsSatisfied => isSatisfied;

        public float MeasuredCurrent => measuredCurrent;

        public event Action<bool> SatisfactionChanged;

        private void OnEnable()
        {
            if (terminal != null)
            {
                terminal.ElectricalStateChanged +=
                    OnElectricalStateChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (terminal != null)
            {
                terminal.ElectricalStateChanged -=
                    OnElectricalStateChanged;
            }
        }

        private void OnValidate()
        {
            minimumCurrent =
                Mathf.Max(0f, minimumCurrent);

            maximumCurrent =
                Mathf.Max(minimumCurrent, maximumCurrent);
        }

        private void OnElectricalStateChanged(
            SparkTerminal changedTerminal)
        {
            Refresh();
        }

        public void Refresh()
        {
            measuredCurrent =
                terminal != null
                    ? Mathf.Abs(terminal.Current)
                    : 0f;

            bool satisfied =
                terminal != null &&
                measuredCurrent >= minimumCurrent &&
                measuredCurrent <= maximumCurrent;

            if (isSatisfied == satisfied)
                return;

            isSatisfied = satisfied;

            SatisfactionChanged?.Invoke(isSatisfied);
        }
    }
}

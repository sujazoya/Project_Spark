using UnityEngine;

namespace ProjectSpark.Domain.Diagnostics
{
    public sealed class DiagnosticView
        : MonoBehaviour
    {
        [SerializeField]
        private GameObject voltageView;

        [SerializeField]
        private GameObject heatView;

        [SerializeField]
        private GameObject faultView;

        private void OnEnable()
        {
            DiagnosticEvents.ModeChanged +=
                UpdateView;
        }

        private void OnDisable()
        {
            DiagnosticEvents.ModeChanged -=
                UpdateView;
        }

        private void UpdateView(
            DiagnosticMode mode)
        {
            voltageView.SetActive(
                mode == DiagnosticMode.Voltage);

            heatView.SetActive(
                mode == DiagnosticMode.Temperature);

            faultView.SetActive(
                mode == DiagnosticMode.Fault);
        }
    }
}

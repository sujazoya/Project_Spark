using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Two-line 3D LCD readout. Reads the existing completed electrical solve;
    /// never changes circuit topology, electrical state, or level objectives.
    /// Designed for the fixed bottom resistor (R2) in Level 7.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLCDLiveReadout : MonoBehaviour
    {
        [Header("1602 LCD Text")]
        [SerializeField] private TMP_Text screenText;

        [Header("Level 7 Electrical References")]
        [SerializeField] private SparkElectricalSolver solver;
        [Tooltip("VOUT midpoint terminal between R1 and R2.")]
        [SerializeField] private SparkTerminal outputTerminal;
        [Tooltip("Battery negative / divider ground terminal.")]
        [SerializeField] private SparkTerminal groundTerminal;
        [Tooltip("Fixed lower resistor R2; its current is the divider current.")]
        [SerializeField] private SparkResistor bottomResistor;

        [Header("Display")]
        [SerializeField, Min(0.05f)] private float refreshIntervalSeconds = 0.2f;

        private float nextRefresh;

        private void OnEnable()
        {
            if (solver == null)
                solver = FindFirstObjectByType<SparkElectricalSolver>();

            nextRefresh = 0f;
            Refresh();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh)
                return;

            nextRefresh = Time.unscaledTime + refreshIntervalSeconds;
            Refresh();
        }

        private void Refresh()
        {
            if (screenText == null)
                return;

            if (solver == null)
                solver = FindFirstObjectByType<SparkElectricalSolver>();

            if (solver == null || solver.IsDirty || solver.IsSolving ||
                solver.LatestSolveResult == null ||
                outputTerminal == null || groundTerminal == null ||
                bottomResistor == null || !bottomResistor.ElectricalEnabled)
            {
                Show("VOUT --.--V\nI    --.--mA");
                return;
            }

            float voltage;
            float current;
            if (!solver.LatestSolveResult.TryGetVoltage(
                    outputTerminal, groundTerminal, out voltage) ||
                !solver.TryCalculateComponentCurrent(bottomResistor, out current))
            {
                Show("VOUT --.--V\nI    --.--mA");
                return;
            }

            Show($"VOUT {voltage:0.00}V\nI    {Mathf.Abs(current) * 1000f:0.00}mA");
        }

        private void Show(string value)
        {
            if (screenText.text != value)
                screenText.text = value;
        }
    }
}

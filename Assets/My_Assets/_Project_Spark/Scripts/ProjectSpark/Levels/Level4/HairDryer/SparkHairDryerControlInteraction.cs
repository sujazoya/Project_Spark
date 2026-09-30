using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Physical interaction component for a hair-dryer control.
    ///
    /// This component operates the actual SparkSwitch.
    ///
    /// Electrical simulation:
    ///     SparkSwitch
    ///         ↓
    ///     SparkElectricalSolver
    ///
    /// Visual simulation:
    ///     SparkHairDryerVisualController
    ///
    /// No appliance-level electrical owner is required.
    ///
    /// Attach this component to the clickable object representing:
    /// - Power switch
    /// - Heat switch
    /// - Fan speed switch
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerControlInteraction : MonoBehaviour
    {
        // ============================================================
        // CONTROL TYPE
        // ============================================================

        public enum ControlType
        {
            Power,
            Heat,
            FanSpeed
        }

        // ============================================================
        // ELECTRICAL SWITCH
        // ============================================================

        [Header("Electrical Switch")]
        [SerializeField]
        private SparkSwitch sparkSwitch;

        // ============================================================
        // CONTROL
        // ============================================================

        [Header("Control")]
        [SerializeField]
        private ControlType controlType;

        [Header("Interaction")]
        [SerializeField]
        private bool toggleOnClick = true;

        [SerializeField]
        private bool allowDirectStateSelection = true;

        // ============================================================
        // STATE
        // ============================================================

        [Header("Runtime State")]
        [SerializeField]
        private bool isOn;

        public ControlType Type =>
            controlType;

        public bool IsOn =>
            isOn;

        public SparkSwitch Switch =>
            sparkSwitch;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            if (sparkSwitch == null)
                sparkSwitch = GetComponent<SparkSwitch>();

            SyncFromSwitch();
        }

        // ============================================================
        // INTERACTION
        // ============================================================

        /// <summary>
        /// Called by the Project Spark interaction system.
        /// </summary>
        public void Activate()
        {
            if (sparkSwitch == null)
                return;

            if (toggleOnClick)
            {
                Toggle();
                return;
            }

            ApplyCurrentState();
        }

        // ============================================================
        // TOGGLE
        // ============================================================

        public void Toggle()
        {
            if (sparkSwitch == null)
                return;

            sparkSwitch.ToggleState();

            SyncFromSwitch();
        }

        // ============================================================
        // DIRECT STATE
        // ============================================================

        public void SetState(bool on)
        {
            if (!allowDirectStateSelection)
                return;

            if (sparkSwitch == null)
                return;

            sparkSwitch.SetState(
                on
                    ? SparkSwitchState.Closed
                    : SparkSwitchState.Open);

            SyncFromSwitch();
        }

        // ============================================================
        // CURRENT STATE
        // ============================================================

        private void ApplyCurrentState()
        {
            SetState(isOn);
        }

        // ============================================================
        // SYNCHRONIZATION
        // ============================================================

        public void SyncFromSwitch()
        {
            if (sparkSwitch == null)
            {
                isOn = false;
                return;
            }

            isOn =
                sparkSwitch.State ==
                SparkSwitchState.Closed;
        }

        // ============================================================
        // PUBLIC HELPERS
        // ============================================================

        public void TurnOn()
        {
            SetState(true);
        }

        public void TurnOff()
        {
            SetState(false);
        }

        public bool IsSwitchClosed()
        {
            return sparkSwitch != null &&
                   sparkSwitch.IsClosed;
        }

        // ============================================================
        // CONTEXT TESTS
        // ============================================================

        [ContextMenu("Test / Activate")]
        private void TestActivate()
        {
            Activate();
        }

        [ContextMenu("Test / Turn On")]
        private void TestTurnOn()
        {
            TurnOn();
        }

        [ContextMenu("Test / Turn Off")]
        private void TestTurnOff()
        {
            TurnOff();
        }

        [ContextMenu("Test / Toggle")]
        private void TestToggle()
        {
            Toggle();
        }
    }
}

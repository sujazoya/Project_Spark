using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Physical interaction component for a hair-dryer control.
    ///
    /// Supports both:
    ///
    ///     SparkSwitch
    ///         → normal ON / OFF
    ///
    ///     SparkSwitchIndex
    ///         → indexed control
    ///            0 = OFF
    ///            1 = LOW
    ///            2 = HIGH
    ///            etc.
    ///
    /// Electrical simulation is handled by the actual electrical component.
    ///
    /// This component only handles player interaction.
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
        // ELECTRICAL COMPONENTS
        // ============================================================

        [Header("Electrical Components")]

        [Tooltip("Use for normal ON/OFF control.")]
        [SerializeField]
        private SparkSwitch sparkSwitch;

        [Tooltip("Use for indexed Heat/Fan controls.")]
        [SerializeField]
        private SparkSwitchIndex sparkSwitchIndex;


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

        [SerializeField]
        private bool cycleIndexOnClick = true;


        // ============================================================
        // STATE
        // ============================================================

        [Header("Runtime State")]

        [SerializeField]
        private bool isOn;

        [SerializeField]
        private int currentIndex;


        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        public ControlType Type =>
            controlType;

        public bool IsOn =>
            isOn;

        public int Index =>
            currentIndex;

        public SparkSwitch Switch =>
            sparkSwitch;

        public SparkSwitchIndex IndexedSwitch =>
            sparkSwitchIndex;


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            AutoFindComponents();
            SyncFromElectricalComponent();
        }


        // ============================================================
        // AUTO FIND
        // ============================================================

        private void AutoFindComponents()
        {
            if (sparkSwitch == null)
                sparkSwitch = GetComponent<SparkSwitch>();

            if (sparkSwitchIndex == null)
                sparkSwitchIndex =
                    GetComponent<SparkSwitchIndex>();

            /*
             * Do not allow both electrical components to control
             * the same interaction.
             *
             * SparkSwitchIndex has priority if both are assigned.
             */
        }


        // ============================================================
        // INTERACTION
        // ============================================================

        /// <summary>
        /// Called by the Project Spark interaction system.
        /// </summary>
        public void Activate()
        {
            if (sparkSwitchIndex != null)
            {
                ActivateIndexed();
                return;
            }

            if (sparkSwitch != null)
            {
                ActivateBinary();
            }
        }


        // ============================================================
        // BINARY SWITCH
        // ============================================================

        private void ActivateBinary()
        {
            if (toggleOnClick)
            {
                Toggle();
                return;
            }

            ApplyCurrentState();
        }


        // ============================================================
        // INDEXED SWITCH
        // ============================================================

        private void ActivateIndexed()
        {
            if (sparkSwitchIndex == null)
                return;

            if (cycleIndexOnClick)
            {
                NextIndex();
                return;
            }

            ApplyCurrentIndex();
        }


        // ============================================================
        // NORMAL TOGGLE
        // ============================================================

        public void Toggle()
        {
            if (sparkSwitch == null)
                return;

            sparkSwitch.ToggleState();

            SyncFromElectricalComponent();
        }


        // ============================================================
        // INDEX NEXT
        // ============================================================

        public void NextIndex()
        {
            if (sparkSwitchIndex == null)
                return;

            sparkSwitchIndex.NextIndex();

            SyncFromElectricalComponent();
        }


        // ============================================================
        // INDEX PREVIOUS
        // ============================================================

        public void PreviousIndex()
        {
            if (sparkSwitchIndex == null)
                return;

            sparkSwitchIndex.PreviousIndex();

            SyncFromElectricalComponent();
        }


        // ============================================================
        // DIRECT BINARY STATE
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

            SyncFromElectricalComponent();
        }


        // ============================================================
        // DIRECT INDEX
        // ============================================================

        public void SetIndex(int index)
        {
            if (!allowDirectStateSelection)
                return;

            if (sparkSwitchIndex == null)
                return;

            sparkSwitchIndex.SetIndex(index);

            SyncFromElectricalComponent();
        }


        // ============================================================
        // CURRENT BINARY STATE
        // ============================================================

        private void ApplyCurrentState()
        {
            SetState(isOn);
        }


        // ============================================================
        // CURRENT INDEX
        // ============================================================

        private void ApplyCurrentIndex()
        {
            SetIndex(currentIndex);
        }


        // ============================================================
        // SYNCHRONIZATION
        // ============================================================

        public void SyncFromElectricalComponent()
        {
            // --------------------------------------------------------
            // INDEXED SWITCH
            // --------------------------------------------------------

            if (sparkSwitchIndex != null)
            {
                currentIndex =
                    sparkSwitchIndex.Index;

                isOn =
                    !sparkSwitchIndex.IsOff;

                return;
            }

            // --------------------------------------------------------
            // NORMAL SWITCH
            // --------------------------------------------------------

            if (sparkSwitch != null)
            {
                isOn =
                    sparkSwitch.State ==
                    SparkSwitchState.Closed;

                currentIndex =
                    isOn ? 1 : 0;

                return;
            }

            // --------------------------------------------------------
            // NOTHING ASSIGNED
            // --------------------------------------------------------

            isOn = false;
            currentIndex = 0;
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
            if (sparkSwitchIndex != null)
            {
                sparkSwitchIndex.TurnOff();
                SyncFromElectricalComponent();
                return;
            }

            SetState(false);
        }


        public bool IsSwitchClosed()
        {
            return sparkSwitch != null &&
                   sparkSwitch.IsClosed;
        }


        public bool IsIndexed()
        {
            return sparkSwitchIndex != null;
        }


        public int GetIndex()
        {
            if (sparkSwitchIndex == null)
                return isOn ? 1 : 0;

            return sparkSwitchIndex.Index;
        }


        public int GetPositionCount()
        {
            if (sparkSwitchIndex == null)
                return 2;

            return sparkSwitchIndex.PositionCount;
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


        [ContextMenu("Test / Next Index")]
        private void TestNextIndex()
        {
            NextIndex();
        }


        [ContextMenu("Test / Previous Index")]
        private void TestPreviousIndex()
        {
            PreviousIndex();
        }


        [ContextMenu("Test / Set Index 0")]
        private void TestIndex0()
        {
            SetIndex(0);
        }


        [ContextMenu("Test / Set Index 1")]
        private void TestIndex1()
        {
            SetIndex(1);
        }


        [ContextMenu("Test / Set Index 2")]
        private void TestIndex2()
        {
            SetIndex(2);
        }
    }
}

using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Interaction component for Project Spark hair-dryer controls.
    ///
    /// Connects a physical control to SparkHairDryerVisualController.
    ///
    /// Visual/control interaction only.
    /// Does NOT perform electrical simulation.
    ///
    /// Attach this to the collider/object representing:
    /// - Power switch
    /// - Heat switch
    /// - Fan speed switch
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerControlInteraction : MonoBehaviour
    {
        public enum ControlType
        {
            Power,
            Heat,
            FanSpeed
        }

        [Header("Controller")]
        [SerializeField]
        private SparkHairDryerVisualController controller;

        [Header("Control")]
        [SerializeField]
        private ControlType controlType;

        [Header("Interaction")]
        [SerializeField]
        private bool cycleOnClick = true;

        [SerializeField]
        private bool allowDirectPositionSelection = true;

        [Header("Current Position")]
        [SerializeField]
        private int currentPosition;

        public ControlType Type =>
            controlType;

        public int CurrentPosition =>
            currentPosition;

        private void Awake()
        {
            SyncFromController();
        }

        public void Activate()
        {
            if (controller == null)
                return;

            if (cycleOnClick)
            {
                CycleControl();
                return;
            }

            ApplyCurrentPosition();
        }

        public void SetPosition(int position)
        {
            if (!allowDirectPositionSelection)
                return;

            if (controller == null)
                return;

            switch (controlType)
            {
                case ControlType.Power:

                    position =
                        Mathf.Clamp(position, 0, 1);

                    currentPosition = position;

                    controller.SetPower(
                        position == 1);

                    break;

                case ControlType.Heat:

                    position =
                        Mathf.Clamp(position, 0, 2);

                    currentPosition = position;

                    switch (position)
                    {
                        case 0:
                            controller.SetCold();
                            break;

                        case 1:
                            controller.SetLowHeat();
                            break;

                        case 2:
                            controller.SetHighHeat();
                            break;
                    }

                    break;

                case ControlType.FanSpeed:

                    position =
                        Mathf.Clamp(position, 0, 2);

                    currentPosition = position;

                    switch (position)
                    {
                        case 0:
                            controller.SetFanOff();
                            break;

                        case 1:
                            controller.SetFanLow();
                            break;

                        case 2:
                            controller.SetFanHigh();
                            break;
                    }

                    break;
            }
        }

        private void CycleControl()
        {
            switch (controlType)
            {
                case ControlType.Power:

                    currentPosition++;

                    if (currentPosition > 1)
                        currentPosition = 0;

                    SetPosition(currentPosition);

                    break;

                case ControlType.Heat:

                    currentPosition++;

                    if (currentPosition > 2)
                        currentPosition = 0;

                    SetPosition(currentPosition);

                    break;

                case ControlType.FanSpeed:

                    currentPosition++;

                    if (currentPosition > 2)
                        currentPosition = 0;

                    SetPosition(currentPosition);

                    break;
            }
        }

        private void ApplyCurrentPosition()
        {
            SetPosition(currentPosition);
        }

        public void SyncFromController()
        {
            if (controller == null)
                return;

            switch (controlType)
            {
                case ControlType.Power:

                    currentPosition =
                        controller.CurrentPowerState ==
                        SparkHairDryerVisualController.PowerState.On
                            ? 1
                            : 0;

                    break;

                case ControlType.Heat:

                    currentPosition =
                        (int)controller.CurrentHeatMode;

                    break;

                case ControlType.FanSpeed:

                    currentPosition =
                        (int)controller.CurrentFanSpeed;

                    break;
            }
        }

        // ============================================================
        // CONTEXT TESTS
        // ============================================================

        [ContextMenu("Test / Activate")]
        private void TestActivate()
        {
            Activate();
        }

        [ContextMenu("Test / Position 0")]
        private void TestPosition0()
        {
            SetPosition(0);
        }

        [ContextMenu("Test / Position 1")]
        private void TestPosition1()
        {
            SetPosition(1);
        }

        [ContextMenu("Test / Position 2")]
        private void TestPosition2()
        {
            SetPosition(2);
        }
    }
}
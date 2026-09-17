using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    /// <summary>
    /// Handles keyboard and UI input for the holographic measurement system.
    ///
    /// Responsibilities:
    /// - Activate measurement mode.
    /// - Deactivate measurement mode.
    /// - Clear the current measurement.
    /// - Keep the UI Toggle synchronized with measurement visibility.
    /// - Select measurement types from keyboard or UI buttons.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HolographicMeasurementInput : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private HolographicMeasurementController measurement;

        [SerializeField]
        private Toggle toggle;

        /// <summary>
        /// Returns true when measurement mode is currently active.
        /// </summary>
        public bool IsVisible { get; private set; }

#if ENABLE_INPUT_SYSTEM

        private void Awake()
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(OnToggleChanged);
            }

            SyncToggle(false);
        }

        private void OnDestroy()
        {
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(OnToggleChanged);
            }
        }

        private void Update()
        {
            if (measurement == null ||
                Keyboard.current == null)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            // --------------------------------------------------
            // Measurement types
            // --------------------------------------------------

            if (keyboard.dKey.wasPressedThisFrame)
            {
                Activate(
                    HolographicMeasurementType.Distance
                );

                return;
            }

            if (keyboard.aKey.wasPressedThisFrame)
            {
                Activate(
                    HolographicMeasurementType.Angle
                );

                return;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                Activate(
                    HolographicMeasurementType.Radius
                );

                return;
            }

            if (keyboard.cKey.wasPressedThisFrame)
            {
                Activate(
                    HolographicMeasurementType.Diameter
                );

                return;
            }

            // --------------------------------------------------
            // Escape = OFF + clear current measurement
            // --------------------------------------------------

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Deactivate();

                return;
            }

            // --------------------------------------------------
            // Delete = clear current measurement only
            // Measurement mode remains ON.
            // --------------------------------------------------

            if (keyboard.deleteKey.wasPressedThisFrame)
            {
                ClearCurrentMeasurement();

                return;
            }
        }

        // ======================================================
        // ACTIVATION
        // ======================================================

        /// <summary>
        /// Activates measurement mode using the specified type.
        /// </summary>
        public void Activate(
            HolographicMeasurementType type)
        {
            if (measurement == null)
            {
                return;
            }

            measurement.SetMeasurementType(type);
            measurement.SetActive(true);

            IsVisible = true;

            SyncToggle(false);
        }

        /// <summary>
        /// Activates distance measurement.
        /// </summary>
        public void ActivateDistance()
        {
            Activate(
                HolographicMeasurementType.Distance
            );
        }

        /// <summary>
        /// Activates angle measurement.
        /// </summary>
        public void ActivateAngle()
        {
            Activate(
                HolographicMeasurementType.Angle
            );
        }

        /// <summary>
        /// Activates radius measurement.
        /// </summary>
        public void ActivateRadius()
        {
            Activate(
                HolographicMeasurementType.Radius
            );
        }

        /// <summary>
        /// Activates diameter measurement.
        /// </summary>
        public void ActivateDiameter()
        {
            Activate(
                HolographicMeasurementType.Diameter
            );
        }

        // ======================================================
        // DEACTIVATION
        // ======================================================

        /// <summary>
        /// Turns measurement mode OFF and clears
        /// the current/in-progress measurement.
        /// </summary>
        public void Deactivate()
        {
            if (measurement == null)
            {
                return;
            }

            // Clear the currently active measurement first.
            measurement.ClearMeasurement();

            // Then disable measurement mode.
            measurement.SetActive(false);

            IsVisible = false;

            SyncToggle(false);
        }

        // ======================================================
        // CLEAR
        // ======================================================

        /// <summary>
        /// Clears only the current/in-progress measurement.
        ///
        /// Measurement mode remains active.
        /// </summary>
        public void ClearCurrentMeasurement()
        {
            if (measurement == null)
            {
                return;
            }

            measurement.ClearMeasurement();
        }

        // ======================================================
        // TOGGLE
        // ======================================================

        /// <summary>
        /// Called by the UI Toggle.
        /// </summary>
        private void OnToggleChanged(bool value)
        {
            if (value)
            {
                ActivateToggle();
            }
            else
            {
                Deactivate();
            }
        }

        /// <summary>
        /// Turns measurement ON from the Toggle.
        /// </summary>
        private void ActivateToggle()
        {
            if (measurement == null)
            {
                return;
            }

            IsVisible = true;

            measurement.SetActive(true);

            SyncToggle(false);
        }

        // ======================================================
        // UI SYNCHRONIZATION
        // ======================================================

        /// <summary>
        /// Synchronizes the UI Toggle without causing
        /// another Toggle callback.
        /// </summary>
        private void SyncToggle(bool sendCallback)
        {
            if (toggle == null)
            {
                return;
            }

            toggle.SetIsOnWithoutNotify(IsVisible);
        }

#endif
    }
}
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Controls the visual airflow of the Level 4 hair dryer.
    ///
    /// Responsibilities:
    /// - Starts/stops airflow.
    /// - Controls airflow strength.
    /// - Controls airflow direction.
    /// - Controls hot/cold visual state.
    /// - Sends values to a VFX Graph.
    ///
    /// This component is VISUAL ONLY.
    ///
    /// Future electrical flow:
    ///
    /// Motor electrical state
    ///        ↓
    /// Fan RPM
    ///        ↓
    /// Airflow strength
    ///        ↓
    /// This component
    ///        ↓
    /// VFX Graph
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerAirflowVisual : MonoBehaviour
    {
        // ============================================================
        // VFX PROPERTY IDS
        // ============================================================

        private static readonly int AirflowStrengthID =
            Shader.PropertyToID("AirflowStrength");

        private static readonly int AirflowTemperatureID =
            Shader.PropertyToID("AirflowTemperature");

        private static readonly int AirflowDirectionID =
            Shader.PropertyToID("AirflowDirection");

        // ============================================================
        // VFX
        // ============================================================

        [Header("VFX")]

        [Tooltip(
            "Visual Effect Graph used to display the expelled airflow.")]
        [SerializeField]
        private VisualEffect airflowVFX;

        [Tooltip(
            "Enables/disables the VFX object when airflow stops.")]
        [SerializeField]
        private bool controlVFXObject = true;

        // ============================================================
        // AIRFLOW
        // ============================================================

        [Header("Airflow")]

        [Tooltip(
            "Current airflow strength.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float airflowStrength;

        [Tooltip(
            "Target airflow strength.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float targetAirflowStrength;

        [Tooltip(
            "How quickly airflow accelerates.")]
        [SerializeField]
        [Min(0f)]
        private float acceleration = 2.5f;

        [Tooltip(
            "How quickly airflow stops.")]
        [SerializeField]
        [Min(0f)]
        private float deceleration = 4f;

        // ============================================================
        // TEMPERATURE
        // ============================================================

        [Header("Air Temperature")]

        [Tooltip(
            "Current visual air temperature.\n" +
            "0 = cold air.\n" +
            "1 = maximum hot air.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float airTemperature;

        [Tooltip(
            "Target visual air temperature.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float targetAirTemperature;

        [Tooltip(
            "How quickly the visual air temperature changes.")]
        [SerializeField]
        [Min(0f)]
        private float temperatureResponse = 0.8f;

        // ============================================================
        // DIRECTION
        // ============================================================

        [Header("Direction")]

        [Tooltip(
            "Transform representing the hair-dryer nozzle direction.")]
        [SerializeField]
        private Transform airflowDirection;

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        /// <summary>
        /// Current airflow strength.
        /// </summary>
        public float AirflowStrength =>
            airflowStrength;

        /// <summary>
        /// Current visual air temperature.
        /// </summary>
        public float AirTemperature =>
            airTemperature;

        /// <summary>
        /// True when airflow is visually active.
        /// </summary>
        public bool IsAirflowActive =>
            airflowStrength > 0.01f;

        /// <summary>
        /// Current airflow direction.
        /// </summary>
        public Vector3 AirflowDirection
        {
            get
            {
                if (airflowDirection == null)
                    return transform.forward;

                return airflowDirection.forward;
            }
        }

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ApplyVFXState(true);
        }

        private void Update()
        {
            UpdateAirflowStrength();
            UpdateAirTemperature();
            UpdateVFX();
        }

        // ============================================================
        // AIRFLOW
        // ============================================================

        private void UpdateAirflowStrength()
        {
            float speed =
                targetAirflowStrength > airflowStrength
                    ? acceleration
                    : deceleration;

            airflowStrength =
                Mathf.MoveTowards(
                    airflowStrength,
                    targetAirflowStrength,
                    speed * Time.deltaTime);
        }

        // ============================================================
        // AIR TEMPERATURE
        // ============================================================

        private void UpdateAirTemperature()
        {
            airTemperature =
                Mathf.MoveTowards(
                    airTemperature,
                    targetAirTemperature,
                    temperatureResponse *
                    Time.deltaTime);
        }

        // ============================================================
        // VFX
        // ============================================================

        private void UpdateVFX()
        {
            if (airflowVFX == null)
                return;

            airflowVFX.SetFloat(
                AirflowStrengthID,
                airflowStrength);

            airflowVFX.SetFloat(
                AirflowTemperatureID,
                airTemperature);

            airflowVFX.SetVector3(
                AirflowDirectionID,
                AirflowDirection);

            if (controlVFXObject)
            {
                bool shouldBeActive =
                    airflowStrength > 0.001f;

                if (airflowVFX.gameObject.activeSelf !=
                    shouldBeActive)
                {
                    airflowVFX.gameObject.SetActive(
                        shouldBeActive);
                }
            }
        }

        private void ApplyVFXState(
            bool force)
        {
            if (airflowVFX == null)
                return;

            airflowVFX.SetFloat(
                AirflowStrengthID,
                airflowStrength);

            airflowVFX.SetFloat(
                AirflowTemperatureID,
                airTemperature);

            airflowVFX.SetVector3(
                AirflowDirectionID,
                AirflowDirection);

            if (!controlVFXObject)
                return;

            bool shouldBeActive =
                airflowStrength > 0.001f;

            if (force ||
                airflowVFX.gameObject.activeSelf !=
                shouldBeActive)
            {
                airflowVFX.gameObject.SetActive(
                    shouldBeActive);
            }
        }

        // ============================================================
        // CONTROL
        // ============================================================

        /// <summary>
        /// Stops airflow.
        /// </summary>
        public void StopAirflow()
        {
            targetAirflowStrength = 0f;
        }

        /// <summary>
        /// Low fan airflow.
        /// </summary>
        public void SetLowAirflow()
        {
            targetAirflowStrength = 0.45f;
        }

        /// <summary>
        /// High fan airflow.
        /// </summary>
        public void SetHighAirflow()
        {
            targetAirflowStrength = 1f;
        }

        /// <summary>
        /// Sets airflow strength directly.
        /// </summary>
        public void SetAirflowStrength(
            float value)
        {
            targetAirflowStrength =
                Mathf.Clamp01(value);
        }

        /// <summary>
        /// Sets airflow temperature.
        ///
        /// 0 = cold
        /// 1 = maximum hot
        /// </summary>
        public void SetAirTemperature(
            float value)
        {
            targetAirTemperature =
                Mathf.Clamp01(value);
        }

        /// <summary>
        /// Immediately synchronizes airflow without interpolation.
        /// </summary>
        public void SetAirflowImmediate(
            float strength,
            float temperatureValue)
        {
            airflowStrength =
                Mathf.Clamp01(strength);

            targetAirflowStrength =
                airflowStrength;

            airTemperature =
                Mathf.Clamp01(
                    temperatureValue);

            targetAirTemperature =
                airTemperature;

            ApplyVFXState(true);
        }

        // ============================================================
        // TEST
        // ============================================================

        [ContextMenu("Test / Airflow OFF")]
        private void TestAirflowOff()
        {
            StopAirflow();
        }

        [ContextMenu("Test / Cold Air")]
        private void TestColdAir()
        {
            SetHighAirflow();
            SetAirTemperature(0f);
        }

        [ContextMenu("Test / Low Warm Air")]
        private void TestLowWarmAir()
        {
            SetLowAirflow();
            SetAirTemperature(0.55f);
        }

        [ContextMenu("Test / High Hot Air")]
        private void TestHighHotAir()
        {
            SetHighAirflow();
            SetAirTemperature(1f);
        }
    }
}
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerAirflowVisual : MonoBehaviour
    {
        // ============================================================
        // VFX PROPERTY NAMES
        // Must exactly match the exposed properties in VFX Graph.
        // ============================================================

        private static readonly int AirflowStrengthID =
            Shader.PropertyToID("AirflowStrength");

        private static readonly int AirflowTemperatureID =
            Shader.PropertyToID("AirflowTemperature");

        private static readonly int AirflowDirectionID =
            Shader.PropertyToID("AirflowDirection");

        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("VFX")]
        [SerializeField] private VisualEffect airflowVFX;

        [SerializeField] private GameObject controlVFXObject;

        [Header("Direction")]
        [SerializeField] private Transform airflowDirection;

        // ============================================================
        // AIRFLOW
        // ============================================================

        [Header("Airflow")]
        [SerializeField]
        [Range(0f, 1f)]
        private float airflowStrength;

        [SerializeField]
        [Range(0f, 1f)]
        private float targetAirflowStrength;

        [SerializeField]
        private float acceleration = 2.5f;

        [SerializeField]
        private float deceleration = 4f;

        // ============================================================
        // TEMPERATURE
        // ============================================================

        [Header("Temperature")]
        [SerializeField]
        [Range(0f, 1f)]
        private float airTemperature;

        [SerializeField]
        [Range(0f, 1f)]
        private float targetAirTemperature;

        [SerializeField]
        private float temperatureResponse = 0.8f;

        // ============================================================
        // STATE
        // ============================================================

        public float AirflowStrength => airflowStrength;
        public float AirTemperature => airTemperature;
        public bool IsRunning => airflowStrength > 0.001f;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            if (controlVFXObject == null && airflowVFX != null)
                controlVFXObject = airflowVFX.gameObject;

            airflowStrength = 0f;
            targetAirflowStrength = 0f;

            airTemperature = 0f;
            targetAirTemperature = 0f;

            ApplyVFXProperties();
            StopVFXImmediate();
        }

        private void Update()
        {
            UpdateAirflow();
            UpdateTemperature();
            ApplyVFXProperties();
            UpdateVFXPlayback();
        }

        // ============================================================
        // AIRFLOW UPDATE
        // ============================================================

        private void UpdateAirflow()
        {
            float speed =
                targetAirflowStrength > airflowStrength
                    ? acceleration
                    : deceleration;

            airflowStrength = Mathf.MoveTowards(
                airflowStrength,
                targetAirflowStrength,
                speed * Time.deltaTime);
        }

        // ============================================================
        // TEMPERATURE UPDATE
        // ============================================================

        private void UpdateTemperature()
        {
            airTemperature = Mathf.MoveTowards(
                airTemperature,
                targetAirTemperature,
                temperatureResponse * Time.deltaTime);
        }

        // ============================================================
        // APPLY VFX
        // ============================================================

        private void ApplyVFXProperties()
        {
            if (airflowVFX == null)
                return;

            airflowVFX.SetFloat(
                AirflowStrengthID,
                airflowStrength);

            airflowVFX.SetFloat(
                AirflowTemperatureID,
                airTemperature);

            if (airflowDirection != null)
            {
                airflowVFX.SetVector3(
                    AirflowDirectionID,
                    airflowDirection.forward);
            }
        }

        // ============================================================
        // PLAYBACK
        // ============================================================

        private void UpdateVFXPlayback()
        {
            if (airflowVFX == null)
                return;

            bool shouldRun = airflowStrength > 0.001f;

            if (shouldRun)
            {
                if (controlVFXObject != null &&
                    !controlVFXObject.activeSelf)
                {
                    controlVFXObject.SetActive(true);
                }

                if (!airflowVFX.enabled)
                    airflowVFX.enabled = true;

                airflowVFX.Play();
            }
            else
            {
                if (airflowVFX.enabled)
                    airflowVFX.Stop();

                if (controlVFXObject != null &&
                    controlVFXObject.activeSelf)
                {
                    controlVFXObject.SetActive(false);
                }
            }
        }

        // ============================================================
        // PUBLIC CONTROL
        // ============================================================

        public void StopAirflow()
        {
            targetAirflowStrength = 0f;
            targetAirTemperature = 0f;
        }

        public void SetLowAirflow()
        {
            targetAirflowStrength = 0.45f;
        }

        public void SetHighAirflow()
        {
            targetAirflowStrength = 1f;
        }

        public void SetAirflowStrength(float strength)
        {
            targetAirflowStrength =
                Mathf.Clamp01(strength);
        }

        public void SetAirTemperature(float temperature)
        {
            targetAirTemperature =
                Mathf.Clamp01(temperature);
        }

        public void SetAirflowImmediate(
            float strength,
            float temperature)
        {
            airflowStrength =
                Mathf.Clamp01(strength);

            targetAirflowStrength =
                airflowStrength;

            airTemperature =
                Mathf.Clamp01(temperature);

            targetAirTemperature =
                airTemperature;

            ApplyVFXProperties();

            if (airflowStrength > 0.001f)
            {
                if (controlVFXObject != null)
                    controlVFXObject.SetActive(true);

                if (airflowVFX != null)
                {
                    airflowVFX.enabled = true;
                    airflowVFX.Play();
                }
            }
            else
            {
                StopVFXImmediate();
            }
        }

        // ============================================================
        // IMMEDIATE STOP
        // ============================================================

        private void StopVFXImmediate()
        {
            if (airflowVFX != null)
            {
                airflowVFX.Stop();
                airflowVFX.enabled = true;
            }

            if (controlVFXObject != null)
                controlVFXObject.SetActive(false);
        }

        // ============================================================
        // TESTS
        // ============================================================

        [ContextMenu("Test OFF")]
        private void TestOff()
        {
            StopAirflow();
        }

        [ContextMenu("Test Cold Air")]
        private void TestColdAir()
        {
            SetAirflowStrength(0.45f);
            SetAirTemperature(0f);
        }

        [ContextMenu("Test Low Warm Air")]
        private void TestLowWarmAir()
        {
            SetAirflowStrength(0.45f);
            SetAirTemperature(0.45f);
        }

        [ContextMenu("Test High Hot Air")]
        private void TestHighHotAir()
        {
            SetAirflowStrength(1f);
            SetAirTemperature(1f);
        }

        [ContextMenu("Test Immediate High")]
        private void TestImmediateHigh()
        {
            SetAirflowImmediate(1f, 1f);
        }
    }
}
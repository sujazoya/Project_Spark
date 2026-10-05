using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Hair dryer visual controller.
    ///
    /// Reads the appliance electrical state and converts it into
    /// visual behavior.
    ///
    /// Electrical values are NOT calculated here.
    ///
    /// The electrical controller is the single source of truth for:
    /// - Motor voltage
    /// - Motor current
    /// - Motor conduction
    /// - Heater voltage
    /// - Heater current
    /// - Heater power
    /// - Heater conduction
    /// - Speed index
    ///
    /// This component controls only:
    /// - Fan rotation
    /// - Airflow VFX
    /// - Airflow strength
    /// - Air temperature
    /// - Heater visual
    /// - Thermal protection visual
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerVisualController : MonoBehaviour
    {
        // ============================================================
        // ELECTRICAL CONTROLLER
        // ============================================================

        [Header("Electrical")]
        [SerializeField]
        private SparkHairDryerElectricalController electricalController;

        // ============================================================
        // HEATER VISUAL
        // ============================================================

        [Header("Heater Visual")]
        [SerializeField]
        private SparkHairDryerHeaterVisual heaterVisual;

        // ============================================================
        // THERMAL PROTECTION
        // ============================================================

        [Header("Thermal Protection Visual")]
        [SerializeField]
        private SparkHairDryerThermalProtectionVisual thermalProtectionVisual;

        // ============================================================
        // FAN
        // ============================================================

        [Header("Fan")]
        [SerializeField]
        private Transform fanTransform;

        [SerializeField]
        private Vector3 localRotationAxis = Vector3.forward;

        [SerializeField, Min(0f)]
        private float runningRotationSpeed = 1800f;

        [SerializeField, Min(0f)]
        private float lowSpeedRotationMultiplier = 0.55f;

        [SerializeField, Min(0f)]
        private float highSpeedRotationMultiplier = 1f;

        [SerializeField]
        private bool reverseRotation;

        // ============================================================
        // AIRFLOW VFX
        // ============================================================

        [Header("Airflow VFX")]
        [SerializeField]
        private VisualEffect airflowVFX;

        [SerializeField]
        private GameObject controlVFXObject;

        [SerializeField]
        private Transform airflowDirection;

        // ============================================================
        // AIRFLOW
        // ============================================================

        [Header("Airflow")]
        [SerializeField, Range(0f, 1f)]
        private float lowSpeedAirflow = 0.45f;

        [SerializeField, Range(0f, 1f)]
        private float highSpeedAirflow = 1f;

        [SerializeField, Min(0f)]
        private float airflowAcceleration = 2.5f;

        [SerializeField, Min(0f)]
        private float airflowDeceleration = 4f;

        // ============================================================
        // TEMPERATURE
        // ============================================================

        [Header("Temperature")]
        [SerializeField, Range(0f, 1f)]
        private float coldAirTemperature = 0f;

        [SerializeField, Range(0f, 1f)]
        private float lowHeatTemperature = 0.45f;

        [SerializeField, Range(0f, 1f)]
        private float highHeatTemperature = 1f;

        [SerializeField, Min(0.000001f)]
        private float maximumHeaterPower = 1800f;

        [SerializeField, Min(0f)]
        private float temperatureResponse = 0.8f;

        // ============================================================
        // VFX PROPERTY NAMES
        // ============================================================

        [Header("VFX Properties")]
        [SerializeField]
        private string airflowStrengthProperty = "AirflowStrength";

        [SerializeField]
        private string airflowTemperatureProperty = "AirflowTemperature";

        [SerializeField]
        private string airflowDirectionProperty = "AirflowDirection";

        // ============================================================
        // PROPERTY IDS
        // ============================================================

        private int airflowStrengthID;
        private int airflowTemperatureID;
        private int airflowDirectionID;

        // ============================================================
        // RUNTIME VISUAL STATE
        // ============================================================

        private bool motorRunning;
        private bool heaterHeating;

        private int speedIndex;

        private float targetAirflowStrength;
        private float airflowStrength;

        private float targetAirTemperature;
        private float airTemperature;

        private bool airflowVFXPlaying;

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        public bool IsMotorRunning => motorRunning;

        public bool IsHeaterHeating => heaterHeating;

        public int SpeedIndex => speedIndex;

        public float AirflowStrength => airflowStrength;

        public float AirTemperature => airTemperature;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ResolveReferences();
            BuildPropertyIDs();
            ValidateRotationAxis();

            motorRunning = false;
            heaterHeating = false;
            speedIndex = 0;

            airflowStrength = 0f;
            targetAirflowStrength = 0f;

            airTemperature = 0f;
            targetAirTemperature = 0f;

            ApplyVFXProperties();
            StopVFXImmediate();
        }

        private void Update()
        {
            if (electricalController == null)
                return;

            /*
             * The electrical controller is responsible for obtaining
             * the actual solved electrical state.
             */
            electricalController.RefreshState();

            ReadElectricalState();

            UpdateFan();

            UpdateAirflowTargets();

            SmoothAirflow();

            SmoothTemperature();

            ApplyVFXProperties();

            UpdateVFXPlayback();
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            if (electricalController == null)
            {
                electricalController =
                    GetComponent<SparkHairDryerElectricalController>();
            }

            if (electricalController == null)
            {
                electricalController =
                    GetComponentInParent<SparkHairDryerElectricalController>();
            }

            if (electricalController == null)
            {
                electricalController =
                    GetComponentInChildren<
                        SparkHairDryerElectricalController>(true);
            }

            if (heaterVisual == null)
            {
                heaterVisual =
                    GetComponentInChildren<
                        SparkHairDryerHeaterVisual>(true);
            }

            if (thermalProtectionVisual == null)
            {
                thermalProtectionVisual =
                    GetComponentInChildren<
                        SparkHairDryerThermalProtectionVisual>(true);
            }

            if (fanTransform == null)
            {
                Transform fan =
                    transform.Find("Fan");

                if (fan != null)
                    fanTransform = fan;
            }

            if (controlVFXObject == null &&
                airflowVFX != null)
            {
                controlVFXObject =
                    airflowVFX.gameObject;
            }
        }

        private void BuildPropertyIDs()
        {
            airflowStrengthID =
                Shader.PropertyToID(
                    airflowStrengthProperty);

            airflowTemperatureID =
                Shader.PropertyToID(
                    airflowTemperatureProperty);

            airflowDirectionID =
                Shader.PropertyToID(
                    airflowDirectionProperty);
        }

        private void ValidateRotationAxis()
        {
            if (localRotationAxis.sqrMagnitude <=
                0.0001f)
            {
                localRotationAxis =
                    Vector3.forward;
            }

            localRotationAxis.Normalize();
        }

        // ============================================================
        // ELECTRICAL STATE
        // ============================================================

        private void ReadElectricalState()
        {
            /*
             * IMPORTANT:
             *
             * Do not calculate motor voltage/current here.
             *
             * Do not calculate heater voltage/current here.
             *
             * The electrical controller already owns those values.
             */
            motorRunning =
                electricalController.IsMotorRunning;

            heaterHeating =
                electricalController.IsHeaterHeating;

            speedIndex =
                Mathf.Clamp(
                    electricalController.SpeedIndex,
                    0,
                    2);

            Debug.Log(
                "[HAIR DRYER VISUAL] " +
                "Motor=" + motorRunning +
                " | Heater=" + heaterHeating +
                " | SpeedIndex=" + speedIndex +
                " | MotorVoltage=" +
                electricalController.MotorVoltage.ToString("F2") +
                " | MotorCurrent=" +
                electricalController.MotorCurrent.ToString("F4") +
                " | MotorConducting=" +
                electricalController.MotorConducting +
                " | HeaterVoltage=" +
                electricalController.HeaterVoltage.ToString("F2") +
                " | HeaterCurrent=" +
                electricalController.HeaterCurrent.ToString("F4") +
                " | HeaterPower=" +
                electricalController.HeaterPower.ToString("F2"),
                this);
        }

        // ============================================================
        // FAN
        // ============================================================

        private void UpdateFan()
        {
            if (fanTransform == null)
                return;

            if (!motorRunning)
                return;

            /*
             * SpeedIndex is a VISUAL speed selection.
             *
             * It does not calculate electrical voltage.
             *
             * ElectricalController already determined whether the
             * motor is actually running.
             */
            float multiplier =
                GetFanSpeedMultiplier();

            if (multiplier <= 0f)
                return;

            float degreesPerSecond =
                runningRotationSpeed *
                multiplier;

            float direction =
                reverseRotation
                    ? -1f
                    : 1f;

            fanTransform.Rotate(
                localRotationAxis,
                degreesPerSecond *
                direction *
                Time.deltaTime,
                Space.Self);
        }

        private float GetFanSpeedMultiplier()
        {
            switch (speedIndex)
            {
                case 0:
                    return 0f;

                case 1:
                    return Mathf.Max(
                        0f,
                        lowSpeedRotationMultiplier);

                case 2:
                    return Mathf.Max(
                        0f,
                        highSpeedRotationMultiplier);

                default:
                    return 0f;
            }
        }

        // ============================================================
        // AIRFLOW
        // ============================================================

        private void UpdateAirflowTargets()
        {
            /*
             * No motor = no airflow.
             */
            if (!motorRunning)
            {
                targetAirflowStrength = 0f;
                targetAirTemperature = 0f;

                UpdateHeaterVisual(0f);
                UpdateThermalProtectionVisual(0f);

                return;
            }

            /*
             * Motor is electrically running.
             */
            targetAirflowStrength =
                GetAirflowForSpeed();

            /*
             * Heater temperature comes from actual heater power.
             */
            float heaterTemperature =
                GetTemperatureForHeater();

            targetAirTemperature =
                heaterTemperature;

            UpdateHeaterVisual(
                heaterTemperature);

            UpdateThermalProtectionVisual(
                heaterTemperature);
        }

        private float GetAirflowForSpeed()
        {
            switch (speedIndex)
            {
                case 0:
                    return 0f;

                case 1:
                    return Mathf.Clamp01(
                        lowSpeedAirflow);

                case 2:
                    return Mathf.Clamp01(
                        highSpeedAirflow);

                default:
                    return 0f;
            }
        }

        // ============================================================
        // HEATER VISUAL
        // ============================================================

        private void UpdateHeaterVisual(
            float temperature)
        {
            if (heaterVisual == null)
                return;

            heaterVisual.SetTemperature(
                Mathf.Clamp01(temperature));
        }

        // ============================================================
        // TEMPERATURE
        // ============================================================

        private float GetTemperatureForHeater()
        {
            /*
             * No motor means no airflow and therefore no useful
             * heater visual.
             */
            if (!motorRunning)
                return 0f;

            /*
             * Heater electrically OFF.
             */
            if (!heaterHeating)
            {
                return Mathf.Clamp01(
                    coldAirTemperature);
            }

            /*
             * Heater power comes directly from the electrical
             * controller.
             */
            float heaterPower =
                Mathf.Max(
                    0f,
                    electricalController.HeaterPower);

            float normalizedPower =
                Mathf.Clamp01(
                    heaterPower /
                    Mathf.Max(
                        0.000001f,
                        maximumHeaterPower));

            return Mathf.Clamp01(
                Mathf.Lerp(
                    lowHeatTemperature,
                    highHeatTemperature,
                    normalizedPower));
        }

        // ============================================================
        // SMOOTHING
        // ============================================================

        private void SmoothAirflow()
        {
            float speed =
                targetAirflowStrength >
                airflowStrength
                    ? airflowAcceleration
                    : airflowDeceleration;

            airflowStrength =
                Mathf.MoveTowards(
                    airflowStrength,
                    targetAirflowStrength,
                    speed *
                    Time.deltaTime);
        }

        private void SmoothTemperature()
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

        private void ApplyVFXProperties()
        {
            if (airflowVFX == null)
                return;

            airflowVFX.SetFloat(
                airflowStrengthID,
                airflowStrength);

            airflowVFX.SetFloat(
                airflowTemperatureID,
                airTemperature);

            if (airflowDirection != null)
            {
                airflowVFX.SetVector3(
                    airflowDirectionID,
                    airflowDirection.forward);
            }
        }

        private void UpdateVFXPlayback()
        {
            if (airflowVFX == null)
                return;

            bool shouldRun =
                motorRunning &&
                targetAirflowStrength > 0.001f;

            if (shouldRun)
            {
                if (controlVFXObject != null &&
                    !controlVFXObject.activeSelf)
                {
                    controlVFXObject.SetActive(true);
                }

                if (!airflowVFX.enabled)
                {
                    airflowVFX.enabled = true;
                }

                /*
                 * Only call Play when transitioning from stopped
                 * to running.
                 */
                if (!airflowVFXPlaying)
                {
                    airflowVFX.Play();
                    airflowVFXPlaying = true;
                }

                return;
            }

            /*
             * Only stop when actually running.
             */
            if (airflowVFXPlaying)
            {
                airflowVFX.Stop();
                airflowVFXPlaying = false;
            }

            if (controlVFXObject != null &&
                controlVFXObject.activeSelf)
            {
                controlVFXObject.SetActive(false);
            }
        }

        private void StopVFXImmediate()
        {
            airflowVFXPlaying = false;

            if (airflowVFX != null)
            {
                airflowVFX.Stop();
                airflowVFX.enabled = true;
            }

            if (controlVFXObject != null)
                controlVFXObject.SetActive(false);
        }

        // ============================================================
        // THERMAL PROTECTION
        // ============================================================

        private void UpdateThermalProtectionVisual(
            float temperature)
        {
            if (thermalProtectionVisual == null)
                return;

            thermalProtectionVisual.SetTemperature(
                Mathf.Clamp01(temperature));
        }

        // ============================================================
        // PUBLIC CONTROL
        // ============================================================

        public void SetElectricalController(
            SparkHairDryerElectricalController controller)
        {
            electricalController = controller;
        }

        public void SetFanTransform(
            Transform fan)
        {
            fanTransform = fan;
        }

        public void SetAirflowVFX(
            VisualEffect vfx)
        {
            airflowVFX = vfx;

            airflowVFXPlaying = false;

            if (controlVFXObject == null &&
                airflowVFX != null)
            {
                controlVFXObject =
                    airflowVFX.gameObject;
            }

            BuildPropertyIDs();
            ApplyVFXProperties();
        }

        public void SetAirflowDirection(
            Transform direction)
        {
            airflowDirection = direction;
        }

        public void RefreshVisuals()
        {
            if (electricalController == null)
                return;

            electricalController.RefreshState();

            ReadElectricalState();

            UpdateAirflowTargets();

            ApplyVFXProperties();

            UpdateVFXPlayback();
        }

        // ============================================================
        // TEST COMMANDS
        // ============================================================

        [ContextMenu("Refresh Visuals")]
        private void TestRefresh()
        {
            RefreshVisuals();
        }

        [ContextMenu("Stop Visuals")]
        private void TestStop()
        {
            motorRunning = false;
            heaterHeating = false;
            speedIndex = 0;

            targetAirflowStrength = 0f;
            targetAirTemperature = 0f;

            airflowStrength = 0f;
            airTemperature = 0f;

            UpdateHeaterVisual(0f);
            UpdateThermalProtectionVisual(0f);

            StopVFXImmediate();
        }

        [ContextMenu("Test Fan Rotation")]
        private void TestFanRotation()
        {
            if (fanTransform == null)
            {
                Debug.LogWarning(
                    "[Hair Dryer Visual] Fan Transform is not assigned.",
                    this);

                return;
            }

            fanTransform.Rotate(
                localRotationAxis,
                45f,
                Space.Self);
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private void OnValidate()
        {
            runningRotationSpeed =
                Mathf.Max(
                    0f,
                    runningRotationSpeed);

            lowSpeedRotationMultiplier =
                Mathf.Max(
                    0f,
                    lowSpeedRotationMultiplier);

            highSpeedRotationMultiplier =
                Mathf.Max(
                    0f,
                    highSpeedRotationMultiplier);

            airflowAcceleration =
                Mathf.Max(
                    0f,
                    airflowAcceleration);

            airflowDeceleration =
                Mathf.Max(
                    0f,
                    airflowDeceleration);

            temperatureResponse =
                Mathf.Max(
                    0f,
                    temperatureResponse);

            maximumHeaterPower =
                Mathf.Max(
                    0.000001f,
                    maximumHeaterPower);

            ValidateRotationAxis();
        }
    }
}
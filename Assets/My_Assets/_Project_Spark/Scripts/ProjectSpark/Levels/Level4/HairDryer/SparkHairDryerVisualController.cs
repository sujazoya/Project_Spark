using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual coordinator for the Project Spark hair dryer.
    ///
    /// IMPORTANT:
    /// This component does NOT control the electrical circuit.
    ///
    /// Electrical state comes from:
    ///     SparkHairDryerMotorElectrical
    ///     SparkHairDryerHeaterElectrical
    ///
    /// Visual state:
    ///     Motor rotation
    ///     Airflow VFX
    ///     Heater visual
    ///
    /// There is intentionally no SparkHairDryer electrical owner.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerVisualController : MonoBehaviour
    {
        // ============================================================
        // ELECTRICAL COMPONENTS
        // ============================================================

        [Header("Electrical State")]
        [SerializeField]
        private SparkHairDryerMotorElectrical motorElectrical;

        [SerializeField]
        private SparkHairDryerHeaterElectrical heaterElectrical;

        // ============================================================
        // MOTOR VISUAL
        // ============================================================

        [Header("Motor Visual")]
        [SerializeField]
        private SparkHairDryerMotorVisual motorVisual;

        // ============================================================
        // AIRFLOW
        // ============================================================

        [Header("Airflow VFX")]
        [SerializeField]
        private VisualEffect airflowVfx;

        [SerializeField]
        private bool disableAirflowObjectWhenOff;

        // ============================================================
        // HEATER VISUAL
        // ============================================================

        [Header("Heater Visual")]
        [SerializeField]
        private GameObject heaterVisual;

        [SerializeField]
        private bool disableHeaterObjectWhenOff;

        // ============================================================
        // HEATER MATERIAL
        // ============================================================

        [Header("Heater Material")]
        [SerializeField]
        private Renderer heaterRenderer;

        [SerializeField]
        private string heaterIntensityProperty = "_EmissionIntensity";

        [SerializeField, Min(0f)]
        private float heaterOffIntensity = 0f;

        [SerializeField, Min(0f)]
        private float heaterOnIntensity = 1f;

        // ============================================================
        // RUNTIME STATE
        // ============================================================

        private bool lastMotorRunning;
        private bool lastHeaterRunning;
        private bool initialized;

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        public bool IsMotorRunning =>
            motorElectrical != null &&
            motorElectrical.IsRunning;

        public bool IsHeating =>
            heaterElectrical != null &&
            heaterElectrical.IsHeating;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            AutoAssignReferences();
            ApplyVisualState();
        }

        private void Update()
        {
            RefreshVisualState();
        }

        // ============================================================
        // REFERENCE SETUP
        // ============================================================

        private void AutoAssignReferences()
        {
            if (motorElectrical == null)
            {
                motorElectrical =
                    GetComponentInChildren<
                        SparkHairDryerMotorElectrical>(true);
            }

            if (heaterElectrical == null)
            {
                heaterElectrical =
                    GetComponentInChildren<
                        SparkHairDryerHeaterElectrical>(true);
            }

            if (motorVisual == null)
            {
                motorVisual =
                    GetComponentInChildren<
                        SparkHairDryerMotorVisual>(true);
            }
        }

        // ============================================================
        // STATE REFRESH
        // ============================================================

        private void RefreshVisualState()
        {
            bool motorRunning = IsMotorRunning;
            bool heating = IsHeating;

            if (!initialized ||
                motorRunning != lastMotorRunning ||
                heating != lastHeaterRunning)
            {
                ApplyMotorVisual(motorRunning);
                ApplyHeaterVisual(heating);

                lastMotorRunning = motorRunning;
                lastHeaterRunning = heating;

                initialized = true;
            }
        }

        // ============================================================
        // APPLY ALL
        // ============================================================

        private void ApplyVisualState()
        {
            bool motorRunning = IsMotorRunning;
            bool heating = IsHeating;

            ApplyMotorVisual(motorRunning);
            ApplyHeaterVisual(heating);

            lastMotorRunning = motorRunning;
            lastHeaterRunning = heating;

            initialized = true;
        }

        // ============================================================
        // MOTOR
        // ============================================================

        private void ApplyMotorVisual(bool running)
        {
            if (motorVisual != null)
                motorVisual.SetRunning(running);

            SetAirflow(running);
        }

        // ============================================================
        // AIRFLOW
        // ============================================================

        private void SetAirflow(bool active)
        {
            if (airflowVfx == null)
                return;

            if (disableAirflowObjectWhenOff)
            {
                if (airflowVfx.gameObject.activeSelf != active)
                {
                    airflowVfx.gameObject.SetActive(active);
                }

                if (!active)
                    airflowVfx.Stop();

                return;
            }

            if (active)
            {
                if (!airflowVfx.enabled)
                    airflowVfx.enabled = true;

                airflowVfx.Play();
            }
            else
            {
                airflowVfx.Stop();

                if (airflowVfx.enabled)
                    airflowVfx.enabled = false;
            }
        }

        // ============================================================
        // HEATER
        // ============================================================

        private void ApplyHeaterVisual(bool heating)
        {
            if (heaterVisual != null &&
                disableHeaterObjectWhenOff)
            {
                if (heaterVisual.activeSelf != heating)
                {
                    heaterVisual.SetActive(heating);
                }
            }

            if (heaterRenderer == null)
                return;

            Material material = heaterRenderer.material;

            if (material == null)
                return;

            if (!material.HasProperty(heaterIntensityProperty))
                return;

            material.SetFloat(
                heaterIntensityProperty,
                heating
                    ? heaterOnIntensity
                    : heaterOffIntensity);
        }

        // ============================================================
        // PUBLIC REFERENCE SETTERS
        // ============================================================

        public void SetMotorElectrical(
            SparkHairDryerMotorElectrical motor)
        {
            motorElectrical = motor;
            ApplyVisualState();
        }

        public void SetHeaterElectrical(
            SparkHairDryerHeaterElectrical heater)
        {
            heaterElectrical = heater;
            ApplyVisualState();
        }

        public void SetMotorVisual(
            SparkHairDryerMotorVisual visual)
        {
            motorVisual = visual;
            ApplyVisualState();
        }

        public void SetAirflowVfx(
            VisualEffect vfx)
        {
            airflowVfx = vfx;

            if (airflowVfx != null)
                SetAirflow(IsMotorRunning);
        }

        public void SetHeaterVisual(
            GameObject visual)
        {
            heaterVisual = visual;
            ApplyHeaterVisual(IsHeating);
        }

        // ============================================================
        // DEBUG / TEST
        // ============================================================

        [ContextMenu("Refresh Visual State")]
        private void TestRefreshVisualState()
        {
            ApplyVisualState();
        }
    }
}

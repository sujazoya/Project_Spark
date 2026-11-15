using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// ART-ONLY Level 72 ALD rig adapter. Connect externally computed simulation values.
    /// FBXs contain independent meshes and pivot location references, NOT completed skeletal rigs.
    /// Assign separately split/reparented animated child meshes under correctly positioned pivots.
    /// No electrical, valve, vacuum, film, or gameplay authority is provided here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel72VisualRig : MonoBehaviour
    {
        [Header("Bind custom moving submeshes, not entire equipment models")]
        [SerializeField] private Transform turboRotor;
        [SerializeField] private Transform backingRotor;
        [SerializeField] private Transform loadLockGate;
        [SerializeField] private Transform isolationValve;
        [SerializeField] private Transform throttleValve;
        [SerializeField] private Transform reactorLid;
        [SerializeField] private Transform waferLift;
        [SerializeField] private Transform waferRobotBase;
        [SerializeField] private Transform waferRobotArm;
        [SerializeField] private Transform waferRobotTool;
        [SerializeField] private Transform pulseValveA;
        [SerializeField] private Transform pulseValveB;
        [SerializeField] private Transform purgeValve;
        [SerializeField] private Transform metrologyHead;
        [SerializeField] private Renderer[] filmRenderers;
        [SerializeField] private Light chamberLight;
        [SerializeField] private ParticleSystem cyanDoseVFX;
        [SerializeField] private ParticleSystem magentaDoseVFX;
        [SerializeField] private ParticleSystem whitePurgeVFX;

        private float turboRPM, backingRPM, gate, isolation, throttle, lid, lift;
        private float baseDegrees, armDegrees, toolDegrees, doseA, doseB, purge, layer, metrology;
        private int stage;
        private Vector3 gateHome, liftHome;
        private Quaternion isolationHome, throttleHome, lidHome, robotBaseHome, robotArmHome,
            robotToolHome, valveAHome, valveBHome, purgeHome, metrologyHome;

        private void Awake()
        {
            if (loadLockGate != null) gateHome = loadLockGate.localPosition;
            if (waferLift != null) liftHome = waferLift.localPosition;
            isolationHome = LocalRotation(isolationValve);
            throttleHome = LocalRotation(throttleValve);
            lidHome = LocalRotation(reactorLid);
            robotBaseHome = LocalRotation(waferRobotBase);
            robotArmHome = LocalRotation(waferRobotArm);
            robotToolHome = LocalRotation(waferRobotTool);
            valveAHome = LocalRotation(pulseValveA);
            valveBHome = LocalRotation(pulseValveB);
            purgeHome = LocalRotation(purgeValve);
            metrologyHome = LocalRotation(metrologyHead);
        }
        private static Quaternion LocalRotation(Transform item) => item != null ? item.localRotation : Quaternion.identity;

        /// <summary>Units RPM. Authoritative pump state is owned by external gameplay code.</summary>
        public void SetVacuumRPM(float turbo, float backing)
        {
            turboRPM = Mathf.Max(0, turbo);
            backingRPM = Mathf.Max(0, backing);
        }

        /// <summary>Actual positions, normalized 0..1. Avoid passing requests instead of feedback.</summary>
        public void SetValveOpenFractions(float loadLock, float isolationFraction, float throttleFraction,
            float precursorA, float precursorB, float purgeFraction)
        {
            gate = Mathf.Clamp01(loadLock);
            isolation = Mathf.Clamp01(isolationFraction);
            throttle = Mathf.Clamp01(throttleFraction);
            doseA = Mathf.Clamp01(precursorA);
            doseB = Mathf.Clamp01(precursorB);
            purge = Mathf.Clamp01(purgeFraction);
        }

        public void SetWaferHandlingPose(float lidOpen, float liftPosition, float baseDeg, float armDeg, float toolDeg)
        {
            lid = Mathf.Clamp01(lidOpen);
            lift = Mathf.Clamp01(liftPosition);
            baseDegrees = baseDeg;
            armDegrees = armDeg;
            toolDegrees = toolDeg;
        }

        /// <summary>Stage 0=idle, 1=A dose, 2=A purge, 3=B dose, 4=B purge.</summary>
        public void SetCycleVisualization(int currentStage, float actualLayerProgress01)
        {
            stage = Mathf.Clamp(currentStage, 0, 4);
            layer = Mathf.Clamp01(actualLayerProgress01);
        }
        public void SetMetrologyPosition(float normalizedPosition) { metrology = Mathf.Clamp01(normalizedPosition); }

        private static void Spin(Transform t, float rpm, float dt)
        {
            if (t != null) t.Rotate(Vector3.right, rpm * 6f * dt, Space.Self);
        }
        private static void SetVFX(ParticleSystem vfx, bool on)
        {
            if (vfx == null) return;
            if (on && !vfx.isPlaying) vfx.Play();
            else if (!on && vfx.isPlaying) vfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Spin(turboRotor, turboRPM, dt);
            Spin(backingRotor, backingRPM, dt);
            if (loadLockGate != null) loadLockGate.localPosition = gateHome + Vector3.up * (.58f * gate);
            if (waferLift != null) waferLift.localPosition = liftHome + Vector3.up * (.18f * lift);
            if (isolationValve != null) isolationValve.localRotation = isolationHome * Quaternion.Euler(0f, isolation * 90f, 0f);
            if (throttleValve != null) throttleValve.localRotation = throttleHome * Quaternion.Euler(0f, throttle * 85f, 0f);
            if (reactorLid != null) reactorLid.localRotation = lidHome * Quaternion.Euler(lid * 58f, 0f, 0f);
            if (waferRobotBase != null) waferRobotBase.localRotation = robotBaseHome * Quaternion.Euler(0f, baseDegrees, 0f);
            if (waferRobotArm != null) waferRobotArm.localRotation = robotArmHome * Quaternion.Euler(0f, 0f, armDegrees);
            if (waferRobotTool != null) waferRobotTool.localRotation = robotToolHome * Quaternion.Euler(0f, 0f, toolDegrees);
            if (pulseValveA != null) pulseValveA.localRotation = valveAHome * Quaternion.Euler(0f, doseA * 65f, 0f);
            if (pulseValveB != null) pulseValveB.localRotation = valveBHome * Quaternion.Euler(0f, doseB * 65f, 0f);
            if (purgeValve != null) purgeValve.localRotation = purgeHome * Quaternion.Euler(0f, purge * 65f, 0f);
            if (metrologyHead != null) metrologyHead.localRotation = metrologyHome * Quaternion.Euler(0f, metrology * 55f, 0f);
            SetVFX(cyanDoseVFX, stage == 1);
            SetVFX(magentaDoseVFX, stage == 3);
            SetVFX(whitePurgeVFX, stage == 2 || stage == 4);
            if (chamberLight != null)
            {
                chamberLight.enabled = stage != 0;
                chamberLight.color = stage == 1 ? new Color(.18f, .85f, 1f) :
                    stage == 3 ? new Color(.94f, .37f, 1f) : Color.white;
                chamberLight.intensity = stage == 0 ? 0 : 1.6f;
            }
            // Film progress is a visual signal only; no film thickness is simulated in this script.
            if (filmRenderers != null)
                foreach (Renderer r in filmRenderers)
                    if (r != null) r.enabled = layer > .004f;
        }
    }
}

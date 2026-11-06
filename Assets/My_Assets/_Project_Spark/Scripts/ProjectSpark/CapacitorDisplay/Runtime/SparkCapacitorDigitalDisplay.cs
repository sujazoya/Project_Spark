
using System;
using ProjectSpark.Electrical;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Read-only, solver-backed capacitor telemetry.
    ///
    /// - Reads only completed solver results.
    /// - Does not reject readings simply because the next
    ///   transient solve is already pending.
    /// - Recovers from late subscription or level switching.
    /// - Never modifies circuit topology or capacitor history.
    /// - Never refreshes freshness timestamps without a solve.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkCapacitorDigitalDisplay : MonoBehaviour
    {
        public enum CapacitorStatus
        {
            NoDevice,
            AwaitingSolve,
            Disconnected,
            Disabled,
            Charging,
            Discharging,
            Holding,
            OverVoltage
        }

        [Header("Actual solver-backed device")]
        [SerializeField] private SparkCapacitor capacitor;
        [SerializeField] private SparkElectricalSolver solver;

        [Header("Display reference")]
        [SerializeField, Min(0.001f)]
        private float targetChargeVoltage = 5f;

        [SerializeField, Min(0.001f)]
        private float maxAcceptedDataAgeSeconds = 1.5f;

        [SerializeField, Min(0f)]
        private float currentDeadbandAmps = 0.000005f;

        [SerializeField]
        private bool automaticallyResolveReferences = true;

        [Header("Diagnostics")]
        [SerializeField]
        private bool logRejectedReadings = false;

        private SparkElectricalSolver subscribedSolver;
        private SparkCapacitor trackedCapacitor;

        private object lastCapturedResult;
        private bool capturePending = true;
        private bool accepted;

        private float lastSolveTime = -1f;
        private float voltage;
        private float current;
        private float power;

        private float nextResolveTime;

        public SparkCapacitor Capacitor => capacitor;
        public SparkElectricalSolver Solver => solver;

        public float Voltage => voltage;
        public float Current => current;
        public float Power => power;

        public float CapacitanceFarads =>
            capacitor != null ? capacitor.CapacitanceFarads : 0f;

        public float RatedVoltage =>
            capacitor != null ? capacitor.VoltageRating : 0f;

        public float TargetChargeVoltage => targetChargeVoltage;

        public float ChargeFraction =>
            HasValidReading && targetChargeVoltage > 0f
                ? Mathf.Clamp01(
                    Mathf.Abs(voltage) / targetChargeVoltage)
                : 0f;

        public float StoredEnergyJoules =>
            HasValidReading
                ? 0.5f * CapacitanceFarads * voltage * voltage
                : 0f;

        public float ChargeCoulombs =>
            HasValidReading
                ? CapacitanceFarads * voltage
                : 0f;

        public float LastSolveAgeSeconds =>
            lastSolveTime < 0f
                ? float.PositiveInfinity
                : Mathf.Max(
                    0f,
                    Time.unscaledTime - lastSolveTime);

        public bool HasValidReading =>
            accepted &&
            capacitor != null &&
            solver != null &&
            capacitor.isActiveAndEnabled &&
            capacitor.ElectricalEnabled &&
            LastSolveAgeSeconds <= maxAcceptedDataAgeSeconds;

        public CapacitorStatus Status
        {
            get
            {
                if (capacitor == null)
                    return CapacitorStatus.NoDevice;

                if (!capacitor.isActiveAndEnabled ||
                    !capacitor.ElectricalEnabled)
                    return CapacitorStatus.Disabled;

                if (!HasValidReading)
                    return CapacitorStatus.AwaitingSolve;

                if (capacitor.IsOverVoltage)
                    return CapacitorStatus.OverVoltage;

                if (current > currentDeadbandAmps)
                    return CapacitorStatus.Charging;

                if (current < -currentDeadbandAmps)
                    return CapacitorStatus.Discharging;

                return CapacitorStatus.Holding;
            }
        }

        public event Action ReadingChanged;

        private void Awake()
        {
            Resolve();
        }

        private void OnEnable()
        {
            Resolve();
            SynchronizeSubscription();

            capturePending = true;
            nextResolveTime = 0f;
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            SparkCapacitor device,
            SparkElectricalSolver electricalSolver)
        {
            Unsubscribe();

            capacitor = device;
            solver = electricalSolver;

            trackedCapacitor = capacitor;
            ResetReading();

            SynchronizeSubscription();

            capturePending = true;
            ReadingChanged?.Invoke();
        }

        public void SetChargeTargetVoltage(float volts)
        {
            if (!Finite(volts) || volts <= 0f)
                return;

            targetChargeVoltage = volts;
            ReadingChanged?.Invoke();
        }

        private void Resolve()
        {
            if (!automaticallyResolveReferences)
                return;

            if (capacitor == null)
                capacitor = GetComponent<SparkCapacitor>();

            if (capacitor == null)
                capacitor =
                    GetComponentInChildren<SparkCapacitor>(true);

            if (solver == null)
                solver =
                    FindFirstObjectByType<SparkElectricalSolver>();
        }

        private void SynchronizeSubscription()
        {
            if (subscribedSolver == solver)
                return;

            Unsubscribe();

            if (!isActiveAndEnabled || solver == null)
                return;

            subscribedSolver = solver;

            subscribedSolver.SolveCompleted += OnSolveCompleted;
            subscribedSolver.SolveFailed += OnSolveFailed;

            capturePending = true;
        }

        private void Unsubscribe()
        {
            if (subscribedSolver == null)
                return;

            subscribedSolver.SolveCompleted -= OnSolveCompleted;
            subscribedSolver.SolveFailed -= OnSolveFailed;

            subscribedSolver = null;
        }

        private void OnSolveCompleted()
        {
            // Do not read immediately inside the event.
            // Capture in LateUpdate after solver processing.
            capturePending = true;
        }

        private void OnSolveFailed()
        {
            accepted = false;
            capturePending = false;

            // Do not interpret an older result as a new solve.
            if (solver != null)
                lastCapturedResult = solver.LatestSolveResult;

            ReadingChanged?.Invoke();
        }

        private void LateUpdate()
        {
            // Recover references after level layout switching.
            if (automaticallyResolveReferences &&
                Time.unscaledTime >= nextResolveTime)
            {
                nextResolveTime = Time.unscaledTime + 0.5f;

                Resolve();
                SynchronizeSubscription();
            }

            if (trackedCapacitor != capacitor)
            {
                trackedCapacitor = capacitor;
                ResetReading();
                capturePending = true;
            }

            if (solver == null || capacitor == null)
                return;

            // Never capture while the solver is still calculating.
            if (solver.IsSolving)
                return;

            object result = solver.LatestSolveResult;

            if (result == null)
                return;

            // Only a new solve result or an actual completion
            // notification may refresh the reading timestamp.
            if (!capturePending &&
                ReferenceEquals(result, lastCapturedResult))
                return;

            TryCapture(result);
        }

        private void TryCapture(object result)
        {
            if (solver == null ||
                capacitor == null ||
                result == null)
                return;

            if (!capacitor.isActiveAndEnabled ||
                !capacitor.ElectricalEnabled)
            {
                accepted = false;
                capturePending = false;
                ReadingChanged?.Invoke();
                return;
            }

            bool discovered = false;
            var components = solver.ElectricalComponents;

            if (components == null)
                return;

            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] != capacitor)
                    continue;

                discovered = true;
                break;
            }

            if (!discovered)
            {
                // Allow another attempt after the active layout
                // and solver component list have synchronized.
                accepted = false;
                ReadingChanged?.Invoke();
                return;
            }

            SparkElectricalState state =
                capacitor.ElectricalState;

            if (!Finite(state.Voltage) ||
                !Finite(state.Current) ||
                !Finite(state.Power))
            {
                accepted = false;
                capturePending = false;

                if (logRejectedReadings)
                {
                    Debug.LogWarning(
                        "[CAP DISPLAY] Non-finite capacitor state. " +
                        capacitor.name,
                        this);
                }

                ReadingChanged?.Invoke();
                return;
            }

            voltage = state.Voltage;
            current = state.Current;
            power = state.Power;

            lastCapturedResult = result;
            lastSolveTime = Time.unscaledTime;

            accepted = true;
            capturePending = false;

            ReadingChanged?.Invoke();
        }

        private void ResetReading()
        {
            accepted = false;
            lastCapturedResult = null;
            lastSolveTime = -1f;

            voltage = 0f;
            current = 0f;
            power = 0f;
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }

        private void OnValidate()
        {
            targetChargeVoltage = Mathf.Max(
                0.001f,
                targetChargeVoltage);

            maxAcceptedDataAgeSeconds = Mathf.Max(
                0.001f,
                maxAcceptedDataAgeSeconds);

            currentDeadbandAmps = Mathf.Max(
                0f,
                currentDeadbandAmps);
        }

        [ContextMenu("Capacitor Display / Log Diagnostic")]
        public void LogDiagnostic()
        {
            Debug.Log(
                "[CAP DISPLAY DIAGNOSTIC]\n" +
                "Device: " +
                    (capacitor != null
                        ? capacitor.name
                        : "MISSING") + "\n" +
                "Solver: " +
                    (solver != null
                        ? solver.name
                        : "MISSING") + "\n" +
                "Subscribed: " +
                    (subscribedSolver == solver) + "\n" +
                "Accepted: " + accepted + "\n" +
                "Valid Reading: " + HasValidReading + "\n" +
                "Status: " + Status + "\n" +
                "Voltage: " + voltage + " V\n" +
                "Current: " + current + " A\n" +
                "Power: " + power + " W\n" +
                "Capacitance: " + CapacitanceFarads + " F\n" +
                "Stored Energy: " + StoredEnergyJoules + " J\n" +
                "Target Voltage: " + targetChargeVoltage + " V\n" +
                "Last Solve Age: " +
                    LastSolveAgeSeconds + " s\n" +
                "Solver Dirty: " +
                    (solver != null && solver.IsDirty) + "\n" +
                "Solver Solving: " +
                    (solver != null && solver.IsSolving) + "\n" +
                "Capture Pending: " + capturePending,
                this);
        }
    }
}

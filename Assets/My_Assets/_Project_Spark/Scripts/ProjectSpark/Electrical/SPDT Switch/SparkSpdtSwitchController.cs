using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Two-position SPDT switch: COM->A (Charge), or COM->B (Backup).
    /// Mechanical state and player interaction only. A circuit router must implement
    /// ISparkSpdtCircuitRouter to apply the state to the actual electrical graph.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSpdtSwitchController : MonoBehaviour
    {
        public enum Position { ChargeA, BackupB }

        [Header("Electrical routing provider")]
        [Tooltip("Assign a component implementing ISparkSpdtCircuitRouter. Without one, only visuals/state change.")]
        [SerializeField] private MonoBehaviour circuitRouter;

        [Header("3 physical terminal anchors (optional; for inspector/visual reference)")]
        [SerializeField] private Transform commonTerminal;
        [SerializeField] private Transform terminalA;
        [SerializeField] private Transform terminalB;

        [Header("Lever animation")]
        [SerializeField] private Transform leverPivot;
        [SerializeField] private Vector3 localRotationAxis = Vector3.forward;
        [SerializeField, Range(0f, 55f)] private float leverAngle = 24f;
        [SerializeField, Min(1f)] private float leverSpeedDegreesPerSecond = 240f;
        [SerializeField] private Position initialPosition = Position.ChargeA;
        [SerializeField] private bool initializeRouterOnStart = true;

        [Header("Interaction")]
        [SerializeField, HideInInspector] private bool allowMouseClick = false;
        [SerializeField] private bool logChanges = false;
        [SerializeField] private UnityEvent onChargeSelected;
        [SerializeField] private UnityEvent onBackupSelected;

        private Quaternion leverRestRotation;
        private Position currentPosition;
        private ISparkSpdtCircuitRouter router;
        private bool initialized;

        public Position CurrentPosition => currentPosition;
        public bool IsCharge => currentPosition == Position.ChargeA;
        public bool IsBackup => currentPosition == Position.BackupB;
        public Transform CommonTerminal => commonTerminal;
        public Transform TerminalA => terminalA;
        public Transform TerminalB => terminalB;
        public bool HasElectricalRouter => router != null;

        public event Action<Position> PositionChanged;

        private void Awake()
        {
            router = circuitRouter as ISparkSpdtCircuitRouter;
            if (circuitRouter != null && router == null)
                Debug.LogError("[SPDT] Circuit Router must implement ISparkSpdtCircuitRouter.", this);

            leverRestRotation = leverPivot != null ? leverPivot.localRotation : Quaternion.identity;
            currentPosition = initialPosition;
            initialized = true;
            SetLeverInstant(currentPosition);
        }

        private void Start()
        {
            if (router == null)
                Debug.LogWarning("[SPDT] No electrical router assigned. Toggle animates but DOES NOT switch the circuit.", this);
        }

        private void Update()
        {
            if (!initialized || leverPivot == null) return;
            var target = GetLeverRotation(currentPosition);
            leverPivot.localRotation = Quaternion.RotateTowards(
                leverPivot.localRotation, target,
                leverSpeedDegreesPerSecond * Time.deltaTime);
        }

        private void OnEnable()
        {
            if (!initialized) return;
            SnapLeverToState();
            if (initializeRouterOnStart) StartCoroutine(RestoreAfterActivation());
        }

        private IEnumerator RestoreAfterActivation()
        {
            // A parent's OnEnable can run before its children finish activation.
            // Wait until all Awake/OnEnable/Start callbacks in this activation finish.
            yield return null;
            if (isActiveAndEnabled) ApplyElectricalState();
        }

        private void OnDisable() { StopAllCoroutines(); }

        [ContextMenu("Toggle SPDT")]
        public void Toggle() => SetPosition(IsCharge ? Position.BackupB : Position.ChargeA);

        public void SelectCharge() => SetPosition(Position.ChargeA);
        public void SelectBackup() => SetPosition(Position.BackupB);

        public void SetPosition(Position position)
        {
            if (position != Position.ChargeA && position != Position.BackupB) return;
            var dual = circuitRouter as SparkSpdtDualSwitchRouter;
            if (dual == null || !dual.IsReady)
            {
                Debug.LogError("[SPDT] Router is not ready; position was not changed.", this);
                return;
            }
            if (currentPosition == position) { ApplyElectricalState(); return; }
            currentPosition = position;
            ApplyElectricalState();
            PositionChanged?.Invoke(position);
            if (position == Position.ChargeA) onChargeSelected?.Invoke();
            else onBackupSelected?.Invoke();
            if (logChanges) Debug.Log("[SPDT] " + position, this);
        }

        public void ApplyElectricalState()
        {
            if (router == null && circuitRouter != null)
                router = circuitRouter as ISparkSpdtCircuitRouter;
            if (router != null)
                router.SetSpdtConnection(currentPosition == Position.ChargeA);
        }

        public void SnapLeverToState()
        {
            if (leverPivot != null) leverPivot.localRotation = GetLeverRotation(currentPosition);
        }

        private void SetLeverInstant(Position position)
        {
            if (leverPivot != null) leverPivot.localRotation = GetLeverRotation(position);
        }

        private Quaternion GetLeverRotation(Position position)
        {
            Vector3 axis = localRotationAxis.sqrMagnitude > .0001f
                ? localRotationAxis.normalized : Vector3.forward;
            float signedAngle = position == Position.ChargeA ? -leverAngle : leverAngle;
            return leverRestRotation * Quaternion.AngleAxis(signedAngle, axis);
        }
    }

    /// <summary>
    /// Implement on a MonoBehaviour to connect COM-A when chargeSelected is true;
    /// otherwise COM-B. Your implementation must disconnect the previous path first,
    /// then notify/recalculate your circuit. Never enable both throws simultaneously.
    /// </summary>
    public interface ISparkSpdtCircuitRouter
    {
        void SetSpdtConnection(bool chargeSelected);
    }
}

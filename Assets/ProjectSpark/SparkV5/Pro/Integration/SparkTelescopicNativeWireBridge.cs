using System;
using System.Collections;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Optional adapter between the original telescopic hand and the existing
    /// cinematic native-wire route. Never connects wires or edits the circuit.
    /// A known compatible cinematic executor must call its explicit methods.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkTelescopicNativeWireBridge : MonoBehaviour
    {
        [SerializeField] private SparkTelescopicArms arms;
        [SerializeField] private bool enableCinematicExtension = true;
        [SerializeField, Range(0.5f, 12f)] private float maximumContactWait = 5f;
        [SerializeField, Range(0.001f, 0.03f)] private float maximumNativeGripMismatch = 0.012f;

        private bool ownsRightArm;
        public bool LastContactSucceeded { get; private set; }
        public string LastError { get; private set; } = "No cinematic contact requested";
        public bool IsContactActive => ownsRightArm && arms && arms.IsActive;
        public bool CinematicExtensionEnabled => enableCinematicExtension;
        public bool IsConfigured => arms && arms.IsBound;
        public float ContactWaitSeconds => maximumContactWait;
        public bool IsRightArmResting => !arms || arms.IsRightArmResting;
        public string ReachDiagnostic => arms ? arms.LastError : "Original arms are unbound";
        internal bool NativeGripSettled => arms && arms.NativeGripSettled;
        internal float GripTransitionSeconds => arms ? arms.NativeGripTransitionSeconds : .22f;
        internal float GripAimError => arms ? arms.NativeGripAimError : float.PositiveInfinity;
        internal bool SetGrip(float closure, float diameter, Vector3 aim = default) =>
            IsContactActive && arms.SetNativeGrip(closure, diameter, aim);

        private void Awake()
        {
            if (!arms) arms = GetComponent<SparkTelescopicArms>();
        }

        public bool CanDrawTo(Transform target)
        {
            return enabled && enableCinematicExtension && arms &&
                   arms.CanDrawToCinematicContact(target);
        }

        internal bool CanDrawTo(Transform target, Vector3 offset) =>
            IsContactActive && arms.CanDrawToCinematicContact(target, offset);

        public bool CanExtendTo(Transform target)
        {
            return enabled && enableCinematicExtension && arms &&
                   arms.CanExtendToCinematicContact(target);
        }

        public bool CanReachWithOriginalHand(Transform target) =>
            isActiveAndEnabled && enableCinematicExtension && arms &&
            arms.CanExtendToCinematicContact(target, requireExtension: false);

        internal bool CanReachFrom(Vector3 shoulder, Transform target) =>
            isActiveAndEnabled && enableCinematicExtension && arms && arms.CanReachCinematicFrom(shoulder, target);

        internal bool SetCarryTarget(Vector3 target)
        {
            if (!IsContactActive) return false;
            arms.FollowCinematicGrip(target);
            return true;
        }

        /// <summary>Contact is verified against ORIGINAL fingertip grip, not wrist.</summary>
        public IEnumerator ReachTerminal(Transform terminal, Func<bool> stillValid, bool prepareGrip = false)
        {
            LastContactSucceeded = false;
            LastError = "Reach was not started";
            if (!enabled || !enableCinematicExtension || !arms || !terminal || stillValid == null)
            { LastError = "Telescopic bridge is not configured"; yield break; }
            if (!stillValid()) { LastError = "Native wiring context changed"; yield break; }
            if (!arms.TryReachCinematicContact(terminal, requireExtension: false))
            { LastError = arms.LastError; yield break; }
            ownsRightArm = true;
            if (prepareGrip && !SetGrip(0f, 0f))
            { LastError = "Original thumb and index pivots are unavailable."; ReleaseArm(); yield break; }
            float deadline = Time.realtimeSinceStartup + maximumContactWait;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (!stillValid() || !terminal || !terminal.gameObject.activeInHierarchy)
                { LastError = "Native wiring context changed during hand extension"; ReleaseArm(); yield break; }
                if (!arms || !arms.isActiveAndEnabled)
                { LastError = "Original arm controller disabled during extension"; ReleaseArm(); yield break; }
                if (arms.IsAtTarget())
                {
                    LastContactSucceeded = true;
                    LastError = "Original fingertip contact verified";
                    yield break;
                }
                yield return null;
            }
            LastError = "Original fingertips did not reach target within " +
                maximumContactWait.ToString("F2") + "s (gap=" +
                (arms ? arms.RightGripError.ToString("F4") : "unavailable") + "m)";
            ReleaseArm();
        }

        /// <summary>
        /// Sample the real grip AFTER the current character pose. Returns false
        /// when no telescopic contact is active; caller uses its normal grip then.
        /// </summary>
        public bool TryGetActiveGrip(out Vector3 worldGrip)
        {
            worldGrip = Vector3.zero;
            if (!IsContactActive) return false;
            if (!arms.TryGetGripPosition(SparkTelescopicArms.ManipulatorSide.Right, out worldGrip))
            { LastError = "Actual original hand grip transform is unavailable"; return false; }
            return true;
        }

        public bool VerifyGripNear(Vector3 expected, out float gap)
        {
            gap = float.PositiveInfinity;
            if (!TryGetActiveGrip(out Vector3 actual)) return false;
            gap = Vector3.Distance(actual, expected);
            return gap <= maximumNativeGripMismatch;
        }

        /// <summary>
        /// Move an already-gripping original hand to the destination while the
        /// cinematic executor continuously updates the native cable shape.
        /// </summary>
        public IEnumerator DrawHeldWireTo(Transform destination, Func<bool> stillValid) =>
            DrawHeldWireTo(destination, stillValid, Vector3.zero, 0f);

        internal IEnumerator DrawHeldWireTo(Transform destination, Func<bool> stillValid, Vector3 offset, float travelSpeed)
        {
            LastContactSucceeded = false;
            if (!IsContactActive || !destination || stillValid == null || !stillValid())
            { LastError = "No active original-hand wire grip or invalid destination"; yield break; }
            if (!arms.MoveCinematicGripToContact(destination, offset, travelSpeed))
            { LastError = arms.LastError; yield break; }
            float deadline = Time.realtimeSinceStartup + maximumContactWait;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (!stillValid() || !destination || !destination.gameObject.activeInHierarchy)
                { LastError = "Native wire context changed during drawing"; ReleaseArm(); yield break; }
                if (!arms || !arms.isActiveAndEnabled || !IsContactActive)
                { LastError = "Original hand stopped while drawing"; ReleaseArm(); yield break; }
                if (arms.IsAtTarget())
                {
                    LastContactSucceeded = true;
                    LastError = "Native wire destination reached by original fingertips";
                    yield break;
                }
                yield return null;
            }
            LastError = "Extended fingertips did not reach destination within " +
                maximumContactWait.ToString("F2") + "s";
            ReleaseArm();
        }

        public void ReleaseArm()
        {
            if (ownsRightArm && arms) arms.RetractRight();
            ownsRightArm = false;
            LastContactSucceeded = false;
        }

        private void OnDisable() { ReleaseArm(); }
        private void OnDestroy() { ReleaseArm(); }

        [ContextMenu("v18.5 / Show original hand bridge status")]
        private void DebugStatus()
        {
            Debug.Log("[Spark Telescopic Bridge v18.5] configured=" + IsConfigured +
                " enabled=" + enableCinematicExtension + " active=" + IsContactActive +
                " success=" + LastContactSucceeded + " detail=" + LastError, this);
        }
    }
}

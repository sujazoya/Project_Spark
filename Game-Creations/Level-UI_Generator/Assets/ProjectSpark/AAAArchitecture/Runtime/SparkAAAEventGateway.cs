using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectSpark.AAAArchitecture
{
    public sealed class SparkAAAEventGateway : SparkAAAMissionGateway
    {
        [Serializable] public sealed class LaunchEvent : UnityEvent<ScriptableObject> { }
        [SerializeField] private LaunchEvent launchRequested = new LaunchEvent();
        [SerializeField] private bool integrationApproved;
        private readonly Dictionary<string,SparkAAAProgress> snapshots = new Dictionary<string,SparkAAAProgress>(StringComparer.Ordinal);
        private bool pending;
        // Call only from an adapter reading the authoritative legacy progression state, on the main thread.
        public void Publish(string campaignKey, SparkAAAProgress progress)
        {
            if (string.IsNullOrEmpty(campaignKey) || progress == null) return;
            var copy = progress.Copy();
            copy.fraction = float.IsNaN(copy.fraction) || float.IsInfinity(copy.fraction) ? 0 : Mathf.Clamp01(copy.fraction);
            snapshots[campaignKey] = copy;
            NotifyChanged();
        }
        public void ClearSnapshots() { snapshots.Clear(); NotifyChanged(); }
        public override SparkAAAProgress Read(SparkAAALevel level)
        {
            if (level != null && snapshots.TryGetValue(level.key, out var state)) return state.Copy();
            return new SparkAAAProgress { status = SparkAAAStatus.Unknown, reason = "Waiting for verified progression data." };
        }
        public override bool TryLaunch(SparkAAALevel level, out string reason)
        {
            reason = null;
            if (!isActiveAndEnabled || !integrationApproved) { reason = "Legacy launch adapter has not been approved."; return false; }
            if (pending) { reason = "A launch request is already pending."; return false; }
            if (level == null || !level.Present || level.blocked) { reason = "Level source unavailable or ambiguous."; return false; }
            var state = Read(level);
            if (!state.canLaunch || state.status == SparkAAAStatus.Unknown || state.status == SparkAAAStatus.Locked) { reason = state.reason ?? "Access not approved."; return false; }
            // Persistent handler must be explicitly connected; no guessed selector API or scene name.
            bool wired = false;
            for (int i = 0; i < launchRequested.GetPersistentEventCount(); i++)
                if (launchRequested.GetPersistentTarget(i) != null && !string.IsNullOrEmpty(launchRequested.GetPersistentMethodName(i)) && launchRequested.GetPersistentListenerState(i) != UnityEventCallState.Off) wired = true;
            if (!wired) { reason = "Connect the verified launch receiver in the Inspector."; return false; }
            pending = true;
            try { launchRequested.Invoke(level.definition); }
            catch (Exception ex) { pending = false; reason = ex.Message; NotifyChanged(); return false; }
            NotifyChanged(); return true;
        }
        // Existing lifecycle adapter must call on rejection, return to menu or load failure.
        public void ReleaseLaunchGate() { pending = false; NotifyChanged(); }
        public bool LaunchPending => pending;
    }
}

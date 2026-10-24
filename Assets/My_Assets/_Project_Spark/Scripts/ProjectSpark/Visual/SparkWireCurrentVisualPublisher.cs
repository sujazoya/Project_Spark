
using System.Collections.Generic;
using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Publishes solved branch currents to dynamically
    /// created and bound SignalWireCurrentFeedback objects.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkWireCurrentVisualPublisher : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SparkWireCurrentSolveBridge bridge;

        [SerializeField]
        private SignalPath_Manager wireManager;

        [Header("Diagnostics")]
        [SerializeField]
        private bool logCurrentEvents = true;

        private bool subscribed;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
            RefreshCurrentVisuals();
        }

        private void OnDisable()
        {
            Unsubscribe();
            Clear();
        }

        private void ResolveReferences()
        {
            if (bridge == null)
            {
                bridge =
                    FindFirstObjectByType<
                        SparkWireCurrentSolveBridge>();
            }

            if (wireManager == null)
            {
                wireManager =
                    FindFirstObjectByType<SignalPath_Manager>();
            }
        }

        private void Subscribe()
        {
            if (subscribed || bridge == null)
                return;

            bridge.CurrentsResolved += Publish;
            bridge.CurrentsInvalidated += Clear;

            subscribed = true;

            if (logCurrentEvents)
            {
                Debug.Log(
                    "[WIRE PUBLISHER] Subscribed to bridge.",
                    this);
            }
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;

            if (bridge != null)
            {
                bridge.CurrentsResolved -= Publish;
                bridge.CurrentsInvalidated -= Clear;
            }

            subscribed = false;
        }

        private SignalWireCurrentFeedback[] FindWires()
        {
            if (wireManager == null)
                ResolveReferences();

            if (wireManager == null)
            {
                Debug.LogWarning(
                    "[WIRE PUBLISHER] Runtime wire manager missing.",
                    this);

                return System.Array.Empty<
                    SignalWireCurrentFeedback>();
            }

            return wireManager.GetComponentsInChildren<
                SignalWireCurrentFeedback>(true);
        }

        private void Clear()
        {
            SignalWireCurrentFeedback[] wires = FindWires();

            if (logCurrentEvents)
            {
                Debug.Log(
                    $"[WIRE PUBLISHER CLEAR] " +
                    $"WireCount={wires.Length}",
                    this);
            }

            foreach (SignalWireCurrentFeedback wire in wires)
            {
                if (wire != null)
                    wire.ClearReading();
            }
        }

        private void Publish(
            IReadOnlyList<
                SparkWireBranchCurrentResolver.WireCurrent> readings)
        {
            if (readings == null)
            {
                Clear();
                return;
            }

            Dictionary<ulong, float> byId =
                new Dictionary<ulong, float>();

            for (int i = 0; i < readings.Count; i++)
            {
                byId[readings[i].ConnectionId] =
                    readings[i].AmpsAtoB;
            }

            SignalWireCurrentFeedback[] wires = FindWires();

            if (logCurrentEvents)
            {
                Debug.Log(
                    $"[WIRE PUBLISHER RESOLVED] " +
                    $"Readings={readings.Count} | " +
                    $"RuntimeWires={wires.Length}",
                    this);
            }

            foreach (SignalWireCurrentFeedback wire in wires)
            {
                if (wire == null || !wire.IsBound)
                    continue;

                if (byId.TryGetValue(
                    wire.ConnectionId,
                    out float amps))
                {
                    bool accepted = wire.PublishCurrent(
                        wire.ConnectionId,
                        amps);

                    if (logCurrentEvents)
                    {
                        Debug.Log(
                            $"[WIRE PUBLISHER MATCH] " +
                            $"Wire={wire.name} | " +
                            $"ConnectionId={wire.ConnectionId} | " +
                            $"Amps={amps:F6} | " +
                            $"Accepted={accepted}",
                            wire);
                    }
                }
                else
                {
                    if (logCurrentEvents)
                    {
                        Debug.LogWarning(
                            $"[WIRE PUBLISHER NO MATCH] " +
                            $"Wire={wire.name} | " +
                            $"ConnectionId={wire.ConnectionId}",
                            wire);
                    }

                    wire.ClearReading();
                }
            }
        }

        /// <summary>
        /// Replays the bridge's latest verified readings
        /// after a runtime wire receives its connection ID.
        /// </summary>
        public void RefreshCurrentVisuals()
        {
            ResolveReferences();
            Subscribe();

            if (bridge == null)
            {
                Debug.LogWarning(
                    "[WIRE PUBLISHER] Current solve bridge missing.",
                    this);
                return;
            }

            if (bridge.CurrentReadings != null &&
                bridge.CurrentReadings.Count > 0)
            {
                Publish(bridge.CurrentReadings);
            }
            else
            {
                if (logCurrentEvents)
                {
                    Debug.Log(
                        "[WIRE PUBLISHER] No cached readings.",
                        this);
                }

                Clear();
            }
        }
    }
}

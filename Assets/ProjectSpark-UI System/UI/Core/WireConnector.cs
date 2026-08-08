using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [DisallowMultipleComponent]
    public sealed class WireConnector : MonoBehaviour
    {
        [Header("Connector")]
        [SerializeField] private Transform point;

        [Header("Connection Settings")]
        [SerializeField] private int maxConnections = 1;

        private readonly List<WireController> connectedWires =
            new List<WireController>();

        public Transform Point =>
            point != null ? point : transform;

        public int ConnectionCount =>
            connectedWires.Count;

        public int MaxConnections =>
            maxConnections;

        public bool IsConnected =>
            connectedWires.Count > 0;

        public bool CanConnect =>
            connectedWires.Count < maxConnections;

        public IReadOnlyList<WireController> ConnectedWires =>
            connectedWires;

        private void Awake()
        {
            if (point == null)
                point = transform;

            maxConnections = Mathf.Max(1, maxConnections);
        }

        public bool AddWire(WireController wire)
        {
            if (wire == null)
                return false;

            if (connectedWires.Contains(wire))
                return true;

            if (!CanConnect)
                return false;

            connectedWires.Add(wire);
            return true;
        }

        public void RemoveWire(WireController wire)
        {
            if (wire == null)
                return;

            connectedWires.Remove(wire);
        }

        public bool HasWire(WireController wire)
        {
            return wire != null &&
                   connectedWires.Contains(wire);
        }

        public void DisconnectAll()
        {
            WireController[] wires =
                connectedWires.ToArray();

            foreach (WireController wire in wires)
            {
                if (wire != null)
                    wire.Disconnect();
            }

            connectedWires.Clear();
        }

        private void OnDestroy()
        {
            connectedWires.Clear();
        }
    }
}
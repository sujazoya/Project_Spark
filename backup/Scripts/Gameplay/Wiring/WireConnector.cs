// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireConnector.cs

using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public enum ConnectorType
    {
        Generic,

        BatteryPositive,
        BatteryNegative,

        BulbPositive,
        BulbNegative,

        Switch,

        Ground
    }

    [DisallowMultipleComponent]
    public sealed class WireConnector : MonoBehaviour
    {
        [SerializeField]
        private ConnectorType connectorType;

        [SerializeField]
        private bool allowMultipleConnections;

        [SerializeField]
        private Transform connectionPoint;

        private readonly List<WireController> wires = new();

        public ConnectorType Type => connectorType;

        public Transform Point =>
            connectionPoint == null
                ? transform
                : connectionPoint;

        public bool CanConnect =>
            allowMultipleConnections || wires.Count == 0;

        public void AddWire(WireController wire)
        {
            if (!wires.Contains(wire))
                wires.Add(wire);
        }

        public void RemoveWire(WireController wire)
        {
            wires.Remove(wire);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(Point.position, 0.008f);
        }
    }
}
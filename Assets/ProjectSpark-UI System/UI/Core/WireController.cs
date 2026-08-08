using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [DisallowMultipleComponent]
    public sealed class WireController : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform startConnector;
        [SerializeField] private Transform endConnector;

        [Header("Visual")]
        [SerializeField] private LineRenderer lineRenderer;

        [Header("Connection")]
        [SerializeField] private WireConnector startTerminal;

        private WireConnector endTerminal;

        private WireState state = WireState.Idle;

        public WireState State => state;

        public Transform EndTransform => endConnector;

        public WireConnector StartConnector => startTerminal;

        public WireConnector EndConnector => endTerminal;

        public bool IsConnected =>
            startTerminal != null &&
            endTerminal != null;

        private void Awake()
        {
            if (lineRenderer != null)
                lineRenderer.positionCount = 2;
        }

        private void LateUpdate()
        {
            if (lineRenderer == null)
                return;

            lineRenderer.SetPosition(0, startConnector.position);
            lineRenderer.SetPosition(1, endConnector.position);
        }

        public void Initialize(WireConnector start)
        {
            if (start == null)
                return;

            if (startTerminal != null)
                startTerminal.RemoveWire(this);

            if (!start.AddWire(this))
                return;

            startTerminal = start;

            startConnector.position = start.Point.position;

            state = WireState.Idle;
        }

        public void Connect(SnapPoint snapPoint)
        {
            if (snapPoint == null)
                return;

            WireConnector newTerminal = snapPoint.Connector;

            if (newTerminal == null)
                return;

            if (endTerminal == newTerminal)
            {
                endConnector.position = snapPoint.Position;
                state = WireState.Snapped;
                return;
            }

            if (!newTerminal.CanConnect)
                return;

            if (endTerminal != null)
                endTerminal.RemoveWire(this);

            if (!newTerminal.AddWire(this))
                return;

            endTerminal = newTerminal;
            endConnector.position = snapPoint.Position;

            state = WireState.Snapped;
        }

        public void Disconnect()
        {
            if (endTerminal != null)
            {
                endTerminal.RemoveWire(this);
                endTerminal = null;
            }

            state = WireState.Idle;
        }

        public void SetDragging(bool dragging)
        {
            state = dragging
                ? WireState.Dragging
                : WireState.Idle;
        }

        public void SetPowered(bool powered)
        {
            state = powered
                ? WireState.Powered
                : WireState.Snapped;
        }
    }
}
using UnityEngine;

namespace AAAUI
{
    [DisallowMultipleComponent]
    public sealed class CircuitConnectionVFX : MonoBehaviour
    {
        [Header("Connection")]
        [SerializeField]
        private Transform startPoint;

        [SerializeField]
        private Transform endPoint;

        [SerializeField]
        private LineRenderer line;

        [Header("Visual")]
        [SerializeField, Min(0.001f)]
        private float width = 0.025f;

        [SerializeField]
        private bool updateEveryFrame = true;

        [Header("Signal")]
        [SerializeField]
        private SignalFlowVFX signalFlow;

        private void Awake()
        {
            if (line == null)
                line = GetComponent<LineRenderer>();

            if (signalFlow == null)
                signalFlow =
                    GetComponent<SignalFlowVFX>();

            ApplyLineSettings();
            UpdateConnection();
        }

        private void Update()
        {
            if (updateEveryFrame)
                UpdateConnection();
        }

        public void SetPoints(
            Transform start,
            Transform end)
        {
            startPoint = start;
            endPoint = end;

            UpdateConnection();
        }

        public void SetSignalFlow(
            SignalFlowVFX flow)
        {
            signalFlow = flow;
        }

        public void SetVisible(
            bool visible)
        {
            if (line != null)
                line.enabled = visible;
        }

        private void UpdateConnection()
        {
            if (line == null ||
                startPoint == null ||
                endPoint == null)
                return;

            Vector3 start =
                startPoint.position;

            Vector3 end =
                endPoint.position;

            line.positionCount = 2;

            line.SetPosition(
                0,
                start
            );

            line.SetPosition(
                1,
                end
            );
        }

        private void ApplyLineSettings()
        {
            if (line == null)
                return;

            line.startWidth = width;
            line.endWidth = width;
            line.useWorldSpace = true;
        }
    }
}
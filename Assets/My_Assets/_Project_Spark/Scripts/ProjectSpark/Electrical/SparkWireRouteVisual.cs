

using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class SparkWireRouteVisual : MonoBehaviour
    {
        public enum WireRoute
        {
            Up,
            Straight,
            Low
        }

        [Header("References")]
        [SerializeField] private SparkSingleWire wire;
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private LineRenderer line;

        [Header("Wire Route")]
        [SerializeField] private WireRoute route =
            WireRoute.Up;

        [SerializeField, Min(0f)]
        private float upHeight = 0.35f;

        [SerializeField, Min(0f)]
        private float lowDepth = 0.15f;

        [SerializeField, Range(0.05f, 0.45f)]
        private float bendPosition = 0.25f;

        [Header("Appearance")]
        [SerializeField, Min(0.001f)]
        private float wireWidth = 0.025f;

        [SerializeField] private Material wireMaterial;

        [SerializeField] private bool updateEveryFrame = true;

        public WireRoute Route => route;

        private void Reset()
        {
            Resolve();
            ConfigureLine();
            RefreshRoute();
        }

        private void Awake()
        {
            Resolve();
            ConfigureLine();
            RefreshRoute();
        }

        private void OnEnable()
        {
            Resolve();
            RefreshRoute();
        }

        private void LateUpdate()
        {
            if (Application.isPlaying && !updateEveryFrame)
                return;

            RefreshRoute();
        }

        private void OnValidate()
        {
            upHeight = Mathf.Max(0f, upHeight);
            lowDepth = Mathf.Max(0f, lowDepth);
            wireWidth = Mathf.Max(0.001f, wireWidth);

            Resolve();
            ConfigureLine();
            RefreshRoute();
        }

        private void Resolve()
        {
            if (wire == null)
                wire = GetComponent<SparkSingleWire>();

            if (line == null)
                line = GetComponent<LineRenderer>();

            if (wire == null)
                return;

            if (startPoint == null && wire.ContactA != null)
                startPoint = wire.ContactA.transform;

            if (endPoint == null && wire.ContactB != null)
                endPoint = wire.ContactB.transform;
        }

        private void ConfigureLine()
        {
            if (line == null)
                return;

            line.useWorldSpace = true;
            line.startWidth = wireWidth;
            line.endWidth = wireWidth;
            line.numCapVertices = 6;
            line.numCornerVertices = 4;

            if (wireMaterial != null)
                line.sharedMaterial = wireMaterial;
        }

        public void RefreshRoute()
        {
            if (line == null)
                return;

            if (startPoint == null || endPoint == null)
            {
                line.positionCount = 0;
                return;
            }

            Vector3 start = startPoint.position;
            Vector3 end = endPoint.position;

            if (route == WireRoute.Straight)
            {
                line.positionCount = 2;
                line.SetPosition(0, start);
                line.SetPosition(1, end);
                return;
            }

            float height = route == WireRoute.Up
                ? upHeight
                : -lowDepth;

            Vector3 middleA = Vector3.Lerp(
                start, end, bendPosition);

            Vector3 middleB = Vector3.Lerp(
                start, end, 1f - bendPosition);

            middleA += Vector3.up * height;
            middleB += Vector3.up * height;

            line.positionCount = 4;
            line.SetPosition(0, start);
            line.SetPosition(1, middleA);
            line.SetPosition(2, middleB);
            line.SetPosition(3, end);
        }

        public void SetRoute(WireRoute newRoute)
        {
            route = newRoute;
            RefreshRoute();
        }

        public void SetUp()
        {
            SetRoute(WireRoute.Up);
        }

        public void SetStraight()
        {
            SetRoute(WireRoute.Straight);
        }

        public void SetLow()
        {
            SetRoute(WireRoute.Low);
        }
    }
}

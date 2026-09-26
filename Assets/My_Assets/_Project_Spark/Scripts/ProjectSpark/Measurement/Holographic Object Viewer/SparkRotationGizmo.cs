using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    /// <summary>
    /// Visual and hit-test representation of the Project Spark
    /// rotation manipulator.
    ///
    /// This component does NOT rotate the target Transform.
    ///
    /// Responsibilities:
    /// - Create X/Y/Z/View rings.
    /// - Follow the selected target.
    /// - Maintain gizmo pivot.
    /// - Perform screen-space ring hit testing.
    /// - Resolve a selected ring into a world-space axis.
    /// - Manage hover and active visual states.
    ///
    /// Actual object rotation is owned exclusively by
    /// HolographicComponentManipulator.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkRotationGizmo : MonoBehaviour
    {
        public enum GizmoAxis
        {
            None,
            X,
            Y,
            Z,
            View
        }

        // ============================================================
        // CAMERA
        // ============================================================

        [Header("Camera")]
        [SerializeField]
        private Camera viewerCamera;

        // ============================================================
        // SCREEN SIZE
        // ============================================================

        [Header("Screen Size")]
        [SerializeField, Min(20f)]
        private float screenRadius = 90f;

        [SerializeField, Min(1f)]
        private float hitTolerancePixels = 14f;

        [SerializeField, Min(20f)]
        private float minimumPointerRadius = 20f;

        // ============================================================
        // GEOMETRY
        // ============================================================

        [Header("Geometry")]
        [SerializeField, Range(24, 128)]
        private int ringSegments = 64;

        [SerializeField, Min(0.001f)]
        private float lineWidth = 0.012f;

        [SerializeField, Min(0.001f)]
        private float hoverLineWidth = 0.018f;

        [SerializeField, Min(0.001f)]
        private float activeLineWidth = 0.024f;

        // ============================================================
        // MATERIALS
        // ============================================================

        [Header("Materials")]
        [SerializeField]
        private Material xMaterial;

        [SerializeField]
        private Material yMaterial;

        [SerializeField]
        private Material zMaterial;

        [SerializeField]
        private Material viewMaterial;

        [SerializeField]
        private Material hoverMaterial;

        [SerializeField]
        private Material activeMaterial;

        // ============================================================
        // VISIBILITY
        // ============================================================

        [Header("Visibility")]
        [SerializeField]
        private bool hideViewRing;

        // ============================================================
        // RUNTIME
        // ============================================================

        private Transform target;

        private Vector3 pivot;

        private GizmoAxis hoveredAxis =
            GizmoAxis.None;

        private GizmoAxis activeAxis =
            GizmoAxis.None;

        private LineRenderer xRing;
        private LineRenderer yRing;
        private LineRenderer zRing;
        private LineRenderer viewRing;

        private bool initialized;

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        public Transform Target
        {
            get { return target; }
        }

        public Vector3 Pivot
        {
            get { return pivot; }
        }

        public GizmoAxis HoveredAxis
        {
            get { return hoveredAxis; }
        }

        public GizmoAxis ActiveAxis
        {
            get { return activeAxis; }
        }

        public bool IsVisible
        {
            get
            {
                return initialized &&
                       xRing != null &&
                       xRing.enabled;
            }
        }

        // ============================================================
        // INITIALIZATION
        // ============================================================

        private void Awake()
        {
            ResolveCamera();
            BuildGizmo();
            Hide();
        }

        private void LateUpdate()
        {
            if (!IsVisible ||
                target == null)
            {
                return;
            }

            RefreshTransform();
        }

        private void ResolveCamera()
        {
            if (viewerCamera == null)
            {
                viewerCamera =
                    Camera.main;
            }
        }

        // ============================================================
        // TARGET
        // ============================================================

      public void SetTarget(
    Transform newTarget,
    Vector3 worldPivot)
{
    bool targetChanged =
        target != newTarget;

    target =
        newTarget;

    pivot =
        worldPivot;

    if (targetChanged)
    {
        hoveredAxis =
            GizmoAxis.None;

        /*
         * Only clear the active axis when the actual selected
         * object changes.
         */
        activeAxis =
            GizmoAxis.None;
    }

    if (target == null)
    {
        Hide();
        return;
    }

    /*
     * Do not automatically change visibility here.
     *
     * SparkRotateTool owns visibility.
     */
    if (IsVisible)
    {
        RefreshTransform();
        RefreshVisuals();
    }
}

        public void SetPivot(
            Vector3 worldPivot)
        {
            pivot =
                worldPivot;

            if (target != null)
            {
                RefreshTransform();
            }
        }

        public void ClearTarget()
        {
            target =
                null;

            pivot =
                Vector3.zero;

            hoveredAxis =
                GizmoAxis.None;

            activeAxis =
                GizmoAxis.None;

            Hide();
        }

        // ============================================================
        // VISIBILITY
        // ============================================================

        public void Show()
        {
            if (!initialized)
            {
                return;
            }

            if (xRing != null)
            {
                xRing.enabled = true;
            }

            if (yRing != null)
            {
                yRing.enabled = true;
            }

            if (zRing != null)
            {
                zRing.enabled = true;
            }

            if (viewRing != null)
            {
                viewRing.enabled =
                    !hideViewRing;
            }

            RefreshTransform();
            RefreshVisuals();
        }

        public void Hide()
        {
            hoveredAxis =
                GizmoAxis.None;

            activeAxis =
                GizmoAxis.None;

            if (xRing != null)
            {
                xRing.enabled = false;
            }

            if (yRing != null)
            {
                yRing.enabled = false;
            }

            if (zRing != null)
            {
                zRing.enabled = false;
            }

            if (viewRing != null)
            {
                viewRing.enabled = false;
            }
        }

        // ============================================================
        // HOVER
        // ============================================================

        /// <summary>
        /// Updates the hover state from the supplied screen position.
        ///
        /// Hover does not start manipulation.
        /// </summary>
        public GizmoAxis UpdateHover(
            Vector2 screenPosition)
        {
            if (!IsVisible ||
                target == null)
            {
                SetHover(
                    GizmoAxis.None);

                return GizmoAxis.None;
            }

            if (activeAxis != GizmoAxis.None)
            {
                return activeAxis;
            }

            TryGetAxis(
                screenPosition,
                out GizmoAxis detectedAxis);

            SetHover(
                detectedAxis);

            return detectedAxis;
        }

        private void SetHover(
            GizmoAxis axis)
        {
            if (hoveredAxis == axis)
            {
                return;
            }

            hoveredAxis =
                axis;

            RefreshVisuals();
        }

        // ============================================================
        // HIT TEST
        // ============================================================

        public bool TryGetAxis(
            Vector2 screenPosition,
            out GizmoAxis axis)
        {
            axis =
                GizmoAxis.None;

            if (!IsVisible ||
                target == null)
            {
                return false;
            }

            ResolveCamera();

            if (viewerCamera == null)
            {
                return false;
            }

            Vector3 pivotScreen3D =
                viewerCamera.WorldToScreenPoint(
                    pivot);

            if (pivotScreen3D.z <= 0f)
            {
                return false;
            }

            Vector2 pivotScreen =
                new Vector2(
                    pivotScreen3D.x,
                    pivotScreen3D.y);

            float pointerRadius =
                Vector2.Distance(
                    screenPosition,
                    pivotScreen);

            if (pointerRadius <
                minimumPointerRadius)
            {
                return false;
            }

            float bestError =
                float.MaxValue;

            GizmoAxis bestAxis =
                GizmoAxis.None;

            TestAxis(
                GizmoAxis.X,
                GetWorldAxis(
                    GizmoAxis.X),
                screenPosition,
                pivotScreen,
                ref bestAxis,
                ref bestError);

            TestAxis(
                GizmoAxis.Y,
                GetWorldAxis(
                    GizmoAxis.Y),
                screenPosition,
                pivotScreen,
                ref bestAxis,
                ref bestError);

            TestAxis(
                GizmoAxis.Z,
                GetWorldAxis(
                    GizmoAxis.Z),
                screenPosition,
                pivotScreen,
                ref bestAxis,
                ref bestError);

            if (!hideViewRing)
            {
                TestAxis(
                    GizmoAxis.View,
                    viewerCamera.transform.forward,
                    screenPosition,
                    pivotScreen,
                    ref bestAxis,
                    ref bestError);
            }

            if (bestAxis ==
                GizmoAxis.None)
            {
                return false;
            }

            axis =
                bestAxis;

            return true;
        }

        private void TestAxis(
            GizmoAxis axis,
            Vector3 worldNormal,
            Vector2 screenPosition,
            Vector2 pivotScreen,
            ref GizmoAxis bestAxis,
            ref float bestError)
        {
            if (worldNormal.sqrMagnitude <
                0.000001f)
            {
                return;
            }

            worldNormal.Normalize();

            Ray ray =
                viewerCamera.ScreenPointToRay(
                    screenPosition);

            Plane plane =
                new Plane(
                    worldNormal,
                    pivot);

            if (!plane.Raycast(
                    ray,
                    out float enter))
            {
                return;
            }

            if (enter < 0f)
            {
                return;
            }

            Vector3 hitPoint =
                ray.GetPoint(enter);

            Vector3 radial =
                hitPoint -
                pivot;

            if (radial.sqrMagnitude <
                0.000001f)
            {
                return;
            }

            radial.Normalize();

            Vector3 ringPoint =
                pivot +
                radial *
                GetWorldRadius();

            Vector3 ringScreen3D =
                viewerCamera.WorldToScreenPoint(
                    ringPoint);

            if (ringScreen3D.z <= 0f)
            {
                return;
            }

            Vector2 ringScreen =
                new Vector2(
                    ringScreen3D.x,
                    ringScreen3D.y);

            float actualRadius =
                Vector2.Distance(
                    pivotScreen,
                    ringScreen);

            float pointerRadius =
                Vector2.Distance(
                    pivotScreen,
                    screenPosition);

            float error =
                Mathf.Abs(
                    pointerRadius -
                    actualRadius);

            if (error >
                hitTolerancePixels)
            {
                return;
            }

            if (axis != GizmoAxis.View)
            {
                float cameraFacing =
                    Mathf.Abs(
                        Vector3.Dot(
                            worldNormal,
                            viewerCamera.transform.forward));

                if (cameraFacing >
                    0.985f)
                {
                    return;
                }
            }

            if (error <
                bestError)
            {
                bestError =
                    error;

                bestAxis =
                    axis;
            }
        }

        // ============================================================
        // PRESS / RELEASE
        // ============================================================

        public bool TryPress(
            Vector2 screenPosition)
        {
            if (!TryGetAxis(
                    screenPosition,
                    out GizmoAxis axis))
            {
                return false;
            }

            activeAxis =
                axis;

            hoveredAxis =
                axis;

            RefreshVisuals();

            return true;
        }

        public void Release()
        {
            activeAxis =
                GizmoAxis.None;

            RefreshVisuals();
        }

        public void Cancel()
        {
            activeAxis =
                GizmoAxis.None;

            hoveredAxis =
                GizmoAxis.None;

            RefreshVisuals();
        }

        // ============================================================
        // WORLD AXIS
        // ============================================================

        /// <summary>
        /// Resolves a gizmo axis into its current world-space axis.
        ///
        /// X/Y/Z use the selected object's local axes.
        /// View uses the viewer camera forward direction.
        /// </summary>
        public bool TryGetWorldAxis(
            GizmoAxis axis,
            out Vector3 worldAxis)
        {
            worldAxis =
                Vector3.zero;

            ResolveCamera();

            if (target == null)
            {
                return false;
            }

            switch (axis)
            {
                case GizmoAxis.X:

                    worldAxis =
                        target.right;

                    break;

                case GizmoAxis.Y:

                    worldAxis =
                        target.up;

                    break;

                case GizmoAxis.Z:

                    worldAxis =
                        target.forward;

                    break;

                case GizmoAxis.View:

                    if (viewerCamera == null)
                    {
                        return false;
                    }

                    worldAxis =
                        viewerCamera.transform.forward;

                    break;

                default:

                    return false;
            }

            if (worldAxis.sqrMagnitude <
                0.000001f)
            {
                worldAxis =
                    Vector3.zero;

                return false;
            }

            worldAxis.Normalize();

            return true;
        }

        private Vector3 GetWorldAxis(
            GizmoAxis axis)
        {
            if (TryGetWorldAxis(
                    axis,
                    out Vector3 worldAxis))
            {
                return worldAxis;
            }

            return Vector3.zero;
        }

        // ============================================================
        // TRANSFORM
        // ============================================================

        private void RefreshTransform()
        {
            ResolveCamera();

            if (target == null ||
                viewerCamera == null)
            {
                return;
            }

            transform.position =
                pivot;

            // X/Y/Z rings are in the selected object's
            // local rotation space.
            transform.rotation =
                target.rotation;

            float radius =
                GetWorldRadius();

            transform.localScale =
                Vector3.one *
                radius;

            UpdateViewRingOrientation();
        }

        private void UpdateViewRingOrientation()
        {
            if (viewRing == null ||
                viewerCamera == null)
            {
                return;
            }

            Vector3 normal =
                viewerCamera.transform.forward;

            if (normal.sqrMagnitude <
                0.000001f)
            {
                return;
            }

            Quaternion worldRotation =
                Quaternion.LookRotation(
                    normal,
                    viewerCamera.transform.up);

            viewRing.transform.rotation =
                worldRotation;
        }

        // ============================================================
        // WORLD RADIUS
        // ============================================================

        private float GetWorldRadius()
        {
            if (viewerCamera == null)
            {
                return 1f;
            }

            float safeScreenHeight =
                Mathf.Max(
                    Screen.height,
                    1);

            if (viewerCamera.orthographic)
            {
                float worldHeight =
                    viewerCamera.orthographicSize *
                    2f;

                float pixelsPerWorldUnit =
                    safeScreenHeight /
                    Mathf.Max(
                        worldHeight,
                        0.001f);

                return screenRadius /
                       Mathf.Max(
                           pixelsPerWorldUnit,
                           0.001f);
            }

            float cameraDistance =
                Vector3.Distance(
                    viewerCamera.transform.position,
                    pivot);

            cameraDistance =
                Mathf.Max(
                    cameraDistance,
                    0.001f);

            float fieldOfViewRadians =
                viewerCamera.fieldOfView *
                Mathf.Deg2Rad;

            float perspectiveWorldHeight =
                2f *
                cameraDistance *
                Mathf.Tan(
                    fieldOfViewRadians *
                    0.5f);

            float pixelsPerWorldUnitPerspective =
                safeScreenHeight /
                Mathf.Max(
                    perspectiveWorldHeight,
                    0.001f);

            return screenRadius /
                   Mathf.Max(
                       pixelsPerWorldUnitPerspective,
                       0.001f);
        }

        // ============================================================
        // BUILD
        // ============================================================

        private void BuildGizmo()
        {
            if (initialized)
            {
                return;
            }

            ringSegments =
                Mathf.Clamp(
                    ringSegments,
                    24,
                    128);

            xRing =
                CreateRing("X");

            yRing =
                CreateRing("Y");

            zRing =
                CreateRing("Z");

            viewRing =
                CreateRing("View");

            BuildRing(
                xRing,
                Vector3.right);

            BuildRing(
                yRing,
                Vector3.up);

            BuildRing(
                zRing,
                Vector3.forward);

            BuildRing(
                viewRing,
                Vector3.forward);

            initialized =
                true;

            RefreshVisuals();
        }

        private LineRenderer CreateRing(
            string ringName)
        {
            GameObject ringObject =
                new GameObject(
                    ringName);

            ringObject.transform.SetParent(
                transform,
                false);

            LineRenderer renderer =
                ringObject.AddComponent<
                    LineRenderer>();

            renderer.useWorldSpace =
                false;

            renderer.loop =
                true;

            renderer.positionCount =
                ringSegments;

            renderer.numCapVertices =
                2;

            renderer.numCornerVertices =
                2;

            renderer.textureMode =
                LineTextureMode.Stretch;

            renderer.alignment =
                LineAlignment.View;

            return renderer;
        }

        private void BuildRing(
            LineRenderer renderer,
            Vector3 normal)
        {
            if (renderer == null)
            {
                return;
            }

            Vector3 tangent =
                Vector3.Cross(
                    normal,
                    Vector3.up);

            if (tangent.sqrMagnitude <
                0.0001f)
            {
                tangent =
                    Vector3.Cross(
                        normal,
                        Vector3.right);
            }

            tangent.Normalize();

            Vector3 bitangent =
                Vector3.Cross(
                    normal,
                    tangent).normalized;

            for (int i = 0;
                 i < ringSegments;
                 i++)
            {
                float normalized =
                    i /
                    (float)ringSegments;

                float angle =
                    normalized *
                    Mathf.PI *
                    2f;

                Vector3 point =
                    tangent *
                    Mathf.Cos(angle) +
                    bitangent *
                    Mathf.Sin(angle);

                renderer.SetPosition(
                    i,
                    point);
            }
        }

        // ============================================================
        // VISUALS
        // ============================================================

        private void RefreshVisuals()
        {
            ApplyRingVisual(
                xRing,
                GizmoAxis.X,
                xMaterial);

            ApplyRingVisual(
                yRing,
                GizmoAxis.Y,
                yMaterial);

            ApplyRingVisual(
                zRing,
                GizmoAxis.Z,
                zMaterial);

            ApplyRingVisual(
                viewRing,
                GizmoAxis.View,
                viewMaterial);
        }

        private void ApplyRingVisual(
            LineRenderer renderer,
            GizmoAxis axis,
            Material normalMaterial)
        {
            if (renderer == null)
            {
                return;
            }

            if (activeAxis == axis)
            {
                renderer.material =
                    activeMaterial;

                renderer.widthMultiplier =
                    activeLineWidth;

                return;
            }

            if (hoveredAxis == axis)
            {
                renderer.material =
                    hoverMaterial;

                renderer.widthMultiplier =
                    hoverLineWidth;

                return;
            }

            renderer.material =
                normalMaterial;

            renderer.widthMultiplier =
                lineWidth;
        }
    }
}
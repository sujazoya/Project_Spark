using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Rotate Tool for Project Spark.
    ///
    /// Responsibilities:
    /// - Shows the rotation gizmo while Rotate Tool is active.
    /// - Hides the gizmo when Rotate Tool is disabled.
    /// - Keeps the gizmo attached to the currently selected object.
    ///
    /// Actual rotation remains owned by:
    /// HolographicComponentManipulator.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkRotateTool : SparkTool
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("References")]
        [SerializeField]
        private HolographicViewer.HolographicObjectController objectController;

        [SerializeField]
        private HolographicViewer.SparkRotationGizmo rotationGizmo;

        [SerializeField]
        private SparkSelectionController selectionController;

        [SerializeField]
        private HolographicViewer.HolographicViewerMouseInput viewerMouseInput;
        private SparkToolController toolController;

        protected override SparkToolType GetToolType() => SparkToolType.Rotate;

        protected override SparkResult OnBegin(SparkToolContext context)
        {
            ResolveReferences();
            if (CurrentTarget == null) return SparkResult.Rejected("Select an object before rotating it.");
            ShowGizmo();
            return SparkResult.Success("Drag a rotation ring to rotate the selected object.");
        }

        protected override SparkResult OnEnd(SparkToolContext context) => SparkResult.Success();
        protected override SparkResult OnCancel(SparkToolContext context)
        {
            if (viewerMouseInput != null) viewerMouseInput.CancelComponentRotation();
            return SparkResult.Cancelled();
        }

        // ============================================================
        // CURRENT TARGET
        // ============================================================

        private Transform CurrentTarget
        {
            get
            {
                if (selectionController == null)
                {
                    return null;
                }

                SparkSelectable selected =
                    selectionController.SelectedObject;

                if (selected == null)
                {
                    return null;
                }

                return selected.transform;
            }
        }

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        public bool IsRotateToolActive =>
            isActiveAndEnabled && toolController != null && toolController.ActiveTool == this;

        // ============================================================
        // INITIALIZATION
        // ============================================================

        protected override void Awake()
        {
            base.Awake();
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (toolController != null) toolController.ActiveToolChanged += OnToolChanged;
            if (IsRotateToolActive) ShowGizmo(); else HideGizmo();
        }

        private void Update()
        {
            if (!IsRotateToolActive)
            {
                HideGizmo();
                return;
            }

            UpdateGizmoTarget();
        }

        private void OnDisable()
        {
            if (toolController != null) toolController.ActiveToolChanged -= OnToolChanged;
            /*
             * Stop an active gizmo drag first.
             *
             * This prevents the mouse input system from retaining
             * a component-rotation session after the Rotate Tool
             * has been disabled.
             */
            if (viewerMouseInput != null)
            {
                viewerMouseInput.CancelComponentRotation();
            }

            HideGizmo();
        }

        // ============================================================
        // TOOL LIFECYCLE
        // ============================================================

        public void OnBegin()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            ResolveReferences();

            ShowGizmo();
        }

        public void OnUpdate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            UpdateGizmoTarget();
        }

        public void OnEnd()
        {
            if (viewerMouseInput != null)
            {
                viewerMouseInput.CancelComponentRotation();
            }

            HideGizmo();
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            if (toolController == null) toolController = GetComponentInParent<SparkToolController>();
            if (toolController == null) toolController = FindFirstObjectByType<SparkToolController>();
            if (selectionController == null)
            {
                selectionController =
                    GetComponentInParent<
                        SparkSelectionController>();
            }

            if (objectController == null)
            {
                objectController =
                    GetComponentInParent<
                        HolographicViewer.HolographicObjectController>();
            }

            if (rotationGizmo == null)
            {
                rotationGizmo =
                    GetComponentInParent<
                        HolographicViewer.SparkRotationGizmo>();
            }

            if (viewerMouseInput == null)
            {
                viewerMouseInput =
                    GetComponentInParent<
                        HolographicViewer.HolographicViewerMouseInput>();
            }
        }

        // ============================================================
        // SHOW
        // ============================================================

        private void ShowGizmo()
        {
            if (!IsRotateToolActive) { HideGizmo(); return; }
            if (rotationGizmo == null)
            {
                return;
            }

            Transform target =
                CurrentTarget;

            if (target == null)
            {
                rotationGizmo.Hide();
                return;
            }

            rotationGizmo.SetTarget(
                target,
                target.position);

            rotationGizmo.Show();
        }

        // ============================================================
        // UPDATE TARGET
        // ============================================================

        private void UpdateGizmoTarget()
        {
            if (!IsRotateToolActive) { HideGizmo(); return; }
            if (rotationGizmo == null)
            {
                return;
            }

            Transform target =
                CurrentTarget;

            if (target == null)
            {
                rotationGizmo.Hide();
                return;
            }

            rotationGizmo.SetTarget(
                target,
                target.position);

            if (!rotationGizmo.IsVisible)
            {
                rotationGizmo.Show();
            }
        }

        // ============================================================
        // HIDE
        // ============================================================

        private void HideGizmo()
        {
            if (rotationGizmo == null)
            {
                return;
            }

            rotationGizmo.Cancel();
            rotationGizmo.Hide();
        }

        private void OnToolChanged(SparkTool previous, SparkTool next)
        {
            if (next == this) ShowGizmo();
            else
            {
                if (viewerMouseInput != null) viewerMouseInput.CancelComponentRotation();
                HideGizmo();
            }
        }
    }
}

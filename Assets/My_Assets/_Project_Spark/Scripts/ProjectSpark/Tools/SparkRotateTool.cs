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
    public sealed class SparkRotateTool : MonoBehaviour
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
            isActiveAndEnabled;

        // ============================================================
        // INITIALIZATION
        // ============================================================

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            ShowGizmo();
        }

        private void Update()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            UpdateGizmoTarget();
        }

        private void OnDisable()
        {
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
    }
}
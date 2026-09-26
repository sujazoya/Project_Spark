using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Owns the currently selected Project Spark object.
    ///
    /// This component is the single source of truth for selection.
    ///
    /// Responsibilities:
    /// - Raycast the scene for selectable objects.
    /// - Select an object under the pointer.
    /// - Clear selection when empty space is clicked.
    /// - Deselect the previous object automatically.
    /// - Maintain the current SparkSelectable.
    /// - Notify listeners when selection changes.
    ///
    /// This component does NOT:
    /// - Move objects.
    /// - Rotate objects.
    /// - Orbit the camera.
    /// - Control tools.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSelectionController : MonoBehaviour
    {
        // ============================================================
        // SELECTION
        // ============================================================

        [Header("Selection")]

        [Tooltip("Camera used for selection raycasts.")]
        [SerializeField]
        private Camera selectionCamera;

        [Tooltip("Layers that can contain selectable objects.")]
        [SerializeField]
        private LayerMask selectableLayers = ~0;

        [Tooltip("Maximum distance of the selection ray.")]
        [SerializeField]
        [Min(0.01f)]
        private float selectionDistance = 1000f;


        // ============================================================
        // OPTIONS
        // ============================================================

        [Header("Options")]

        [Tooltip(
            "If enabled, clicking empty space clears the current selection.")]
        [SerializeField]
        private bool clearSelectionOnEmptyClick = true;

        [Tooltip(
            "If enabled, clicking the currently selected object keeps " +
            "the selection without firing SelectionChanged.")]
        [SerializeField]
        private bool keepCurrentSelectionOnRepeatClick = true;


        // ============================================================
        // RUNTIME
        // ============================================================

        private SparkSelectable selectedObject;


        // ============================================================
        // PUBLIC STATE
        // ============================================================

        /// <summary>
        /// Currently selected Project Spark object.
        /// </summary>
        public SparkSelectable SelectedObject
        {
            get
            {
                return selectedObject;
            }
        }


        /// <summary>
        /// Returns true when an object is currently selected.
        /// </summary>
        public bool HasSelection
        {
            get
            {
                return selectedObject != null;
            }
        }


        /// <summary>
        /// Camera currently used for selection.
        /// </summary>
        public Camera SelectionCamera
        {
            get
            {
                return selectionCamera;
            }
        }


        // ============================================================
        // EVENTS
        // ============================================================

        /// <summary>
        /// Fired when the selected object changes.
        ///
        /// Parameters:
        /// previous selection, new selection
        /// </summary>
        public event Action<SparkSelectable, SparkSelectable>
            SelectionChanged;


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ResolveCamera();
        }

        private void OnDisable()
        {
            ClearSelection();
        }


        // ============================================================
        // CAMERA
        // ============================================================

        /// <summary>
        /// Resolves the selection camera if one was not assigned.
        /// </summary>
        private void ResolveCamera()
        {
            if (selectionCamera != null)
            {
                return;
            }

            selectionCamera =
                Camera.main;
        }


        /// <summary>
        /// Assigns the camera used by the selection system.
        /// </summary>
        public void SetSelectionCamera(
            Camera camera)
        {
            selectionCamera =
                camera;
        }


        // ============================================================
        // POINTER SELECTION
        // ============================================================

        /// <summary>
        /// Attempts to select the Project Spark object under
        /// the supplied screen position.
        ///
        /// Clicking empty space can optionally clear selection.
        /// </summary>
        public bool TrySelect(
            Vector2 screenPosition,
            out string reason)
        {
            reason = null;

            ResolveCamera();

            if (selectionCamera == null)
            {
                reason =
                    "Selection camera is not configured.";

                return false;
            }

            if (selectionDistance <= 0f)
            {
                reason =
                    "Selection distance must be greater than zero.";

                return false;
            }


            // --------------------------------------------------------
            // Create pointer ray
            // --------------------------------------------------------

            Ray ray =
                selectionCamera.ScreenPointToRay(
                    screenPosition);


            // --------------------------------------------------------
            // Raycast
            // --------------------------------------------------------

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    selectionDistance,
                    selectableLayers,
                    QueryTriggerInteraction.Ignore))
            {
                if (clearSelectionOnEmptyClick)
                {
                    ClearSelection();
                }

                reason =
                    "No selectable object.";

                return false;
            }


            // --------------------------------------------------------
            // Find selectable parent
            // --------------------------------------------------------

            SparkSelectable selectable =
                hit.collider.GetComponentInParent<
                    SparkSelectable>();


            // --------------------------------------------------------
            // Non-selectable collider
            // --------------------------------------------------------

            if (selectable == null)
            {
                if (clearSelectionOnEmptyClick)
                {
                    ClearSelection();
                }

                reason =
                    "Hit object is not selectable.";

                return false;
            }


            // --------------------------------------------------------
            // Select
            // --------------------------------------------------------

            Select(
                selectable);

            return true;
        }


        // ============================================================
        // DIRECT SELECTION
        // ============================================================

        /// <summary>
        /// Makes the supplied SparkSelectable the active selection.
        ///
        /// The previous selection is automatically deselected.
        /// </summary>
        public void Select(
            SparkSelectable selectable)
        {
            if (selectable == null)
            {
                ClearSelection();
                return;
            }


            // --------------------------------------------------------
            // Same object
            // --------------------------------------------------------

            if (selectedObject == selectable)
            {
                if (!keepCurrentSelectionOnRepeatClick)
                {
                    SelectionChanged?.Invoke(
                        selectedObject,
                        selectedObject);
                }

                return;
            }


            // --------------------------------------------------------
            // Previous object
            // --------------------------------------------------------

            SparkSelectable previous =
                selectedObject;

            if (previous != null)
            {
                previous.SetSelected(false);
            }


            // --------------------------------------------------------
            // New object
            // --------------------------------------------------------

            selectedObject =
                selectable;

            selectedObject.SetSelected(true);


            // --------------------------------------------------------
            // Notify
            // --------------------------------------------------------

            SelectionChanged?.Invoke(
                previous,
                selectedObject);
        }


        // ============================================================
        // CLEAR
        // ============================================================

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            if (selectedObject == null)
            {
                return;
            }

            SparkSelectable previous =
                selectedObject;

            selectedObject =
                null;

            previous.SetSelected(false);

            SelectionChanged?.Invoke(
                previous,
                null);
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Ensures the current selection is still valid.
        ///
        /// Useful when an electronic component is destroyed,
        /// disabled or removed from the scene.
        /// </summary>
        public void ValidateSelection()
        {
            if (selectedObject == null)
            {
                return;
            }

            if (!selectedObject.isActiveAndEnabled)
            {
                ClearSelection();
            }
        }


        /// <summary>
        /// Returns the selected object's transform when available.
        /// </summary>
        public bool TryGetSelectedTransform(
            out Transform target)
        {
            target = null;

            if (selectedObject == null)
            {
                return false;
            }

            target =
                selectedObject.transform;

            return target != null;
        }


        // ============================================================
        // HIT TEST
        // ============================================================

        /// <summary>
        /// Performs a selection raycast without changing selection.
        ///
        /// Useful for tools that need to inspect what is under
        /// the pointer without taking ownership of selection.
        /// </summary>
        public bool TryGetSelectableUnderPointer(
            Vector2 screenPosition,
            out SparkSelectable selectable,
            out RaycastHit hit)
        {
            selectable = null;
            hit = default;

            ResolveCamera();

            if (selectionCamera == null)
            {
                return false;
            }

            Ray ray =
                selectionCamera.ScreenPointToRay(
                    screenPosition);

            if (!Physics.Raycast(
                    ray,
                    out hit,
                    selectionDistance,
                    selectableLayers,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            selectable =
                hit.collider.GetComponentInParent<
                    SparkSelectable>();

            return selectable != null;
        }


        // ============================================================
        // EDITOR VALIDATION
        // ============================================================

        private void OnValidate()
        {
            if (selectionDistance < 0.01f)
            {
                selectionDistance =
                    0.01f;
            }
        }
    }
}
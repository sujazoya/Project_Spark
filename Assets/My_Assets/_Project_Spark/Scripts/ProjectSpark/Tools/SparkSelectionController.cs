using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Owns the currently selected Project Spark object.
    /// Selection remains active while different tools operate on it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSelectionController : MonoBehaviour
    {
        [SerializeField]
        private Camera selectionCamera;

        [SerializeField]
        private LayerMask selectableLayers = ~0;

        [SerializeField, Min(0.01f)]
        private float selectionDistance = 1000f;

        private SparkSelectable selectedObject;

        public SparkSelectable SelectedObject
        {
            get { return selectedObject; }
        }

        public bool HasSelection
        {
            get { return selectedObject != null; }
        }

        public event Action<SparkSelectable, SparkSelectable>
            SelectionChanged;

        private void Awake()
        {
            if (selectionCamera == null)
            {
                selectionCamera = Camera.main;
            }
        }

        /// <summary>
        /// Attempts to select the Spark object under the pointer.
        /// </summary>
        public bool TrySelect(
            Vector2 screenPosition,
            out string reason)
        {
            reason = null;

            if (selectionCamera == null)
            {
                reason =
                    "Selection camera is not configured.";

                return false;
            }

            Ray ray =
                selectionCamera.ScreenPointToRay(
                    screenPosition);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    selectionDistance,
                    selectableLayers,
                    QueryTriggerInteraction.Ignore))
            {
                ClearSelection();
                reason = "No selectable object.";
                return false;
            }

            SparkSelectable selectable =
                hit.collider.GetComponentInParent<SparkSelectable>();

            if (selectable == null)
            {
                ClearSelection();
                reason = "Hit object is not selectable.";
                return false;
            }

            Select(selectable);

            return true;
        }

        /// <summary>
        /// Selects a specific Spark object.
        /// </summary>
        public void Select(
            SparkSelectable selectable)
        {
            if (selectable == null)
            {
                ClearSelection();
                return;
            }

            if (selectedObject == selectable)
            {
                return;
            }

            SparkSelectable previous =
                selectedObject;

            if (previous != null)
            {
                previous.SetSelected(false);
            }

            selectedObject =
                selectable;

            selectedObject.SetSelected(true);

            SelectionChanged?.Invoke(
                previous,
                selectedObject);
        }

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

            selectedObject = null;

            previous.SetSelected(false);

            SelectionChanged?.Invoke(
                previous,
                null);
        }
    }
}
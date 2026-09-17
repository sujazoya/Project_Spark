using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Owns the currently selected Project Spark object.
    /// This is the single source of truth for selection.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSelectionController : MonoBehaviour
    {
        [Header("Selection")]
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
        /// Selects the object under the supplied screen position.
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

                reason =
                    "No selectable object.";

                return false;
            }

            SparkSelectable selectable =
                hit.collider.GetComponentInParent<
                    SparkSelectable>();

            if (selectable == null)
            {
                ClearSelection();

                reason =
                    "Hit object is not selectable.";

                return false;
            }

            Select(selectable);

            return true;
        }

        /// <summary>
        /// Makes the supplied object the active selection.
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
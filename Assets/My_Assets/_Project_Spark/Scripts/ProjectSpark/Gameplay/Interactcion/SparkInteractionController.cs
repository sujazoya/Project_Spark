
using UnityEngine;
using UnityEngine.EventSystems;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Central ProjectSpark interaction controller.
    ///
    /// Supports:
    /// - Normal SparkInteractable interactions
    /// - Switch controls
    /// - Visibility panels
    /// - Two-click manual shelf placement
    /// - Level-specific workstation slots
    /// - Hover highlighting
    ///
    /// Uses RaycastAll to detect valid interactions
    /// behind non-interactive decorative colliders.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkInteractionController : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField]
        private Camera targetCamera;

        [Header("Raycast")]
        [SerializeField, Min(0.1f)]
        private float interactionDistance = 20f;

        [SerializeField]
        private LayerMask interactionLayers = ~0;

        [SerializeField]
        private QueryTriggerInteraction triggerInteraction =
            QueryTriggerInteraction.Ignore;

        [Header("Input")]
        [SerializeField]
        private KeyCode interactKey = KeyCode.Mouse0;

        [SerializeField]
        private bool ignoreClicksOverUI = true;

        [Header("Manual Shelf Placement")]
        [SerializeField]
        private SparkManualShelfPlacement manualPlacement;

        [SerializeField]
        private bool cancelShelfSelectionOnEscape = true;

        [SerializeField]
        private bool cancelShelfSelectionOnEmptyClick = false;

        [Header("Spark AI Component Memory")]
        [SerializeField]
        private ProjectSpark.AI.Tutorial.SparkAIUnifiedAssistant
            askSpark;

        [SerializeField]
        private bool rememberComponentsAutomatically = true;

        [Header("Settings")]
        [SerializeField]
        private bool interactionEnabled = true;        
        [Header("Drag And Drop")]
        [SerializeField]
        private SparkShelfDragHandler shelfDragHandler;

        private SparkInteractable lastClickedInteractable;
        [SerializeField]
        private ProjectSpark.AI.Tutorial.SparkAIComponentInspector
        componentInspector;        

        [SerializeField]
        private bool highlightTarget = true;

        [Header("Debug")]
        [SerializeField]
        private bool logRaycastHits = false;

        [SerializeField]
        private bool logPlacementResults = true;

        [Header("Runtime")]
        [SerializeField]
        private SparkInteractable currentTarget;

        [SerializeField]
        private SparkInteractable previousTarget;

        private SparkShelfSlotClickTarget currentSlotTarget;

        public SparkInteractable CurrentTarget => currentTarget;

        public bool HasTarget => currentTarget != null;

        public bool InteractionEnabled => interactionEnabled;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            if (!interactionEnabled)
            {
                ClearTarget();
                return;
            }

            if (cancelShelfSelectionOnEscape &&
                Input.GetKeyDown(KeyCode.Escape))
            {
                manualPlacement?.CancelSelection();
            }

            if (IsPointerOverUI())
            {
                ClearTarget();
                return;
            }
            shelfDragHandler?.Tick();

            UpdateTarget();
            if (Input.GetKeyDown(interactKey))
            {
                if (shelfDragHandler == null ||
                    !shelfDragHandler.ConsumeInteractionClick())
                {
                    TryInteract();
                }
            }
           if (Input.GetKeyDown(KeyCode.I))
            {
                if (currentTarget != null)
                {
                    if (componentInspector != null)
                    {
                        componentInspector.Inspect(currentTarget);
                    }

                    RememberForSparkAI(currentTarget.gameObject);
                }
            }
        }

        private void OnDisable()
        {
            ClearTarget();
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (manualPlacement == null)
            {
                manualPlacement =
                    FindFirstObjectByType<
                        SparkManualShelfPlacement>();
            }
            if (shelfDragHandler == null)
            {
                shelfDragHandler =
                    GetComponent<SparkShelfDragHandler>();
            }
            if (askSpark == null)
            {
                askSpark =
                    FindFirstObjectByType<
                        ProjectSpark.AI.Tutorial.SparkAIUnifiedAssistant>();
            }
        }

        // ============================================================
        // RAYCAST TARGET DETECTION
        // ============================================================

        private void UpdateTarget()
        {
            SparkInteractable detectedInteractable = null;
            SparkShelfSlotClickTarget detectedSlot = null;

            FindTargets(
                out detectedInteractable,
                out detectedSlot);

            currentSlotTarget = detectedSlot;

            if (detectedInteractable == currentTarget)
                return;

            previousTarget = currentTarget;
            currentTarget = detectedInteractable;

            UpdateHighlightState();
        }

        private void FindTargets(
            out SparkInteractable interactable,
            out SparkShelfSlotClickTarget slotTarget)
        {
            interactable = null;
            slotTarget = null;

            if (targetCamera == null)
                ResolveReferences();

            if (targetCamera == null)
                return;

            Ray ray = targetCamera.ScreenPointToRay(
                Input.mousePosition);

            RaycastHit[] hits = Physics.RaycastAll(
                ray,
                interactionDistance,
                interactionLayers,
                triggerInteraction);

            if (hits == null || hits.Length == 0)
                return;

            // RaycastAll is not distance-sorted.
            System.Array.Sort(
                hits,
                (a, b) => a.distance.CompareTo(b.distance));

            bool shouldLog =
                logRaycastHits &&
                Input.GetKeyDown(interactKey);

            foreach (RaycastHit hit in hits)
            {
                Collider hitCollider = hit.collider;

                if (hitCollider == null)
                    continue;

                if (shouldLog)
                {
                    Debug.Log(
                        "[SparkInteraction] Raycast hit: " +
                        hitCollider.name +
                        " | Distance: " +
                        hit.distance.ToString("F2") +
                        " | Layer: " +
                        LayerMask.LayerToName(
                            hitCollider.gameObject.layer),
                        hitCollider);
                }

                // --------------------------------------------
                // WORKSTATION SLOT
                // --------------------------------------------

                SparkShelfSlotClickTarget slot =
                    hitCollider.GetComponentInParent<
                        SparkShelfSlotClickTarget>();

                if (slot != null &&
                    slot.isActiveAndEnabled &&
                    slot.gameObject.activeInHierarchy)
                {
                    slotTarget = slot;
                    return;
                }

                // --------------------------------------------
                // NORMAL INTERACTABLE
                // --------------------------------------------

                SparkInteractable target =
                    FindInteractable(hitCollider);

                    

                if (target != null)
                {
                    interactable = target;
                    return;
                }

                // Non-interactive decorative colliders
                // are skipped.
            }
        }

        private SparkInteractable FindInteractable(
            Collider hitCollider)
        {
            if (hitCollider == null)
                return null;

            SparkInteractable interactable =
                hitCollider.GetComponent<SparkInteractable>();

            if (interactable == null)
            {
                interactable =
                    hitCollider.GetComponentInParent<
                        SparkInteractable>();
            }

            if (interactable == null)
            {
                interactable =
                    hitCollider.GetComponentInChildren<
                        SparkInteractable>(true);
            }

            if (interactable == null ||
                !interactable.CanInteract())
            {
                return null;
            }

            return interactable;
        }

        
private void RememberForSparkAI(GameObject selectedObject)
{
    if (!rememberComponentsAutomatically ||
        selectedObject == null)
    {
        return;
    }

    if (askSpark == null)
    {
        askSpark =
            FindFirstObjectByType<
                ProjectSpark.AI.Tutorial.SparkAIUnifiedAssistant>();
    }

    if (askSpark == null)
        return;

    askSpark.RememberComponent(selectedObject);

    if (logPlacementResults)
    {
        Debug.Log(
            "[SparkInteraction] Spark AI remembered: " +
            selectedObject.name,
            selectedObject);
    }
}


        // ============================================================
        // INTERACTION
        // ============================================================

        

private void TryInteract()
{
    // ========================================================
    // PRIORITY 1: WORKSTATION SLOT
    // ========================================================

    if (currentSlotTarget != null)
    {
        // Single-click an empty workstation slot
        // to place the currently selected shelf item.
        if (manualPlacement != null &&
            manualPlacement.HasSelection)
        {
            TryPlaceAtSlot(currentSlotTarget);
        }

        return;
    }

    // ========================================================
    // GET CURRENT INTERACTABLE
    // ========================================================

    SparkInteractable target = currentTarget;

    // ========================================================
    // NO TARGET
    // ========================================================

    if (target == null)
    {
        if (cancelShelfSelectionOnEmptyClick)
        {
            manualPlacement?.CancelSelection();
        }

        return;
    }

    // ========================================================
    // VALIDATE INTERACTION
    // ========================================================

    if (!target.CanInteract())
        return;

    // ========================================================
    // PRIORITY 2: SHELF COMPONENT
    // ========================================================

    // Detect actual shelf items even when the
    // SparkInteractable.IsShelfItem flag is incorrect.
    SparkShelfItem shelfItem = FindShelfItem(target);

    
if (shelfItem != null)
{
    // A component already on the workbench can
    // still be inspected and remembered by Spark AI.
    if (shelfItem.IsAtWorkbench)
    {
        RememberForSparkAI(shelfItem.gameObject);

        // Preserve existing drag-and-drop behavior.
        return;
    }

    // The normal interaction system performs the
    // real shelf selection and its validation.
  
target.Interact();

// Fallback when the normal interaction did not
// select the shelf component.
if (manualPlacement != null &&
    manualPlacement.SelectedItem != shelfItem &&
    shelfItem != null &&
    shelfItem.isActiveAndEnabled &&
    !shelfItem.IsAtWorkbench)
{
    manualPlacement.SelectItem(shelfItem);
}


    // Remember the component only when the
    // manual shelf system confirms its selection.
    if (manualPlacement != null &&
        manualPlacement.SelectedItem == shelfItem)
    {
        RememberForSparkAI(shelfItem.gameObject);
    }

    return;
}

    // ========================================================
    // PRIORITY 3: VISIBILITY PANEL
    // ========================================================

    SparkObjectVisibilityController visibility =
        FindVisibilityController(target);

    if (visibility != null)
    {
        visibility.OpenPanel();
        return;
    }

    // ========================================================
    // PRIORITY 4: NORMAL INTERACTION
    // ========================================================

    target.Interact();
}

private SparkShelfItem FindShelfItem(SparkInteractable target)
{
    if (target == null)
        return null;

    SparkShelfItem item =
        target.GetComponent<SparkShelfItem>();

    if (item == null)
        item = target.GetComponentInParent<SparkShelfItem>();

    if (item == null)
        item = target.GetComponentInChildren<SparkShelfItem>(true);

    return item;
}
        private void TryPlaceAtSlot(
            SparkShelfSlotClickTarget clickTarget)
        {
            if (clickTarget == null)
                return;

            if (manualPlacement == null)
                ResolveReferences();

            if (manualPlacement == null)
            {
                Debug.LogWarning(
                    "[ManualShelf] Manual placement controller missing.",
                    this);
                return;
            }

            if (!manualPlacement.HasSelection)
            {
                if (logPlacementResults)
                {
                    Debug.Log(
                        "[ManualShelf] Slot clicked, " +
                        "but no shelf item is selected.",
                        clickTarget);
                }

                return;
            }

            SparkShelfSlot slot = clickTarget.Slot;

            if (slot == null)
            {
                Debug.LogWarning(
                    "[ManualShelf] Click target has no assigned slot.",
                    clickTarget);
                return;
            }

            SparkShelfItem selected =
                manualPlacement.SelectedItem;

            bool success =
                manualPlacement.TryPlaceAt(slot);
                if (success && selected != null)
                {
                    RememberForSparkAI(selected.gameObject);
                }

            if (logPlacementResults)
            {
                Debug.Log(
                    "[ManualShelf] Item: " +
                    (selected != null
                        ? selected.ItemId
                        : "None") +
                    " | Slot: " + slot.SlotId +
                    " | Placement: " +
                    (success ? "SUCCESS" : "REJECTED"),
                    slot);
            }
        }

        // ============================================================
        // VISIBILITY
        // ============================================================

        private SparkObjectVisibilityController
            FindVisibilityController(
                SparkInteractable interactable)
        {
            if (interactable == null)
                return null;

            SparkObjectVisibilityController controller =
                interactable.GetComponent<
                    SparkObjectVisibilityController>();

            if (controller != null)
                return controller;

            controller =
                interactable.GetComponentInParent<
                    SparkObjectVisibilityController>();

            if (controller != null)
                return controller;

            return interactable.GetComponentInChildren<
                SparkObjectVisibilityController>(true);
        }

        // ============================================================
        // HIGHLIGHT
        // ============================================================

        private void UpdateHighlightState()
        {
            if (!highlightTarget)
                return;

            if (previousTarget != null)
                previousTarget.SetHighlighted(false);

            if (currentTarget != null)
                currentTarget.SetHighlighted(true);
        }

        private void ClearTarget()
        {
            if (currentTarget != null)
                currentTarget.SetHighlighted(false);

            if (previousTarget != null &&
                previousTarget != currentTarget)
            {
                previousTarget.SetHighlighted(false);
            }

            currentTarget = null;
            previousTarget = null;
            currentSlotTarget = null;
        }

        // ============================================================
        // UI
        // ============================================================

        private bool IsPointerOverUI()
        {
            if (!ignoreClicksOverUI)
                return false;

            return EventSystem.current != null &&
                   EventSystem.current.IsPointerOverGameObject();
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        public void SetInteractionEnabled(bool enabled)
        {
            interactionEnabled = enabled;

            if (!enabled)
                ClearTarget();
        }

        public void SetInteractionDistance(float distance)
        {
            interactionDistance = Mathf.Max(0.1f, distance);
        }

        public void SetCamera(Camera camera)
        {
            targetCamera = camera;
        }

        public void SetManualPlacement(
            SparkManualShelfPlacement placement)
        {
            manualPlacement = placement;
        }

        [ContextMenu("Test / Clear Target")]
        private void TestClearTarget()
        {
            ClearTarget();
        }

    }

}

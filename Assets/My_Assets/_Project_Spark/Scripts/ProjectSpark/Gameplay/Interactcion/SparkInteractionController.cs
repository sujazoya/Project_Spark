
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Central Project Spark interaction controller.
    ///
    /// One raycast determines:
    /// 1. Shelf item
    /// 2. Workstation slot
    /// 3. Normal interactable
    ///
    /// Recycle selection works independently of
    /// SparkInteractable.CanInteract().
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

        [Header("Recycle Icon")]
        [SerializeField]
        private SparkShelfRemovePrompt removePrompt;

        [Header("Drag And Drop")]
        [SerializeField]
        private SparkShelfDragHandler shelfDragHandler;

        [Header("Spark AI Component Memory")]
        [SerializeField]
        private ProjectSpark.AI.Tutorial.SparkAIUnifiedAssistant
            askSpark;

        [SerializeField]
        private bool rememberComponentsAutomatically = true;

        [Header("Component Inspector")]
        [SerializeField]
        private ProjectSpark.AI.Tutorial.SparkAIComponentInspector
            componentInspector;

        [Header("Settings")]
        [SerializeField]
        private bool interactionEnabled = true;

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
        private SparkShelfItem currentShelfItem;

        public SparkInteractable CurrentTarget => currentTarget;
        public bool HasTarget => currentTarget != null;
        public bool InteractionEnabled => interactionEnabled;

        // =====================================================
        // UNITY
        // =====================================================

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            if (!interactionEnabled)
            {
                removePrompt?.ClearSelection();
                ClearTarget();
                return;
            }

            if (cancelShelfSelectionOnEscape &&
                Input.GetKeyDown(KeyCode.Escape))
            {
                manualPlacement?.CancelSelection();
                removePrompt?.ClearSelection();
            }

            // Unity UI owns pointer interactions.
            if (!SparkSelectedObjectActions.SelectToolEnabled)
            {
                shelfDragHandler?.CancelDrag();
                removePrompt?.ClearSelection();
                manualPlacement?.CancelSelection();
            }

            // This prevents clicking the recycle button
            // from also interacting with objects behind it.
            if (IsPointerOverUI())
            {
                shelfDragHandler?.CancelDrag();
                ClearTarget();
                return;
            }

            // Preserve existing dragging.
            shelfDragHandler?.Tick();

            // Exactly one physics query for target detection.
            UpdateTarget();

           
if (Input.GetKeyDown(interactKey))
{
    // Select the recycle target BEFORE the drag handler
    // gets a chance to consume the mouse click.
    if (currentShelfItem != null && SparkSelectedObjectActions.TrySelectItem(currentShelfItem))
    {
        if (removePrompt == null)
            ResolveReferences();

        removePrompt?.SelectItem(currentShelfItem);

        Debug.Log(
            "[ShelfRemove] Click routed: " +
            currentShelfItem.name,
            currentShelfItem);
    }
    else
    {
        removePrompt?.ClearSelection();
    }

    bool consumed =
        shelfDragHandler != null &&
        shelfDragHandler.ConsumeInteractionClick();

    if (!consumed)
        HandleInteractionClick();
}


            if (Input.GetKeyDown(KeyCode.I))
            {
                if (currentTarget != null)
                {
                    componentInspector?.Inspect(currentTarget);

                    RememberForSparkAI(
                        currentTarget.gameObject);
                }
                else if (currentShelfItem != null)
                {
                    RememberForSparkAI(
                        currentShelfItem.gameObject);
                }
            }
        }

        private void OnDisable()
        {
            removePrompt?.ClearSelection();
            ClearTarget();
        }

        // =====================================================
        // REFERENCES
        // =====================================================

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

            if (removePrompt == null)
            {
                removePrompt =
                    FindFirstObjectByType<
                        SparkShelfRemovePrompt>();
            }

            if (askSpark == null)
            {
                askSpark =
                    FindFirstObjectByType<
                        ProjectSpark.AI.Tutorial
                        .SparkAIUnifiedAssistant>();
            }
        }

        // =====================================================
        // TARGET DETECTION
        // =====================================================

        private void UpdateTarget()
        {
            FindTargets(
                out SparkShelfItem detectedItem,
                out SparkInteractable detectedInteractable,
                out SparkShelfSlotClickTarget detectedSlot);

            currentShelfItem = detectedItem;

            // An item has priority over the slot behind it.
            currentSlotTarget =
                detectedItem != null ? null : detectedSlot;

            if (detectedInteractable == currentTarget)
                return;

            previousTarget = currentTarget;
            currentTarget = detectedInteractable;

            UpdateHighlightState();
        }

        private void FindTargets(
            out SparkShelfItem shelfItem,
            out SparkInteractable interactable,
            out SparkShelfSlotClickTarget slotTarget)
        {
            shelfItem = null;
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

            Array.Sort(
                hits,
                (a, b) =>
                    a.distance.CompareTo(b.distance));

            // ---------------------------------------------
            // PASS 1: SHELF ITEMS
            // Do not require SparkInteractable.
            // ---------------------------------------------

            foreach (RaycastHit hit in hits)
            {
                Collider collider = hit.collider;

                if (collider == null)
                    continue;

                if (logRaycastHits &&
                    Input.GetKeyDown(interactKey))
                {
                    Debug.Log(
                        "[SparkInteraction] Raycast hit: " +
                        collider.name +
                        " | Distance: " +
                        hit.distance.ToString("F2") +
                        " | Layer: " +
                        LayerMask.LayerToName(
                            collider.gameObject.layer),
                        collider);
                }

                SparkShelfItem item =
                    collider.GetComponentInParent<
                        SparkShelfItem>();

                if (item == null ||
                    !item.isActiveAndEnabled ||
                    !item.gameObject.activeInHierarchy)
                {
                    continue;
                }

                shelfItem = item;

                // Resolve the item's actual interactable,
                // not an unrelated interactable on the slot.
                interactable =
                    item.GetComponent<SparkInteractable>();

                if (interactable == null)
                {
                    interactable =
                        item.GetComponentInChildren<
                            SparkInteractable>(true);
                }

                if (interactable != null &&
                    !interactable.CanInteract())
                {
                    interactable = null;
                }

                return;
            }

            // ---------------------------------------------
            // PASS 2: SLOT / NORMAL INTERACTABLE
            // ---------------------------------------------

            foreach (RaycastHit hit in hits)
            {
                Collider collider = hit.collider;

                if (collider == null)
                    continue;

                // A fixed SPDT sits below a slot target. A hit on its own
                // collider operates the switch rather than selecting that slot.
                SparkInteractable spdtTarget = FindInteractable(collider);
                if (spdtTarget != null)
                {
                    var spdt = spdtTarget.GetComponentInChildren<SparkSpdtInteraction>(true);
                    if (spdt != null && spdt.isActiveAndEnabled)
                    {
                        interactable = spdtTarget;
                        return;
                    }
                }

                SparkShelfSlotClickTarget slot =
                    collider.GetComponentInParent<
                        SparkShelfSlotClickTarget>();

                if (slot != null &&
                    slot.isActiveAndEnabled &&
                    slot.gameObject.activeInHierarchy)
                {
                    slotTarget = slot;
                    return;
                }

                SparkInteractable target =
                    FindInteractable(collider);

                if (target != null)
                {
                    interactable = target;
                    return;
                }
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
                !interactable.isActiveAndEnabled ||
                !interactable.CanInteract())
            {
                return null;
            }

            return interactable;
        }

        // =====================================================
        // SINGLE CLICK ROUTER
        // =====================================================

        private void HandleInteractionClick()
        {
            // ---------------------------------------------
            // PRIORITY 1: SHELF ITEM
            // ---------------------------------------------

            if (currentShelfItem != null)
            {
                SparkShelfItem item = currentShelfItem;

                if (!SparkSelectedObjectActions.SelectToolEnabled)
                {
                    // Operating a placed switch is separate from moving/recycling it.
                    if (item.IsAtWorkbench && currentTarget != null && currentTarget.CanInteract())
                    {
                        var spdt = currentTarget.GetComponentInChildren<SparkSpdtInteraction>(true);
                        if (spdt != null && spdt.isActiveAndEnabled) currentTarget.Interact();
                    }
                    return;
                }
                if (!SparkSelectedObjectActions.TrySelectItem(item)) return;

                // Exactly one recycle selection call.
               /* if (removePrompt == null)
                    ResolveReferences();

                removePrompt?.SelectItem(item);*/

                if (logPlacementResults)
                {
                    Debug.Log(
                        "[SparkInteraction] Shelf clicked: " +
                        item.name +
                        " | Workbench=" +
                        item.IsAtWorkbench +
                        " | Prompt=" +
                        (removePrompt != null
                            ? "FOUND"
                            : "MISSING"),
                        item);
                }

                // Installed components:
                // Show recycle icon without triggering
                // placement or movement.
                if (item.IsAtWorkbench)
                {
                    if (currentTarget != null && currentTarget.CanInteract())
                    {
                        var spdt = currentTarget.GetComponentInChildren<SparkSpdtInteraction>(true);
                        if (spdt != null && spdt.isActiveAndEnabled)
                            currentTarget.Interact();
                    }
                    RememberForSparkAI(item.gameObject);
                    return;
                }

                // Storage components:
                // Preserve normal shelf interaction.
                InteractWithShelfItem(item);
                return;
            }

            // ---------------------------------------------
            // PRIORITY 2: OTHER WORLD OBJECTS
            // ---------------------------------------------

            // Only clear when no shelf item was clicked.
            removePrompt?.ClearSelection();

            TryInteract();
        }

        // =====================================================
        // STORAGE ITEM INTERACTION
        // =====================================================

        private void InteractWithShelfItem(
            SparkShelfItem item)
        {
            if (item == null)
                return;

            SparkInteractable target = currentTarget;

            bool shelfInteractionHandled = false;
            if (target != null && target.CanInteract())
            {
                shelfInteractionHandled = target.Type == SparkInteractable.InteractionType.ShelfItem;
                target.Interact();
            }

            // Preserve manual selection fallback.
            if (!shelfInteractionHandled && manualPlacement != null &&
                manualPlacement.SelectedItem != item &&
                item.isActiveAndEnabled &&
                !item.IsAtWorkbench)
            {
                manualPlacement.SelectItem(item);
            }

            if (manualPlacement != null &&
                manualPlacement.SelectedItem == item)
            {
                RememberForSparkAI(item.gameObject);
            }
        }

        // =====================================================
        // NORMAL WORLD INTERACTION
        // =====================================================

        private void TryInteract()
        {
            // ---------------------------------------------
            // WORKSTATION SLOT
            // ---------------------------------------------

            if (currentSlotTarget != null)
            {
                if (manualPlacement != null &&
                    manualPlacement.HasSelection)
                {
                    TryPlaceAtSlot(currentSlotTarget);
                }

                return;
            }

            // ---------------------------------------------
            // INTERACTABLE
            // ---------------------------------------------

            SparkInteractable target = currentTarget;

            if (target == null)
            {
                if (cancelShelfSelectionOnEmptyClick)
                {
                    manualPlacement?.CancelSelection();
                }

                return;
            }

            if (!target.CanInteract())
                return;

            // ---------------------------------------------
            // VISIBILITY PANEL
            // ---------------------------------------------

            SparkObjectVisibilityController visibility =
                FindVisibilityController(target);

            if (visibility != null)
            {
                visibility.OpenPanel();
                return;
            }

            // ---------------------------------------------
            // NORMAL INTERACTION
            // ---------------------------------------------

            target.Interact();
        }

        // =====================================================
        // SLOT PLACEMENT
        // =====================================================

        private void TryPlaceAtSlot(
            SparkShelfSlotClickTarget clickTarget)
        {
            if (!SparkSelectedObjectActions.SelectToolEnabled) return;
            if (clickTarget == null)
                return;

            if (manualPlacement == null)
                ResolveReferences();

            if (manualPlacement == null)
            {
                Debug.LogWarning(
                    "[ManualShelf] Placement controller missing.",
                    this);
                return;
            }

            if (!manualPlacement.HasSelection)
            {
                if (logPlacementResults)
                {
                    Debug.Log(
                        "[ManualShelf] No item selected.",
                        clickTarget);
                }

                return;
            }

            SparkShelfSlot slot = clickTarget.Slot;

            if (slot == null)
            {
                Debug.LogWarning(
                    "[ManualShelf] Click target has no slot.",
                    clickTarget);
                return;
            }

            SparkShelfItem selected =
                manualPlacement.SelectedItem;

            if (!SparkSelectedObjectActions.CanAct(selected)) return;

            bool success =
                manualPlacement.TryPlaceAt(slot);

            if (success && selected != null)
            {
                RememberForSparkAI(selected.gameObject);
            }

            if (logPlacementResults)
            {
                Debug.Log(
                    "[ManualShelf] Item=" +
                    (selected != null
                        ? selected.ItemId
                        : "None") +
                    " | Slot=" + slot.SlotId +
                    " | Result=" +
                    (success ? "SUCCESS" : "REJECTED"),
                    slot);
            }
        }

        // =====================================================
        // VISIBILITY PANELS
        // =====================================================

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

        // =====================================================
        // SPARK AI
        // =====================================================

        private void RememberForSparkAI(
            GameObject selectedObject)
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
                        ProjectSpark.AI.Tutorial
                        .SparkAIUnifiedAssistant>();
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

        // =====================================================
        // HIGHLIGHT
        // =====================================================

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
            currentShelfItem = null;
        }

        // =====================================================
        // UI
        // =====================================================

        private bool IsPointerOverUI()
        {
            if (!ignoreClicksOverUI)
                return false;

            return EventSystem.current != null &&
                   EventSystem.current.IsPointerOverGameObject();
        }

        // =====================================================
        // PUBLIC API
        // =====================================================

        public void SetInteractionEnabled(bool enabled)
        {
            interactionEnabled = enabled;

            if (!enabled)
            {
                removePrompt?.ClearSelection();
                ClearTarget();
            }
        }

        public void SetInteractionDistance(float distance)
        {
            interactionDistance =
                Mathf.Max(0.1f, distance);
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
            removePrompt?.ClearSelection();
        }
    }
}

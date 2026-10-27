
using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.AI.Tutorial;

namespace ProjectSpark.Gameplay.Shelf
{
    /// <summary>
    /// ProjectSpark manual component placement.
    ///
    /// Supports:
    /// - Single-click shelf item selection.
    /// - Single-click placement into workstation slots.
    /// - Drag-and-drop placement.
    /// - Runtime-created workstation slots.
    /// - Electrical movement safety.
    /// - AI tutorial mistake reporting.
    ///
    /// Input remains owned by SparkInteractionController
    /// and SparkShelfDragHandler.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkManualShelfPlacement :
        MonoBehaviour
    {
        // =====================================================
        // REFERENCES
        // =====================================================

        [Header("Shelf System")]
        [SerializeField]
        private SparkObjectShelf shelf;

        [Header("AI Tutorial")]
        [SerializeField]
        private SparkTutorialMistakeCoach tutorialCoach;

        // =====================================================
        // INTERACTION SETTINGS
        // =====================================================

        [Header("Interaction")]
        [Tooltip(
            "Legacy option. Keep OFF when using drag-and-drop " +
            "to return workstation components.")]
        [SerializeField]
        private bool clickPlacedItemToReturn = false;

        [SerializeField]
        private bool logSelection = true;

        [SerializeField]
        private bool logPlacementFailures = true;

        [SerializeField]
        private bool logPlacementSuccess = true;

        [Header("Diagnostics")]
        [SerializeField]
        private bool logPlacementDetails = false;

        // =====================================================
        // ALLOWED ITEMS AND SLOTS
        // =====================================================

        private readonly HashSet<SparkShelfItem>
            allowedItems =
                new HashSet<SparkShelfItem>();

        private readonly HashSet<SparkShelfSlot>
            allowedSlots =
                new HashSet<SparkShelfSlot>();

        private SparkShelfItem selected;

        // =====================================================
        // PUBLIC PROPERTIES
        // =====================================================

        public SparkShelfItem SelectedItem =>
            selected;

        public bool HasSelection =>
            selected != null;

        public int AllowedItemCount =>
            allowedItems.Count;

        public int AllowedSlotCount =>
            allowedSlots.Count;

        // =====================================================
        // EVENTS
        // =====================================================

        public event Action<SparkShelfItem>
            SelectionChanged;

        public event Action<SparkShelfItem, SparkShelfSlot>
            ItemPlaced;

        public event Action<SparkShelfItem, SparkShelfSlot>
            PlacementRejected;

        // =====================================================
        // UNITY
        // =====================================================

        private void Awake()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (shelf == null)
            {
                shelf =
                    GetComponent<SparkObjectShelf>();
            }

            if (shelf == null)
            {
                shelf =
                    FindFirstObjectByType<
                        SparkObjectShelf>();
            }

            if (tutorialCoach == null)
            {
                tutorialCoach =
                    FindFirstObjectByType<
                        SparkTutorialMistakeCoach>();
            }
        }

        // =====================================================
        // LEVEL CONFIGURATION
        // =====================================================

        /// <summary>
        /// Configures the inventory and workstation slots
        /// belonging to the active level.
        ///
        /// Call this when the level is initialized.
        /// Do not call it every frame.
        /// </summary>
        public void Configure(
            SparkShelfItem[] items,
            SparkShelfSlot[] workstationSlots)
        {
            CancelSelection();

            allowedItems.Clear();
            allowedSlots.Clear();

            if (items != null)
            {
                foreach (SparkShelfItem item in items)
                {
                    if (item != null)
                    {
                        allowedItems.Add(item);
                    }
                }
            }

            if (workstationSlots != null)
            {
                foreach (SparkShelfSlot slot
                    in workstationSlots)
                {
                    if (slot == null)
                        continue;

                    if (slot.Kind !=
                        SparkShelfSlotKind.Workstation)
                    {
                        continue;
                    }

                    allowedSlots.Add(slot);
                }
            }

            if (logSelection)
            {
                Debug.Log(
                    "[ManualShelf] Configured level. " +
                    "Allowed Items=" +
                    allowedItems.Count +
                    " | Workstation Slots=" +
                    allowedSlots.Count,
                    this);
            }
        }

        /// <summary>
        /// Removes references to the previous level.
        /// </summary>
        public void ClearLevel()
        {
            CancelSelection();

            allowedItems.Clear();
            allowedSlots.Clear();

            if (logSelection)
            {
                Debug.Log(
                    "[ManualShelf] Level configuration cleared.",
                    this);
            }
        }

        // =====================================================
        // ITEM SELECTION
        // =====================================================

        public bool SelectItem(SparkShelfItem item)
        {
            ResolveReferences();

            // ---------------------------------------------
            // VALIDATE SHELF
            // ---------------------------------------------

            if (shelf == null)
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .GenericPlacementFailure,
                    "SparkObjectShelf is missing.");

                return false;
            }

            // ---------------------------------------------
            // VALIDATE ITEM
            // ---------------------------------------------

            if (item == null)
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .NoSelection,
                    "Requested item is null.");

                return false;
            }

            // ---------------------------------------------
            // ACTIVE LEVEL INVENTORY
            // ---------------------------------------------

            if (!allowedItems.Contains(item))
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .WrongComponent,
                    "Item is not available in active level: " +
                    item.ItemId);

                return false;
            }

            // ---------------------------------------------
            // ITEM STATUS
            // ---------------------------------------------

            if (!item.isActiveAndEnabled ||
                !item.gameObject.activeInHierarchy)
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .GenericPlacementFailure,
                    "Item is inactive: " + item.ItemId);

                return false;
            }

            if (item.IsMoving)
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .ComponentMoving,
                    "Item is still moving: " + item.ItemId);

                return false;
            }

            if (!item.AllowTouch)
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .GenericPlacementFailure,
                    "Touch disabled: " + item.ItemId);

                return false;
            }

            // ---------------------------------------------
            // CLICK SAME ITEM AGAIN
            // ---------------------------------------------

            if (item == selected)
            {
                CancelSelection();
                return true;
            }

            // ---------------------------------------------
            // PLACED COMPONENT HANDLING
            // ---------------------------------------------

            if (item.IsAtWorkbench)
            {
                if (clickPlacedItemToReturn)
                {
                    return TryReturnItem(item);
                }

                // A placed item must be returned by dragging.
                // Do not select it for another workstation
                // placement using the single-click workflow.

                if (logSelection)
                {
                    Debug.Log(
                        "[ManualShelf] Item is already " +
                        "on the workstation. " +
                        "Drag it back to storage: " +
                        item.ItemId,
                        item);
                }

                return false;
            }

            // ---------------------------------------------
            // ELECTRICAL SAFETY
            // ---------------------------------------------

            if (!shelf.CanMoveItemSafely(item))
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .WiredComponent,
                    "Selected component cannot move safely: " +
                    item.ItemId);

                return false;
            }

            // ---------------------------------------------
            // SELECT ITEM
            // ---------------------------------------------

            selected = item;

            item.NotifySelected();

            SelectionChanged?.Invoke(selected);

            if (logSelection)
            {
                Debug.Log(
                    "[ManualShelf] Selected: " +
                    item.ItemId,
                    item);
            }

            return true;
        }

        // =====================================================
        // WORKSTATION PLACEMENT
        // =====================================================

        public bool TryPlaceAt(SparkShelfSlot slot)
        {
            ResolveReferences();

            SparkShelfItem item = selected;

            // ---------------------------------------------
            // DIAGNOSTIC SNAPSHOT
            // ---------------------------------------------

            if (logPlacementDetails)
            {
                Debug.Log(
                    "[Placement DEBUG] " +
                    "Item=" +
                    (item != null
                        ? item.ItemId
                        : "NULL") +
                    " | Slot=" +
                    (slot != null
                        ? slot.SlotId
                        : "NULL") +
                    " | AllowedSlot=" +
                    (slot != null &&
                     allowedSlots.Contains(slot)) +
                    " | AllowedItem=" +
                    (item != null &&
                     allowedItems.Contains(item)) +
                    " | SlotKind=" +
                    (slot != null
                        ? slot.Kind.ToString()
                        : "NULL") +
                    " | CanAccept=" +
                    (slot != null &&
                     item != null &&
                     slot.CanAccept(item)) +
                    " | IsMoving=" +
                    (item != null &&
                     item.IsMoving) +
                    " | AllowTouch=" +
                    (item != null &&
                     item.AllowTouch),
                    this);
            }

            // ---------------------------------------------
            // NOTHING SELECTED
            // ---------------------------------------------

            if (item == null)
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .NoSelection,
                    "No selected shelf item.");
            }

            // ---------------------------------------------
            // MISSING SHELF SYSTEM
            // ---------------------------------------------

            if (shelf == null)
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .GenericPlacementFailure,
                    "SparkObjectShelf is missing.");
            }

            // ---------------------------------------------
            // INVALID SLOT
            // ---------------------------------------------

            if (slot == null)
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .UnregisteredSlot,
                    "Destination slot is null.");
            }

            // ---------------------------------------------
            // ACTIVE LEVEL SLOT REGISTRATION
            // ---------------------------------------------

            if (!allowedSlots.Contains(slot))
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .UnregisteredSlot,
                    "Slot is not registered for active level: " +
                    slot.SlotId);
            }

            // ---------------------------------------------
            // SLOT ACTIVE
            // ---------------------------------------------

            if (!slot.isActiveAndEnabled ||
                !slot.gameObject.activeInHierarchy)
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .UnregisteredSlot,
                    "Workstation slot is inactive: " +
                    slot.SlotId);
            }

            // ---------------------------------------------
            // WORKSTATION KIND
            // ---------------------------------------------

            if (slot.Kind !=
                SparkShelfSlotKind.Workstation)
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .IncompatibleSlot,
                    "Destination is not a workstation slot.");
            }

            // ---------------------------------------------
            // ITEM BELONGS TO ACTIVE LEVEL
            // ---------------------------------------------

            if (!allowedItems.Contains(item))
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .WrongComponent,
                    "Selected item is unavailable in active level: " +
                    item.ItemId);
            }

            // ---------------------------------------------
            // ITEM ENABLED
            // ---------------------------------------------

            if (!item.isActiveAndEnabled ||
                !item.gameObject.activeInHierarchy ||
                !item.AllowTouch)
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .GenericPlacementFailure,
                    "Selected item is inactive or touch-disabled.");
            }

            // ---------------------------------------------
            // ITEM MOVING
            // ---------------------------------------------

            if (item.IsMoving)
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .ComponentMoving,
                    "Selected component is already moving.");
            }

            // ---------------------------------------------
            // SLOT OCCUPANCY
            // ---------------------------------------------

            if (slot.Occupant != null &&
                slot.Occupant != item)
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .OccupiedSlot,
                    "Slot " + slot.SlotId +
                    " contains " +
                    slot.Occupant.ItemId);
            }

            // ---------------------------------------------
            // CATEGORY COMPATIBILITY
            // ---------------------------------------------

            if (!slot.CanAccept(item))
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .IncompatibleSlot,
                    "Item category=" +
                    item.Category +
                    " | Slot category=" +
                    slot.Category);
            }

            // ---------------------------------------------
            // ELECTRICAL SAFETY
            // ---------------------------------------------

            if (!shelf.CanMoveItemSafely(item))
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .WiredComponent,
                    "Item cannot move safely. " +
                    "Check connected wires.");
            }

            // ---------------------------------------------
            // PERFORM MOVEMENT
            // ---------------------------------------------

            if (!shelf.MoveToSlot(item, slot))
            {
                return RejectPlacement(
                    item,
                    slot,
                    SparkTutorialMistakeCoach.Mistake
                        .GenericPlacementFailure,
                    "SparkObjectShelf.MoveToSlot() returned false.");
            }

            // ---------------------------------------------
            // SUCCESS
            // ---------------------------------------------

            if (logPlacementSuccess)
            {
                Debug.Log(
                    "[ManualShelf] PLACEMENT ACCEPTED: " +
                    item.ItemId +
                    " -> " +
                    slot.SlotId,
                    item);
            }

            ItemPlaced?.Invoke(item, slot);

            CancelSelection();

            return true;
        }

        // =====================================================
        // RETURN ITEM TO STORAGE
        // =====================================================

        /// <summary>
        /// Used by drag-and-drop to return a placed
        /// component to its storage location.
        /// </summary>
        public bool TryReturnItem(SparkShelfItem item)
        {
            ResolveReferences();

            if (shelf == null || item == null)
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .GenericPlacementFailure,
                    "Cannot return: shelf or item missing.");

                return false;
            }

            if (!allowedItems.Contains(item))
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .WrongComponent,
                    "Cannot return item outside active level: " +
                    item.ItemId);

                return false;
            }

            if (item.IsMoving)
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .ComponentMoving,
                    "Cannot return an item while moving.");

                return false;
            }

            if (!item.IsAtWorkbench)
            {
                return false;
            }

            if (!shelf.CanMoveItemSafely(item))
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .WiredComponent,
                    "Disconnect wires before returning " +
                    item.ItemId);

                return false;
            }

            bool returned =
                shelf.TryReturnToShelf(item);

            if (!returned)
            {
                RejectSelection(
                    SparkTutorialMistakeCoach.Mistake
                        .GenericPlacementFailure,
                    "SparkObjectShelf.TryReturnToShelf failed: " +
                    item.ItemId);

                return false;
            }

            if (logSelection)
            {
                Debug.Log(
                    "[ManualShelf] RETURN ACCEPTED: " +
                    item.ItemId,
                    item);
            }

            CancelSelection();

            return true;
        }

        // =====================================================
        // RUNTIME WORKSTATION REGISTRATION
        // =====================================================

        /// <summary>
        /// Registers workstation slots instantiated
        /// while the level is running.
        /// </summary>
        public bool RegisterRuntimeSlot(
            SparkShelfSlot slot)
        {
            if (slot == null)
                return false;

            if (slot.Kind !=
                SparkShelfSlotKind.Workstation)
            {
                return false;
            }

            bool added =
                allowedSlots.Add(slot);

            if (added && logSelection)
            {
                Debug.Log(
                    "[ManualShelf] Registered runtime slot: " +
                    slot.SlotId,
                    slot);
            }

            return added;
        }

        /// <summary>
        /// Unregisters a runtime workstation slot.
        /// </summary>
        public void UnregisterRuntimeSlot(
            SparkShelfSlot slot)
        {
            if (slot == null)
                return;

            allowedSlots.Remove(slot);

            if (logSelection)
            {
                Debug.Log(
                    "[ManualShelf] Unregistered slot: " +
                    slot.SlotId,
                    slot);
            }
        }

        // =====================================================
        // AI CORRECTION REPORTING
        // =====================================================

        private bool RejectPlacement(
            SparkShelfItem item,
            SparkShelfSlot slot,
            SparkTutorialMistakeCoach.Mistake mistake,
            string reason)
        {
            if (logPlacementFailures)
            {
                Debug.LogWarning(
                    "[ManualShelf] PLACEMENT REJECTED: " +
                    reason +
                    " | Item=" +
                    (item != null
                        ? item.ItemId
                        : "NULL") +
                    " | Slot=" +
                    (slot != null
                        ? slot.SlotId
                        : "NULL"),
                    slot != null
                        ? (UnityEngine.Object)slot
                        : this);
            }

            ReportTutorialMistake(
                mistake,
                reason);

            PlacementRejected?.Invoke(
                item,
                slot);

            return false;
        }

        private void RejectSelection(
            SparkTutorialMistakeCoach.Mistake mistake,
            string reason)
        {
            if (logPlacementFailures)
            {
                Debug.LogWarning(
                    "[ManualShelf] SELECTION REJECTED: " +
                    reason,
                    this);
            }

            ReportTutorialMistake(
                mistake,
                reason);
        }

        private void ReportTutorialMistake(
            SparkTutorialMistakeCoach.Mistake mistake,
            string detail)
        {
            if (tutorialCoach == null)
            {
                tutorialCoach =
                    FindFirstObjectByType<
                        SparkTutorialMistakeCoach>();
            }

            if (tutorialCoach == null)
                return;

            tutorialCoach.Report(
                mistake,
                detail);
        }

        // =====================================================
        // SELECTION MANAGEMENT
        // =====================================================

        public void CancelSelection()
        {
            if (selected == null)
                return;

            if (logSelection)
            {
                Debug.Log(
                    "[ManualShelf] Selection cleared: " +
                    selected.ItemId,
                    this);
            }

            selected = null;

            SelectionChanged?.Invoke(null);
        }

        // =====================================================
        // DIAGNOSTICS
        // =====================================================

        [ContextMenu("Diagnostics / Print Selection")]
        private void PrintSelection()
        {
            Debug.Log(
                "[ManualShelf DEBUG]\n" +
                "Selected=" +
                (selected != null
                    ? selected.ItemId
                    : "NULL") +
                "\nAllowed Items=" +
                allowedItems.Count +
                "\nAllowed Workstation Slots=" +
                allowedSlots.Count +
                "\nShelf Assigned=" +
                (shelf != null) +
                "\nTutorial Coach Assigned=" +
                (tutorialCoach != null),
                this);
        }

        [ContextMenu("Diagnostics / Print Allowed Slots")]
        private void PrintAllowedSlots()
        {
            Debug.Log(
                "[ManualShelf] Allowed workstation slots: " +
                allowedSlots.Count,
                this);

            foreach (SparkShelfSlot slot in allowedSlots)
            {
                if (slot == null)
                    continue;

                Debug.Log(
                    "[ManualShelf SLOT] " +
                    slot.SlotId +
                    " | Kind=" +
                    slot.Kind +
                    " | Category=" +
                    slot.Category +
                    " | Occupant=" +
                    (slot.Occupant != null
                        ? slot.Occupant.ItemId
                        : "None"),
                    slot);
            }
        }

        private void OnDisable()
        {
            ClearLevel();
        }
    }
}

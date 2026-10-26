using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Gameplay.Shelf
{
    /// <summary>
    /// Two-click selection: select an available shelf item, then click an active
    /// level workstation slot. No input polling; SparkInteractionController owns input.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkManualShelfPlacement : MonoBehaviour
    {
        [SerializeField] private SparkObjectShelf shelf;
        [SerializeField] private bool clickPlacedItemToReturn = true;
        [SerializeField] private bool logSelection;

        private readonly HashSet<SparkShelfItem> allowedItems = new HashSet<SparkShelfItem>();
        private readonly HashSet<SparkShelfSlot> allowedSlots = new HashSet<SparkShelfSlot>();
        private SparkShelfItem selected;

        public SparkShelfItem SelectedItem => selected;
        public bool HasSelection => selected != null;
        public event Action<SparkShelfItem> SelectionChanged;

        private void Awake()
        {
            if (shelf == null) shelf = GetComponent<SparkObjectShelf>();
            if (shelf == null) shelf = FindFirstObjectByType<SparkObjectShelf>();
        }

        public void Configure(SparkShelfItem[] items, SparkShelfSlot[] workstationSlots)
        {
            CancelSelection();
            allowedItems.Clear();
            allowedSlots.Clear();
            if (items != null)
                foreach (SparkShelfItem item in items)
                    if (item != null) allowedItems.Add(item);
            if (workstationSlots != null)
                foreach (SparkShelfSlot slot in workstationSlots)
                    if (slot != null && slot.Kind == SparkShelfSlotKind.Workstation)
                        allowedSlots.Add(slot);
        }

        public void ClearLevel()
        {
            CancelSelection();
            allowedItems.Clear();
            allowedSlots.Clear();
        }

        public bool SelectItem(SparkShelfItem item)
        {
            if (shelf == null || item == null || !allowedItems.Contains(item) ||
                !item.isActiveAndEnabled || !item.AllowTouch || item.IsMoving)
                return false;

            if (item == selected)
            {
                CancelSelection();
                return true;
            }

            // Returning a placed item is an explicit click on that item.
            if (item.IsAtWorkbench && clickPlacedItemToReturn)
            {
                CancelSelection();
                return shelf.TryReturnToShelf(item);
            }

            if (!shelf.CanMoveItemSafely(item)) return false;
            selected = item;
            item.NotifySelected();
            SelectionChanged?.Invoke(selected);
            if (logSelection) Debug.Log("[ManualShelf] Selected " + item.ItemId, item);
            return true;
        }

       
public bool TryPlaceAt(SparkShelfSlot slot)
{
    Debug.Log(
    "[Placement DEBUG] " +
    "Item=" + (selected != null ? selected.ItemId : "NULL") +
    " | Slot=" + (slot != null ? slot.SlotId : "NULL") +
    " | AllowedSlot=" + (slot != null && allowedSlots.Contains(slot)) +
    " | AllowedItem=" + (selected != null && allowedItems.Contains(selected)) +
    " | SlotKind=" + (slot != null ? slot.Kind.ToString() : "NULL") +
    " | CanAccept=" + (slot != null && slot.CanAccept(selected)) +
    " | IsMoving=" + (selected != null && selected.IsMoving) +
    " | AllowTouch=" + (selected != null && selected.AllowTouch)
);
    if (selected == null)
    {
        Debug.LogWarning("[ManualShelf] REJECTED: No selected item.");
        return false;
    }

    if (shelf == null)
    {
        Debug.LogWarning("[ManualShelf] REJECTED: SparkObjectShelf missing.");
        return false;
    }

    if (slot == null)
    {
        Debug.LogWarning("[ManualShelf] REJECTED: Slot is null.");
        return false;
    }

    if (!allowedSlots.Contains(slot))
    {
        Debug.LogWarning(
            "[ManualShelf] REJECTED: Slot '" + slot.SlotId +
            "' is NOT registered in the active level's Workstation Slots.",
            slot);
        return false;
    }

    if (!slot.isActiveAndEnabled ||
        !slot.gameObject.activeInHierarchy)
    {
        Debug.LogWarning(
            "[ManualShelf] REJECTED: Workstation slot is inactive.",
            slot);
        return false;
    }

    if (slot.Kind != SparkShelfSlotKind.Workstation)
    {
        Debug.LogWarning(
            "[ManualShelf] REJECTED: Slot Kind is " + slot.Kind,
            slot);
        return false;
    }

    SparkShelfItem item = selected;

    if (!allowedItems.Contains(item))
    {
        Debug.LogWarning(
            "[ManualShelf] REJECTED: Selected item is not " +
            "available in the active level.",
            item);
        return false;
    }

    if (!item.AllowTouch || item.IsMoving)
    {
        Debug.LogWarning(
            "[ManualShelf] REJECTED: Item is disabled or moving.",
            item);
        return false;
    }

    if (!slot.CanAccept(item))
    {
        Debug.LogWarning(
            "[ManualShelf] REJECTED: Slot cannot accept item. " +
            "Item category = " + item.Category +
            ", Slot category = " + slot.Category +
            ", Occupant = " +
            (slot.Occupant != null ? slot.Occupant.ItemId : "None"),
            slot);
        return false;
    }

    if (!shelf.CanMoveItemSafely(item))
    {
        Debug.LogWarning(
            "[ManualShelf] REJECTED: Item cannot move safely. " +
            "Check electrical connections.",
            item);
        return false;
    }

    if (!shelf.MoveToSlot(item, slot))
    {
        Debug.LogWarning(
            "[ManualShelf] REJECTED: SparkObjectShelf.MoveToSlot() " +
            "returned false. Check slot registration, reservation, " +
            "and movement restrictions.",
            slot);
        return false;
    }

    Debug.Log(
        "[ManualShelf] PLACEMENT ACCEPTED: " +
        item.ItemId + " -> " + slot.SlotId,
        item);

    CancelSelection();
    return true;
}
        /// <summary>
        /// Registers a workstation slot created at runtime.
        /// </summary>
        public bool RegisterRuntimeSlot(SparkShelfSlot slot)
        {
            if (slot == null ||
                slot.Kind != SparkShelfSlotKind.Workstation)
                return false;

            return allowedSlots.Add(slot);
        }

        /// <summary>
        /// Removes a runtime workstation slot.
        /// </summary>
        public void UnregisterRuntimeSlot(SparkShelfSlot slot)
        {
            if (slot != null)
                allowedSlots.Remove(slot);
        }

        public void CancelSelection()
        {
            if (selected == null) return;
            selected = null;
            SelectionChanged?.Invoke(null);
        }

        private void OnDisable() => ClearLevel();
    }
}

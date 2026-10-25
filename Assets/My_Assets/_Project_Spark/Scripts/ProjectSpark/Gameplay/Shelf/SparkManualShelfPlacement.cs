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
            if (selected == null || shelf == null || slot == null ||
                !allowedSlots.Contains(slot) || !slot.isActiveAndEnabled ||
                !slot.gameObject.activeInHierarchy ||
                slot.Kind != SparkShelfSlotKind.Workstation)
                return false;

            SparkShelfItem item = selected;
            if (!allowedItems.Contains(item) || !item.AllowTouch || item.IsMoving ||
                !slot.CanAccept(item)) return false;
            if (!shelf.MoveToSlot(item, slot)) return false;
            CancelSelection();
            return true;
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

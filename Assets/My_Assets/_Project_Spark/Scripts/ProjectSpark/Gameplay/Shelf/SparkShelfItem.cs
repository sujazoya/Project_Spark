
using System;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectSpark.Gameplay.Shelf
{
    /// <summary>
    /// Represents a reusable electrical component managed by SparkObjectShelf.
    ///
    /// Supports:
    /// - Permanent preferred storage slot
    /// - Level-specific manual workstation placement
    /// - Selection and movement state
    /// - Level-controlled availability
    /// - Placement events for SelfComponent
    ///
    /// Does not handle input, movement, or electrical simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkShelfItem : MonoBehaviour
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        [Header("Item Identity")]
        [SerializeField] private string itemId;

        [SerializeField]
        private SparkShelfCategory category = SparkShelfCategory.Other;

        // ============================================================
        // SLOTS
        // ============================================================

        [Header("Shelf Slots")]

        [Tooltip("Permanent storage slot for this component.")]
        [SerializeField]
        private SparkShelfSlot preferredStorageSlot;

        [Tooltip(
            "Optional fallback workstation slot. " +
            "Leave empty when using level-specific manual placement.")]
        [SerializeField]
        private SparkShelfSlot preferredWorkstationSlot;

        // ============================================================
        // INTERACTION
        // ============================================================

        [Header("Interaction")]

        [Tooltip("Collider used to select this shelf component.")]
        [SerializeField]
        private Collider interactionCollider;

        [Tooltip("Whether this component is available for shelf selection.")]
        [SerializeField]
        private bool allowTouch = true;

        // ============================================================
        // EVENTS
        // ============================================================

        [Header("Events")]
        [SerializeField] private UnityEvent onSelected;
        [SerializeField] private UnityEvent onPlacedOnWorkbench;
        [SerializeField] private UnityEvent onReturnedToShelf;

        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        public string ItemId =>
            string.IsNullOrWhiteSpace(itemId) ? name : itemId;

        public SparkShelfCategory Category => category;

        public SparkShelfSlot PreferredStorageSlot =>
            preferredStorageSlot;

        public SparkShelfSlot PreferredWorkstationSlot =>
            preferredWorkstationSlot;

        public SparkShelfSlot CurrentSlot { get; internal set; }

        public bool AllowTouch => allowTouch;

        public bool IsMoving { get; internal set; }

        public bool IsAtWorkbench =>
            CurrentSlot != null &&
            CurrentSlot.Kind == SparkShelfSlotKind.Workstation;

        public bool IsInStorage =>
            CurrentSlot != null &&
            CurrentSlot.Kind == SparkShelfSlotKind.Storage;

        public bool CanBeSelected =>
            isActiveAndEnabled &&
            gameObject.activeInHierarchy &&
            allowTouch &&
            !IsMoving;

        public Collider InteractionCollider => interactionCollider;

        // ============================================================
        // C# EVENTS
        // ============================================================

        public event Action<SparkShelfItem> Selected;

        public event Action<SparkShelfItem, SparkShelfSlot> Placed;

        // ============================================================
        // AVAILABILITY
        // ============================================================

        /// <summary>
        /// Enables or disables selection of this shelf item.
        ///
        /// Does not disable the GameObject, collider,
        /// electrical terminals, or circuit simulation.
        /// </summary>
        public void SetTouchEnabled(bool enabled)
        {
            allowTouch = enabled;
        }

        // ============================================================
        // NOTIFICATIONS
        // ============================================================

        /// <summary>
        /// Called by the manual placement controller when
        /// this component is selected.
        /// </summary>
        internal void NotifySelected()
        {
            onSelected?.Invoke();
            Selected?.Invoke(this);
        }

        /// <summary>
        /// Called by SparkObjectShelf after movement completes.
        /// </summary>
        internal void NotifyPlaced(SparkShelfSlot slot)
        {
            if (slot == null)
                return;

            if (slot.Kind == SparkShelfSlotKind.Workstation)
            {
                onPlacedOnWorkbench?.Invoke();
            }
            else if (slot.Kind == SparkShelfSlotKind.Storage)
            {
                onReturnedToShelf?.Invoke();
            }

            Placed?.Invoke(this, slot);
        }

        // ============================================================
        // UNITY
        // ============================================================

        private void Reset()
        {
            if (interactionCollider == null)
            {
                interactionCollider =
                    GetComponentInChildren<Collider>();
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                itemId = name;
            }
        }
    }
}

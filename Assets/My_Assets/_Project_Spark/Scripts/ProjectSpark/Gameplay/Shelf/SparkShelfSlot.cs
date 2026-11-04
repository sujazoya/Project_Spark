using System;
using UnityEngine;
using TMPro;

namespace ProjectSpark.Gameplay.Shelf
{
    public enum SparkShelfCategory
    {
        Any,
        PowerSource,
        Switch,
        Resistor,
        LED,
        Capacitor,
        Diode,
        Sensor,
        Motor,
        Instrument,
        Wire,
        Other
    }

    public enum SparkShelfSlotKind
    {
        Storage,
        Workstation
    }

    /// <summary>
    /// Storage/workstation slot with stable, inspector-configured runtime binding IDs.
    ///
    /// The slot owns *IDs and occupancy*, not electrical binding implementation.
    /// A bridge subscribes to BindingRefreshRequested and uses Occupant to update
    /// SparkLevelSceneBindings. Reservation is not considered completed placement.
    ///
    /// Compatible with the existing SparkObjectShelf and SparkShelfItem APIs.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkShelfSlot : MonoBehaviour
    {
        // ============================================================
        // SLOT IDENTITY
        // ============================================================

        [Header("Slot Identity")]
        [SerializeField] private string slotId;
        [SerializeField] private SparkShelfCategory category = SparkShelfCategory.Any;
        [SerializeField] private SparkShelfSlotKind kind = SparkShelfSlotKind.Storage;

        // ============================================================
        // PLACEMENT
        // ============================================================

        [Header("Placement")]
        [SerializeField] private Transform placementAnchor;
        [SerializeField] private bool acceptAnyCategory;

        // Retained for backward compatibility with existing serialized prefabs.
        // Runtime Occupant is still established by shelf registration.
        [SerializeField] private bool occupiedAtStart;

        // ============================================================
        // SMART RUNTIME BINDINGS
        // ============================================================

        [Header("Smart Runtime Bindings (Workstation Only)")]
        [Tooltip("When enabled, this workstation provides stable IDs for the current component and two named terminals. A binding bridge must consume the refresh event.")]
        [SerializeField] private bool enableSmartBinding;

        [Tooltip("Logical ID used by SparkLevelSceneBindings, e.g. led-1 or R_LIMIT_Component.")]
        [SerializeField] private string runtimeComponentId;

        [Tooltip("Logical ID for terminal A, e.g. led-1_Anode or R_LIMIT_Input.")]
        [SerializeField] private string runtimeTerminalAId;

        [Tooltip("Logical ID for terminal B, e.g. led-1_Cathode or R_LIMIT_Output.")]
        [SerializeField] private string runtimeTerminalBId;

        [Header("Smart Binding Diagnostics")]
        [SerializeField] private bool logBindingRefresh;

        // ============================================================
        // CATEGORY LABEL
        // ============================================================

        [Header("Category Label")]
        [Tooltip("TextMeshPro label displaying the storage value or slot category.")]
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private bool autoUpdateLabel = true;
        [SerializeField] private bool uppercaseLabel = true;

        // ============================================================
        // RUNTIME
        // ============================================================

        [NonSerialized] private SparkShelfItem occupant;

        // ============================================================
        // PUBLIC API: KEEP COMPATIBLE
        // ============================================================

        public string SlotId => string.IsNullOrWhiteSpace(slotId) ? name : slotId;
        public SparkShelfCategory Category => category;
        public SparkShelfSlotKind Kind => kind;
        public Transform Anchor => placementAnchor != null ? placementAnchor : transform;
        public SparkShelfItem Occupant => occupant;
        public bool IsAvailable => occupant == null;
        public TMP_Text LabelText => labelText;

        public bool EnableSmartBinding => enableSmartBinding && kind == SparkShelfSlotKind.Workstation;
        public string RuntimeComponentId => runtimeComponentId;
        public string RuntimeTerminalAId => runtimeTerminalAId;
        public string RuntimeTerminalBId => runtimeTerminalBId;
        public bool OccupiedAtStart => occupiedAtStart;

        /// <summary>
        /// Fired when an installed item finishes placement or when a slot loses
        /// its occupant. Subscribe from the active level binding bridge.
        /// Not fired by TryReserve because that is an in-flight destination hold.
        /// </summary>
        public event Action<SparkShelfSlot> BindingRefreshRequested;

        public bool HasValidSmartBindingIds =>
            EnableSmartBinding &&
            !string.IsNullOrWhiteSpace(runtimeComponentId) &&
            !string.IsNullOrWhiteSpace(runtimeTerminalAId) &&
            !string.IsNullOrWhiteSpace(runtimeTerminalBId) &&
            runtimeComponentId != runtimeTerminalAId &&
            runtimeComponentId != runtimeTerminalBId &&
            runtimeTerminalAId != runtimeTerminalBId;

        /// <summary>
        /// The occupant has actually arrived when CurrentSlot points at this slot
        /// and IsMoving is false. A reserved item en route is not yet installed.
        /// </summary>
        public SparkShelfItem InstalledItem
        {
            get
            {
                if (occupant == null || occupant.IsMoving || occupant.CurrentSlot != this)
                    return null;
                return occupant;
            }
        }

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            RefreshCategoryLabel();
        }

        private void OnEnable()
        {
            RefreshCategoryLabel();
        }

        private void Reset()
        {
            if (placementAnchor == null)
                placementAnchor = transform;

            if (labelText == null)
                labelText = GetComponentInChildren<TMP_Text>(true);

            RefreshCategoryLabel();
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(slotId))
                slotId = name;

            if (kind == SparkShelfSlotKind.Workstation && enableSmartBinding &&
                (!string.IsNullOrWhiteSpace(runtimeComponentId) ||
                 !string.IsNullOrWhiteSpace(runtimeTerminalAId) ||
                 !string.IsNullOrWhiteSpace(runtimeTerminalBId)) &&
                !HasValidSmartBindingIds)
            {
                // Do not edit IDs automatically. The user may still be entering them.
                // A context-menu validator below reports complete diagnostics.
            }

            RefreshCategoryLabel();
        }

        // ============================================================
        // LABEL
        // ============================================================

        public void RefreshCategoryLabel()
        {
            if (!autoUpdateLabel || labelText == null)
                return;

            string displayText;

            if (kind == SparkShelfSlotKind.Storage)
            {
                SparkShelfItem item = occupant;

                if (item == null)
                {
                    // Keep the original editor/startup behavior: an empty storage
                    // slot can display its preferred item's value before registration.
                    SparkShelfItem[] allItems = FindObjectsByType<SparkShelfItem>(
                        FindObjectsInactive.Include, FindObjectsSortMode.None);

                    foreach (SparkShelfItem candidate in allItems)
                    {
                        if (candidate != null && candidate.PreferredStorageSlot == this)
                        {
                            item = candidate;
                            break;
                        }
                    }
                }

                displayText = item != null ? item.StorageLabel : GetCategoryDisplayName();
            }
            else
            {
                displayText = GetCategoryDisplayName();
            }

            if (uppercaseLabel)
                displayText = displayText.ToUpperInvariant();

            labelText.text = displayText;
        }

        private string GetCategoryDisplayName()
        {
            switch (category)
            {
                case SparkShelfCategory.PowerSource: return "Power Source";
                case SparkShelfCategory.LED: return "LED";
                default: return category.ToString();
            }
        }

        public void SetCategory(SparkShelfCategory newCategory)
        {
            category = newCategory;
            RefreshCategoryLabel();
        }

        public void SetLabelText(TMP_Text textComponent)
        {
            labelText = textComponent;
            RefreshCategoryLabel();
        }

        // ============================================================
        // COMPATIBILITY + RESERVATION
        // ============================================================

        public bool CanAccept(SparkShelfItem item)
        {
            if (item == null)
                return false;

            bool categoryMatches = acceptAnyCategory ||
                                   category == SparkShelfCategory.Any ||
                                   category == item.Category;

            bool slotAvailable = occupant == null || occupant == item;
            return categoryMatches && slotAvailable;
        }

        /// <summary>
        /// Reserves a destination for a moving item. This does NOT bind the item:
        /// placement has not finished and CurrentSlot still refers to the old slot.
        /// </summary>
        public bool TryReserve(SparkShelfItem item)
        {
            if (!CanAccept(item))
                return false;

            occupant = item;
            RefreshCategoryLabel();
            return true;
        }

        /// <summary>
        /// Removes this reservation/occupant. Consumers must re-resolve bindings.
        /// The binding bridge should read InstalledItem, not Occupant, to avoid
        /// accidentally binding an item still moving to its destination.
        /// </summary>
        public void Release(SparkShelfItem item)
        {
            if (occupant != item)
                return;

            occupant = null;
            RefreshCategoryLabel();
            RefreshSmartBinding();
        }

        // ============================================================
        // SMART BINDING REFRESH
        // ============================================================

        /// <summary>
        /// Requests a binding refresh from the active bridge. This method does
        /// not edit SparkLevelSceneBindings by itself. Invoke after completed
        /// placement (CurrentSlot updated) and after startup bridge subscription.
        /// </summary>
        public void RefreshSmartBinding()
        {
            if (!EnableSmartBinding)
                return;

            if (logBindingRefresh)
            {
                SparkShelfItem installed = InstalledItem;
                Debug.Log(
                    $"[SmartSlot] Refresh {SlotId} | ComponentID={runtimeComponentId} | " +
                    $"Installed={(installed != null ? installed.ItemId : "NONE")}", this);
            }

            BindingRefreshRequested?.Invoke(this);
        }

        [ContextMenu("Smart Binding / Validate IDs")]
        private void ValidateSmartBindingIds()
        {
            if (!EnableSmartBinding)
            {
                Debug.Log($"[SmartSlot] {SlotId}: smart binding disabled (or storage slot).", this);
                return;
            }

            if (!HasValidSmartBindingIds)
            {
                Debug.LogWarning(
                    $"[SmartSlot] {SlotId}: IDs must be nonempty and unique. " +
                    $"Component='{runtimeComponentId}', A='{runtimeTerminalAId}', B='{runtimeTerminalBId}'.", this);
                return;
            }

            Debug.Log(
                $"[SmartSlot] {SlotId}: valid IDs. " +
                $"Component='{runtimeComponentId}', A='{runtimeTerminalAId}', B='{runtimeTerminalBId}'. " +
                $"Installed={(InstalledItem != null ? InstalledItem.ItemId : "NONE")}", this);
        }

        [ContextMenu("Smart Binding / Request Refresh")]
        private void RequestRefreshFromInspector()
        {
            RefreshSmartBinding();
        }
    }
}

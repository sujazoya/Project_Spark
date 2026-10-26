
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

    [DisallowMultipleComponent]
    public sealed class SparkShelfSlot : MonoBehaviour
    {
        // ============================================================
        // SLOT IDENTITY
        // ============================================================

        [Header("Slot Identity")]

        [SerializeField]
        private string slotId;

        [SerializeField]
        private SparkShelfCategory category =
            SparkShelfCategory.Any;

        [SerializeField]
        private SparkShelfSlotKind kind =
            SparkShelfSlotKind.Storage;

        // ============================================================
        // PLACEMENT
        // ============================================================

        [Header("Placement")]

        [SerializeField]
        private Transform placementAnchor;

        [SerializeField]
        private bool acceptAnyCategory;

        [SerializeField]
        private bool occupiedAtStart;

        // ============================================================
        // CATEGORY LABEL
        // ============================================================

        [Header("Category Label")]

        [Tooltip(
            "TextMeshPro UI label displaying the slot category.")]
        [SerializeField]
        private TMP_Text labelText;

        [Tooltip(
            "Automatically update the label from the category.")]
        [SerializeField]
        private bool autoUpdateLabel = true;

        [Tooltip("Display the category in uppercase.")]
        [SerializeField]
        private bool uppercaseLabel = true;

        // ============================================================
        // RUNTIME
        // ============================================================

        private SparkShelfItem occupant;

        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        public string SlotId =>
            string.IsNullOrWhiteSpace(slotId)
                ? name
                : slotId;

        public SparkShelfCategory Category => category;

        public SparkShelfSlotKind Kind => kind;

        public Transform Anchor =>
            placementAnchor != null
                ? placementAnchor
                : transform;

        public SparkShelfItem Occupant => occupant;

        public bool IsAvailable => occupant == null;

        public TMP_Text LabelText => labelText;

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

            RefreshCategoryLabel();
        }

        // ============================================================
        // CATEGORY LABEL
        // ============================================================

        /// <summary>
        /// Updates the TextMeshPro label using the slot category.
        /// </summary>
       public void RefreshCategoryLabel()
{
    if (!autoUpdateLabel || labelText == null)
        return;

    string displayText;

    if (kind == SparkShelfSlotKind.Storage)
    {
        // Prefer the component currently assigned to this slot.
        SparkShelfItem item = occupant;

        if (item == null)
        {
            // Before runtime reservation, find the item whose
            // permanent storage slot is this slot.
            SparkShelfItem[] items =
                FindObjectsByType<SparkShelfItem>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            foreach (SparkShelfItem candidate in items)
            {
                if (candidate != null &&
                    candidate.PreferredStorageSlot == this)
                {
                    item = candidate;
                    break;
                }
            }
        }

        displayText = item != null
            ? item.StorageLabel
            : GetCategoryDisplayName();
    }
    else
    {
        // Workstation always displays category.
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
                case SparkShelfCategory.PowerSource:
                    return "Power Source";

                case SparkShelfCategory.LED:
                    return "LED";

                default:
                    return category.ToString();
            }
        }

        

        // ============================================================
        // SLOT COMPATIBILITY
        // ============================================================

        public bool CanAccept(SparkShelfItem item)
        {
            if (item == null)
                return false;

            bool categoryMatches =
                acceptAnyCategory ||
                category == SparkShelfCategory.Any ||
                category == item.Category;

            bool slotAvailable =
                occupant == null ||
                occupant == item;

            return categoryMatches && slotAvailable;
        }

        // ============================================================
        // SLOT RESERVATION
        // ============================================================

       public bool TryReserve(SparkShelfItem item)
{
    if (!CanAccept(item))
        return false;

    occupant = item;
    RefreshCategoryLabel();
    return true;
}

       public void Release(SparkShelfItem item)
{
    if (occupant != item)
        return;

    occupant = null;
    RefreshCategoryLabel();
}

        // ============================================================
        // PUBLIC LABEL API
        // ============================================================

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
    }
}

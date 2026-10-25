using UnityEngine;

namespace ProjectSpark.Gameplay.Shelf
{
    public enum SparkShelfCategory { Any, PowerSource, Switch, Resistor, LED, Capacitor, Diode, Sensor, Motor, Instrument, Wire, Other }
    public enum SparkShelfSlotKind { Storage, Workstation }

    [DisallowMultipleComponent]
    public sealed class SparkShelfSlot : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private SparkShelfCategory category = SparkShelfCategory.Any;
        [SerializeField] private SparkShelfSlotKind kind = SparkShelfSlotKind.Storage;
        [SerializeField] private Transform placementAnchor;
        [SerializeField] private bool acceptAnyCategory;
        [SerializeField] private bool occupiedAtStart;
        private SparkShelfItem occupant;

        public string SlotId => string.IsNullOrWhiteSpace(slotId) ? name : slotId;
        public SparkShelfCategory Category => category;
        public SparkShelfSlotKind Kind => kind;
        public Transform Anchor => placementAnchor != null ? placementAnchor : transform;
        public SparkShelfItem Occupant => occupant;
        public bool IsAvailable => occupant == null;
        public bool CanAccept(SparkShelfItem item) => item != null && (acceptAnyCategory || category == SparkShelfCategory.Any || category == item.Category) && (occupant == null || occupant == item);

        public bool TryReserve(SparkShelfItem item)
        {
            if (!CanAccept(item)) return false;
            occupant = item;
            return true;
        }
        public void Release(SparkShelfItem item)
        {
            if (occupant == item) occupant = null;
        }
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(slotId)) slotId = name;
        }
    }
}

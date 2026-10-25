
using UnityEngine;

namespace ProjectSpark.Gameplay.Shelf
{
    /// <summary>
    /// Connects a clickable workstation collider
    /// to its SparkShelfSlot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkShelfSlotClickTarget : MonoBehaviour
    {
        [SerializeField]
        private SparkShelfSlot slot;

        public SparkShelfSlot Slot => slot;

        private void Reset()
        {
            slot = GetComponent<SparkShelfSlot>();

            if (slot == null)
                slot = GetComponentInParent<SparkShelfSlot>();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay.Shelf
{
    /// <summary>
    /// Shelf storage/workstation controller with safe runtime registration
    /// and unregistration for purchased and recycled components.
    ///
    /// Electrical topology is owned by SparkCircuitSystem.
    /// Items must be disconnected before moving or unregistering.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkObjectShelf : MonoBehaviour
    {
        [Header("Registration")]
        [SerializeField] private SparkShelfSlot[] slots;
        [SerializeField] private SparkShelfItem[] items;
        [SerializeField] private bool autoFindChildren = true;

        [SerializeField]
        private SparkShelfSlot[] additionalStorageSlots;

        [Header("Placement")]
        [SerializeField, Min(0f)] private float travelSeconds = .35f;
        [SerializeField] private AnimationCurve motion = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool parentToSlot = true;

        [SerializeField] private bool disablePhysicsWhileStored = true;
        [SerializeField] private bool logTransitions;

        [Header("Circuit Safety")]
        [SerializeField] private SparkCircuitSystem circuitSystem;
        [SerializeField] private bool preventMovementWhileConnected = true;

        [Header("Manual Placement")]
        [SerializeField] private SparkManualShelfPlacement manualPlacement;

        private readonly List<SparkCircuitConnection> connectionBuffer =
            new List<SparkCircuitConnection>(8);
        private readonly Dictionary<SparkShelfItem, Coroutine> motions =
            new Dictionary<SparkShelfItem, Coroutine>();
        private readonly Dictionary<SparkShelfItem, SparkShelfSlot> destinations =
            new Dictionary<SparkShelfItem, SparkShelfSlot>();
        private readonly Dictionary<SparkShelfItem, Transform> originalParents =
            new Dictionary<SparkShelfItem, Transform>();
        private readonly Dictionary<SparkShelfItem, RigidbodyState> physics =
            new Dictionary<SparkShelfItem, RigidbodyState>();

        private bool initialized;

        private struct RigidbodyState
        {
            public Rigidbody body;
            public bool wasKinematic;
            public bool usedGravity;
        }

        public Func<SparkShelfItem, bool> CanMoveItem;
        public event Action<SparkShelfItem, SparkShelfSlot> ItemPlaced;

        public IReadOnlyList<SparkShelfItem> RegisteredItems => items;
        public IReadOnlyList<SparkShelfSlot> RegisteredSlots => slots;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;

            if (autoFindChildren)
            {
                if (slots == null || slots.Length == 0)
                    slots = GetComponentsInChildren<SparkShelfSlot>(true);
                if (items == null || items.Length == 0)
                    items = GetComponentsInChildren<SparkShelfItem>(true);
            }

            if (items == null) items = Array.Empty<SparkShelfItem>();
            if (slots == null) slots = Array.Empty<SparkShelfSlot>();
            RegisterAdditionalStorageSlots();

            // Snapshot because Snap() can reparent objects during initialization.
            SparkShelfItem[] initialItems = (SparkShelfItem[])items.Clone();
            foreach (SparkShelfItem item in initialItems)
            {
                if (item == null) continue;
                CacheItemState(item);

                // Respect existing scene placement when slot and occupant agree.
                SparkShelfSlot current = item.CurrentSlot;
                if (current != null && current.Occupant == item)
                    continue;

                SparkShelfSlot start = FindStorageSlot(item);
                if (start != null && start.TryReserve(item))
                {
                    item.CurrentSlot = start;
                    Snap(item, start);
                }
                else
                {
                    Debug.LogWarning(
                        "[SparkShelf] No free starting storage slot for " + item.ItemId,
                        item);
                }
            }
        }

        private void CacheItemState(SparkShelfItem item)
        {
            if (item == null) return;
            if (!originalParents.ContainsKey(item))
                originalParents.Add(item, item.transform.parent);

            if (!physics.ContainsKey(item))
            {
                Rigidbody rb = item.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    physics.Add(item, new RigidbodyState
                    {
                        body = rb,
                        wasKinematic = rb.isKinematic,
                        usedGravity = rb.useGravity
                    });
                }
            }
        }

        private bool IsRegistered(SparkShelfItem item)
        {
            return item != null && Array.IndexOf(items, item) >= 0;
        }

        private void ResolveCircuitSystem()
        {
            if (circuitSystem != null) return;
            circuitSystem = GetComponentInParent<SparkCircuitSystem>();
            if (circuitSystem == null)
                circuitSystem = FindFirstObjectByType<SparkCircuitSystem>();
        }

        /// <summary>
        /// True only when none of this item's terminals has a valid
        /// registered connection. Refuses electrical items when the
        /// circuit manager is unavailable (fail-safe).
        /// </summary>
        private bool IsElectricallyDisconnected(SparkShelfItem item)
        {
            if (item == null) return false;

            SparkTerminal[] terminals =
                item.GetComponentsInChildren<SparkTerminal>(true);
            if (terminals.Length == 0) return true;

            ResolveCircuitSystem();
            if (circuitSystem == null)
            {
                Debug.LogWarning(
                    "[SparkShelf] Circuit system missing: cannot verify connections for " +
                    item.ItemId, item);
                return false;
            }

            foreach (SparkTerminal terminal in terminals)
            {
                if (terminal == null) continue;
                connectionBuffer.Clear();
                circuitSystem.GetConnections(terminal, connectionBuffer);
                foreach (SparkCircuitConnection connection in connectionBuffer)
                {
                    if (connection != null && connection.IsValid)
                    {
                        Debug.LogWarning(
                            "[SparkShelf] Disconnect wires before moving/removing " +
                            item.ItemId, item);
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CanMoveSafely(SparkShelfItem item)
        {
            if (!IsRegistered(item) || !item.isActiveAndEnabled || item.IsMoving)
                return false;
            if (preventMovementWhileConnected && !IsElectricallyDisconnected(item))
                return false;
            return CanMoveItem == null || CanMoveItem(item);
        }

        public bool CanMoveItemSafely(SparkShelfItem item)
        {
            Initialize();
            return CanMoveSafely(item);
        }

        public bool Touch(SparkShelfItem item)
        {
            Initialize();
            if (!IsRegistered(item) || item == null || !item.AllowTouch || item.IsMoving)
                return false;

            if (manualPlacement != null && manualPlacement.isActiveAndEnabled)
                return manualPlacement.SelectItem(item);

            SparkShelfSlotKind kind = item.IsAtWorkbench
                ? SparkShelfSlotKind.Storage
                : SparkShelfSlotKind.Workstation;
            bool moved = MoveToCategorySlot(item, kind);
            if (moved) item.NotifySelected();
            return moved;
        }

        private SparkShelfSlot FindStorageSlot(SparkShelfItem item)
        {
            if (item == null) return null;
            SparkShelfSlot preferred = item.PreferredStorageSlot;
            if (preferred != null && preferred.isActiveAndEnabled &&
                preferred.Kind == SparkShelfSlotKind.Storage &&
                IsKnownSlot(preferred) && preferred.CanAccept(item))
                return preferred;

            foreach (SparkShelfSlot slot in slots)
            {
                if (slot != null && slot.isActiveAndEnabled &&
                    slot.Kind == SparkShelfSlotKind.Storage &&
                    slot.CanAccept(item))
                    return slot;
            }
            return null;
        }

        private bool IsKnownSlot(SparkShelfSlot slot)
        {
            return slot != null && Array.IndexOf(slots, slot) >= 0;
        }

        public bool MoveToCategorySlot(SparkShelfItem item, SparkShelfSlotKind kind)
        {
            Initialize();
            if (!CanMoveSafely(item)) return false;

            SparkShelfSlot preferred = kind == SparkShelfSlotKind.Storage
                ? item.PreferredStorageSlot
                : item.PreferredWorkstationSlot;

            if (preferred != null && IsKnownSlot(preferred) &&
                preferred.isActiveAndEnabled && preferred.Kind == kind &&
                preferred.CanAccept(item))
                return MoveToSlot(item, preferred);

            foreach (SparkShelfSlot slot in slots)
            {
                if (slot != null && slot.isActiveAndEnabled &&
                    slot.Kind == kind && slot.CanAccept(item))
                    return MoveToSlot(item, slot);
            }
            Debug.LogWarning("[SparkShelf] No free " + kind + " slot for " + item.ItemId, item);
            return false;
        }

        public bool MoveToSlot(SparkShelfItem item, SparkShelfSlot target)
        {
            Initialize();
            if (item == null || target == null)
{
    Debug.LogWarning("[Shelf DEBUG] Item or target is NULL", this);
    return false;
}

Debug.Log(
    $"[Shelf DEBUG] Item={item.name} | " +
    $"ItemRegistered={IsRegistered(item)} | " +
    $"Slot={target.name} | " +
    $"SlotRegistered={IsKnownSlot(target)} | " +
    $"SlotActive={target.isActiveAndEnabled} | " +
    $"Compatible={target.CanAccept(item)} | " +
    $"Occupant={(target.Occupant != null ? target.Occupant.name : "NONE")} | " +
    $"ItemActive={item.isActiveAndEnabled} | " +
    $"ItemMoving={item.IsMoving} | " +
    $"CanMoveSafely={CanMoveSafely(item)}",
    this
);
            if (target == null || !IsKnownSlot(target) ||
                !target.isActiveAndEnabled || !target.CanAccept(item) ||
                !CanMoveSafely(item))
                return false;

            if (item.CurrentSlot == target)
                return target.Occupant == item;

            if (!target.TryReserve(item)) return false;

            item.IsMoving = true;
            destinations[item] = target;
            motions[item] = StartCoroutine(AnimateMove(item, item.CurrentSlot, target));
            return true;
        }

        private IEnumerator AnimateMove(
            SparkShelfItem item, SparkShelfSlot previous, SparkShelfSlot target)
        {
            Transform tr = item.transform;
            Vector3 startPos = tr.position;
            Quaternion startRot = tr.rotation;
            Freeze(item);
            float elapsed = 0f;

            while (elapsed < travelSeconds)
            {
                elapsed += Time.deltaTime;
                float t = travelSeconds <= 0f ? 1f :
                    Mathf.Clamp01(elapsed / travelSeconds);
                float eased = motion == null ? t :
                    Mathf.Clamp01(motion.Evaluate(t));
                tr.SetPositionAndRotation(
                    Vector3.Lerp(startPos, target.Anchor.position, eased),
                    Quaternion.Slerp(startRot, target.Anchor.rotation, eased));
                yield return null;
            }

            Snap(item, target);
            previous?.Release(item);
            item.CurrentSlot = target;
            item.IsMoving = false;
            destinations.Remove(item);
            motions.Remove(item);
            item.NotifyPlaced(target);
            ItemPlaced?.Invoke(item, target);
            if (logTransitions)
                Debug.Log("[SparkShelf] " + item.ItemId + " -> " + target.SlotId, item);
        }

        private void Freeze(SparkShelfItem item)
        {
            if (!disablePhysicsWhileStored || item == null) return;
            if (!physics.TryGetValue(item, out RigidbodyState state) ||
                state.body == null) return;

            state.body.linearVelocity = Vector3.zero;
            state.body.angularVelocity = Vector3.zero;
            state.body.isKinematic = true;
            state.body.useGravity = false;
        }

        private void Snap(SparkShelfItem item, SparkShelfSlot slot)
        {
            if (item == null || slot == null) return;
            Freeze(item);
            item.transform.SetPositionAndRotation(
                slot.Anchor.position, slot.Anchor.rotation);

            if (parentToSlot)
                item.transform.SetParent(slot.Anchor, true);
            else if (originalParents.TryGetValue(item, out Transform parent))
                item.transform.SetParent(parent, true);
        }

        public bool TryReturnToShelf(SparkShelfItem item) =>
            MoveToCategorySlot(item, SparkShelfSlotKind.Storage);

        public bool TryPlaceOnWorkbench(SparkShelfItem item) =>
            MoveToCategorySlot(item, SparkShelfSlotKind.Workstation);

        // ============================================================
        // PURCHASED ITEM REGISTRATION
        // ============================================================

        /// <summary>
        /// Registers an already instantiated purchased component and
        /// assigns it a free compatible storage slot.
        /// No inventory is consumed here; the economy service owns that.
        /// If registration fails, caller still owns the GameObject.
        /// </summary>
        public bool TryRegisterPurchasedItem(SparkShelfItem item)
        {
            Initialize();
            if (item == null || !item.isActiveAndEnabled || item.IsMoving)
                return false;

            if (IsRegistered(item))
                return item.CurrentSlot != null &&
                       item.CurrentSlot.Occupant == item;

            // Reject items already reserved by any of this shelf's slots.
            foreach (SparkShelfSlot slot in slots)
            {
                if (slot != null && slot.Occupant == item)
                    return false;
            }

            // The prefab should enter the shelf disconnected.
            if (!IsElectricallyDisconnected(item)) return false;

            SparkShelfSlot storage = FindStorageSlot(item);
            if (storage == null || !storage.TryReserve(item))
            {
                Debug.LogWarning(
                    "[SparkShelf] No free compatible storage slot for purchase: " +
                    item.ItemId, item);
                return false;
            }

            CacheItemState(item);
            int count = items.Length;
            Array.Resize(ref items, count + 1);
            items[count] = item;

            item.CurrentSlot = storage;
            item.IsMoving = false;
            Snap(item, storage);
            item.NotifyPlaced(storage);
            ItemPlaced?.Invoke(item, storage);

            if (logTransitions)
                Debug.Log("[SparkShelf] Registered purchased item: " + item.ItemId, item);
            return true;
        }

        // ============================================================
        // SAFE ITEM UNREGISTRATION / RECYCLING
        // ============================================================

        /// <summary>
        /// Checks whether a registered item may be removed from the
        /// shelf's bookkeeping. Always demands electrical disconnection,
        /// even if ordinary shelf movement permits connected items.
        /// Does NOT require a burned resistor; the recycle bin checks that.
        /// </summary>
        public bool CanUnregisterItem(SparkShelfItem item)
        {
            Initialize();
            if (!IsRegistered(item) || item.IsMoving || motions.ContainsKey(item) ||
                destinations.ContainsKey(item))
                return false;

            SparkShelfSlot current = item.CurrentSlot;
            if (current != null)
            {
                if (current.Kind != SparkShelfSlotKind.Storage ||
                    current.Occupant != item)
                    return false;
            }

            return IsElectricallyDisconnected(item);
        }

        /// <summary>
        /// Releases the storage slot and removes runtime bookkeeping.
        /// Does NOT destroy the object or award currency.
        /// The recycle bin should call this before Destroy().
        /// </summary>
        public bool TryUnregisterItem(SparkShelfItem item)
        {
            if (!CanUnregisterItem(item)) return false;

            SparkShelfSlot previous = item.CurrentSlot;
            previous?.Release(item);
            item.CurrentSlot = null;

            List<SparkShelfItem> updated = new List<SparkShelfItem>(items);
            updated.Remove(item);
            items = updated.ToArray();

            motions.Remove(item);
            destinations.Remove(item);
            physics.Remove(item);
            originalParents.Remove(item);

            // Item can now be destroyed by the recycle bin.
            if (logTransitions)
                Debug.Log("[SparkShelf] Unregistered item: " + item.ItemId, item);
            return true;
        }

        private void OnDisable()
        {
            foreach (KeyValuePair<SparkShelfItem, Coroutine> pair in motions)
                if (pair.Value != null) StopCoroutine(pair.Value);
            motions.Clear();

            foreach (KeyValuePair<SparkShelfItem, SparkShelfSlot> pair in destinations)
            {
                SparkShelfItem item = pair.Key;
                SparkShelfSlot destination = pair.Value;
                if (item == null) continue;
                if (destination != null) destination.Release(item);
                if (item.CurrentSlot != null) Snap(item, item.CurrentSlot);
                item.IsMoving = false;
            }
            destinations.Clear();
        }

                private void RegisterAdditionalStorageSlots()
        {
            if (additionalStorageSlots == null ||
                additionalStorageSlots.Length == 0)
                return;

            List<SparkShelfSlot> registered =
                new List<SparkShelfSlot>(slots);

            foreach (SparkShelfSlot slot in additionalStorageSlots)
            {
                if (slot == null ||
                    slot.Kind != SparkShelfSlotKind.Storage ||
                    registered.Contains(slot))
                    continue;

                registered.Add(slot);
            }

            slots = registered.ToArray();
        }

       public void RegisterRuntimeSlots(Transform levelRoot)
{
    Initialize();

    if (levelRoot == null)
        return;

    List<SparkShelfSlot> registered =
        new List<SparkShelfSlot>();

    // Preserve existing valid slot references.
    foreach (SparkShelfSlot slot in slots)
    {
        if (slot != null && !registered.Contains(slot))
            registered.Add(slot);
    }

    SparkShelfSlot[] runtimeSlots =
        levelRoot.GetComponentsInChildren<SparkShelfSlot>(true);

    int added = 0;

    foreach (SparkShelfSlot slot in runtimeSlots)
    {
        if (slot == null || registered.Contains(slot))
            continue;

        registered.Add(slot);
        added++;
    }

    slots = registered.ToArray();

    Debug.Log(
        $"[SparkShelf] Registered {added} runtime slots. " +
        $"Total: {slots.Length}",
        this);
}

        
public void UnregisterRuntimeSlots(Transform levelRoot)
{
    if (levelRoot == null || slots == null)
        return;

    SparkShelfSlot[] runtimeSlots =
        levelRoot.GetComponentsInChildren<SparkShelfSlot>(true);

    HashSet<SparkShelfSlot> removing =
        new HashSet<SparkShelfSlot>(runtimeSlots);

    List<SparkShelfSlot> remaining =
        new List<SparkShelfSlot>();

    foreach (SparkShelfSlot slot in slots)
    {
        if (slot != null && !removing.Contains(slot))
            remaining.Add(slot);
    }

    slots = remaining.ToArray();
}


    }
}

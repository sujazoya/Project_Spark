using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay.Shelf
{
    [DisallowMultipleComponent]
    public sealed class SparkObjectShelf : MonoBehaviour
    {
        [Header("Registration")]
        [SerializeField] private SparkShelfSlot[] slots;
        [SerializeField] private SparkShelfItem[] items;
        [SerializeField] private bool autoFindChildren = true;
        [Header("Placement")]
        [SerializeField, Min(0f)] private float travelSeconds = .35f;
        [SerializeField] private AnimationCurve motion = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool parentToSlot = true;
        [SerializeField] private bool disablePhysicsWhileStored = true;
        [SerializeField] private bool logTransitions;
        [Header("Circuit Safety")]
        [SerializeField] private SparkCircuitSystem circuitSystem;
        [SerializeField] private bool preventMovementWhileConnected = true;

        private readonly List<SparkCircuitConnection> connectionBuffer = new List<SparkCircuitConnection>(8);
        private readonly Dictionary<SparkShelfItem, Coroutine> motions = new Dictionary<SparkShelfItem, Coroutine>();
        private readonly Dictionary<SparkShelfItem, SparkShelfSlot> destinations = new Dictionary<SparkShelfItem, SparkShelfSlot>();
        private readonly Dictionary<SparkShelfItem, Transform> originalParents = new Dictionary<SparkShelfItem, Transform>();
        private readonly Dictionary<SparkShelfItem, RigidbodyState> physics = new Dictionary<SparkShelfItem, RigidbodyState>();
        private bool initialized;

        private struct RigidbodyState
        {
            public Rigidbody body;
            public bool wasKinematic;
            public bool usedGravity;
        }

        public Func<SparkShelfItem, bool> CanMoveItem;
        public event Action<SparkShelfItem, SparkShelfSlot> ItemPlaced;

        private void Awake() { Initialize(); }

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            if (autoFindChildren)
            {
                if (slots == null || slots.Length == 0) slots = GetComponentsInChildren<SparkShelfSlot>(true);
                if (items == null || items.Length == 0) items = GetComponentsInChildren<SparkShelfItem>(true);
            }
            if (items == null) items = Array.Empty<SparkShelfItem>();
            if (slots == null) slots = Array.Empty<SparkShelfSlot>();
            foreach (var item in items)
            {
                if (item == null) continue;
                originalParents[item] = item.transform.parent;
                var rb = item.GetComponent<Rigidbody>();
                if (rb != null) physics[item] = new RigidbodyState { body = rb, wasKinematic = rb.isKinematic, usedGravity = rb.useGravity };
                var start = item.PreferredStorageSlot;
                if (start == null || start.Kind != SparkShelfSlotKind.Storage)
                {
                    foreach (var slot in slots)
                        if (slot != null && slot.Kind == SparkShelfSlotKind.Storage && slot.CanAccept(item)) { start = slot; break; }
                }
                if (start != null && start.TryReserve(item))
                {
                    item.CurrentSlot = start;
                    Snap(item, start);
                }
                else Debug.LogWarning("[SparkShelf] No free starting storage slot for " + item.ItemId, item);
            }
        }

        private bool CanMoveSafely(SparkShelfItem item)
        {
            if (item == null || !item.isActiveAndEnabled || item.IsMoving) return false;
            if (preventMovementWhileConnected)
            {
                if (circuitSystem == null) circuitSystem = GetComponentInParent<SparkCircuitSystem>();
                if (circuitSystem == null) circuitSystem = FindFirstObjectByType<SparkCircuitSystem>();
                var terminals = item.GetComponentsInChildren<SparkTerminal>(true);
                if (terminals.Length > 0 && circuitSystem == null)
                {
                    Debug.LogWarning("[SparkShelf] Circuit system missing: refusing to move electrical item.", item);
                    return false;
                }
                if (circuitSystem != null)
                {
                    foreach (var terminal in terminals)
                    {
                        if (terminal == null) continue;
                        connectionBuffer.Clear();
                        circuitSystem.GetConnections(terminal, connectionBuffer);
                        foreach (var connection in connectionBuffer)
                            if (connection != null && connection.IsValid)
                            {
                                Debug.LogWarning("[SparkShelf] Disconnect wires before moving " + item.ItemId, item);
                                return false;
                            }
                    }
                }
            }
            return CanMoveItem == null || CanMoveItem(item);
        }

        [Header("Manual Placement")]
        [SerializeField] private SparkManualShelfPlacement manualPlacement;

        public bool Touch(SparkShelfItem item)
        {
            if (manualPlacement != null && manualPlacement.isActiveAndEnabled)
                return manualPlacement.SelectItem(item);
            if (item == null || !item.AllowTouch || item.IsMoving) return false;
            var kind = item.IsAtWorkbench ? SparkShelfSlotKind.Storage : SparkShelfSlotKind.Workstation;
            bool moved = MoveToCategorySlot(item, kind);
            if (moved) item.NotifySelected();
            return moved;
        }

        public bool MoveToCategorySlot(SparkShelfItem item, SparkShelfSlotKind kind)
        {
            if (!CanMoveSafely(item)) return false;
            var preferred = kind == SparkShelfSlotKind.Storage ? item.PreferredStorageSlot : item.PreferredWorkstationSlot;
            if (preferred != null && preferred.Kind == kind && preferred.CanAccept(item)) return MoveToSlot(item, preferred);
            foreach (var slot in slots)
                if (slot != null && slot.Kind == kind && slot.CanAccept(item)) return MoveToSlot(item, slot);
            Debug.LogWarning("[SparkShelf] No free " + kind + " slot for " + item.ItemId, item);
            return false;
        }

       public bool CanMoveItemSafely(SparkShelfItem item)
{
    return item != null &&
           !item.IsMoving &&
           CanMoveSafely(item);
}

        public bool MoveToSlot(SparkShelfItem item, SparkShelfSlot target)
        {
            if (target == null || !target.isActiveAndEnabled || !target.CanAccept(item) || !CanMoveSafely(item)) return false;
            if (item.CurrentSlot == target) return true;
            if (!target.TryReserve(item)) return false;
            item.IsMoving = true;
            destinations[item] = target;
            motions[item] = StartCoroutine(AnimateMove(item, item.CurrentSlot, target));
            return true;
        }

        private IEnumerator AnimateMove(SparkShelfItem item, SparkShelfSlot previous, SparkShelfSlot target)
        {
            var tr = item.transform;
            Vector3 startPos = tr.position;
            Quaternion startRot = tr.rotation;
            Freeze(item);
            float elapsed = 0f;
            while (elapsed < travelSeconds)
            {
                elapsed += Time.deltaTime;
                float t = travelSeconds <= 0 ? 1 : Mathf.Clamp01(elapsed / travelSeconds);
                float eased = motion == null ? t : Mathf.Clamp01(motion.Evaluate(t));
                tr.SetPositionAndRotation(Vector3.Lerp(startPos, target.Anchor.position, eased), Quaternion.Slerp(startRot, target.Anchor.rotation, eased));
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
            if (logTransitions) Debug.Log("[SparkShelf] " + item.ItemId + " -> " + target.SlotId, item);
        }

        private void Freeze(SparkShelfItem item)
        {
            if (!disablePhysicsWhileStored) return;
            if (!physics.TryGetValue(item, out var state) || state.body == null) return;
            state.body.linearVelocity = Vector3.zero;
            state.body.angularVelocity = Vector3.zero;
            state.body.isKinematic = true;
            state.body.useGravity = false;
        }

        private void Snap(SparkShelfItem item, SparkShelfSlot slot)
        {
            if (item == null || slot == null) return;
            Freeze(item);
            item.transform.SetPositionAndRotation(slot.Anchor.position, slot.Anchor.rotation);
            if (parentToSlot) item.transform.SetParent(slot.Anchor, true);
            else if (originalParents.TryGetValue(item, out var parent)) item.transform.SetParent(parent, true);
        }

        public bool TryReturnToShelf(SparkShelfItem item) => MoveToCategorySlot(item, SparkShelfSlotKind.Storage);
        public bool TryPlaceOnWorkbench(SparkShelfItem item) => MoveToCategorySlot(item, SparkShelfSlotKind.Workstation);

        private void OnDisable()
        {
            foreach (var pair in motions) if (pair.Value != null) StopCoroutine(pair.Value);
            motions.Clear();
            foreach (var pair in destinations)
            {
                var item = pair.Key;
                var destination = pair.Value;
                if (item == null) continue;
                if (destination != null) destination.Release(item);
                if (item.CurrentSlot != null) Snap(item, item.CurrentSlot);
                item.IsMoving = false;
            }
            destinations.Clear();
        }
        
    }
}

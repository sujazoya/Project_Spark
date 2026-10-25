using System;
using UnityEngine;
using UnityEngine.Events;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Controls shelf/workbench availability without disabling the item root.
    /// Place on the same GameObject as SparkShelfItem, or a child of it.
    /// Does not add/remove wires or electrical connections.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SelfComponent : MonoBehaviour
    {
        public enum ItemState { Stored, Moving, Workbench }

        [Header("Shelf")]
        [SerializeField] private SparkShelfItem shelfItem;
        [SerializeField] private bool useSlotStateAtStartup = true;
        [SerializeField] private ItemState fallbackStartupState = ItemState.Stored;

        [Header("Terminals")]
        [SerializeField] private bool autoFindTerminals = true;
        [SerializeField] private SparkTerminal[] terminals = Array.Empty<SparkTerminal>();
        [Tooltip("Disabling SparkTerminal behaviours can affect circuit registration. Enable only after confirming your circuit system supports re-enabling terminals.")]
        [SerializeField] private bool toggleTerminalBehaviours = false;
        [SerializeField] private Collider[] terminalInteractionColliders = Array.Empty<Collider>();

        [Header("Component Operation")]
        [Tooltip("Only add actual electrical/operational scripts here, never SparkShelfItem, SparkInteractable, or this controller.")]
        [SerializeField] private Behaviour[] operationalBehaviours = Array.Empty<Behaviour>();
        [SerializeField] private Collider[] workbenchOnlyColliders = Array.Empty<Collider>();
        [SerializeField] private GameObject[] workbenchOnlyObjects = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] storageOnlyObjects = Array.Empty<GameObject>();

        [Header("Events")]
        [SerializeField] private UnityEvent onStored;
        [SerializeField] private UnityEvent onWorkbench;
        [SerializeField] private UnityEvent onMoving;

        public ItemState State { get; private set; }
        public bool IsOperational => State == ItemState.Workbench;
        public event Action<SelfComponent, ItemState> StateChanged;

        private bool initialized;

        private void Reset()
        {
            shelfItem = GetComponent<SparkShelfItem>();
            if (shelfItem == null) shelfItem = GetComponentInParent<SparkShelfItem>();
            DiscoverTerminals();
        }

        private void Awake()
        {
            ResolveItem();
            if (autoFindTerminals) DiscoverTerminals();
        }

        private void OnEnable()
        {
            ResolveItem();
            if (shelfItem != null)
            {
                shelfItem.Placed -= HandlePlaced;
                shelfItem.Placed += HandlePlaced;
            }
            RefreshState(true);
        }

        private void Start()
        {
            // Shelf Awake may initialize its CurrentSlot after this component's Awake.
            RefreshState(true);
        }

        private void Update()
        {
            // SparkObjectShelf sets IsMoving but only emits Placed on arrival.
            // Polling this lightweight flag ensures terminals are inactive in transit.
            RefreshState(false);
        }

        private void OnDisable()
        {
            if (shelfItem != null) shelfItem.Placed -= HandlePlaced;
        }

        private void ResolveItem()
        {
            if (shelfItem != null) return;
            shelfItem = GetComponent<SparkShelfItem>();
            if (shelfItem == null) shelfItem = GetComponentInParent<SparkShelfItem>();
        }

        [ContextMenu("Find Terminals")]
        private void DiscoverTerminals()
        {
            Transform root = shelfItem != null ? shelfItem.transform : transform;
            terminals = root.GetComponentsInChildren<SparkTerminal>(true);
        }

        private void HandlePlaced(SparkShelfItem item, SparkShelfSlot slot)
        {
            RefreshState(true);
        }

        public void RefreshState(bool force)
        {
            ItemState next = fallbackStartupState;
            if (shelfItem != null)
            {
                if (shelfItem.IsMoving) next = ItemState.Moving;
                else if (shelfItem.CurrentSlot != null)
                    next = shelfItem.IsAtWorkbench ? ItemState.Workbench : ItemState.Stored;
                else if (useSlotStateAtStartup) next = ItemState.Stored;
            }

            if (initialized && !force && State == next) return;
            if (initialized && State == next) return;
            State = next;
            initialized = true;
            ApplyState();
            StateChanged?.Invoke(this, State);
            switch (State)
            {
                case ItemState.Stored: onStored?.Invoke(); break;
                case ItemState.Workbench: onWorkbench?.Invoke(); break;
                case ItemState.Moving: onMoving?.Invoke(); break;
            }
        }

        private void ApplyState()
        {
            bool active = State == ItemState.Workbench;
            foreach (Collider c in terminalInteractionColliders)
                if (c != null) c.enabled = active;
            foreach (Collider c in workbenchOnlyColliders)
                if (c != null) c.enabled = active;
            foreach (Behaviour b in operationalBehaviours)
                if (b != null && b != this && !(b is SparkInteractable) &&
                    !(b is SparkShelfItem) && !(b is SparkTerminal)) b.enabled = active;
            foreach (GameObject go in workbenchOnlyObjects)
                if (go != null && go != gameObject && !IsAncestorOfShelfItem(go)) go.SetActive(active);
            foreach (GameObject go in storageOnlyObjects)
                if (go != null && go != gameObject && !IsAncestorOfShelfItem(go)) go.SetActive(!active);
            if (toggleTerminalBehaviours)
                foreach (SparkTerminal terminal in terminals)
                    if (terminal != null) terminal.enabled = active;
        }

        private bool IsAncestorOfShelfItem(GameObject go)
        {
            if (shelfItem == null) return false;
            return shelfItem.transform == go.transform || shelfItem.transform.IsChildOf(go.transform);
        }
    }
}

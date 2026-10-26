
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Controls the persistent component shelf for each level.
    ///
    /// Supports:
    /// - Level-specific available components
    /// - Shelf visual visibility
    /// - Safe return of components to storage
    /// - Runtime-generated workstation slots
    /// - Two-click manual shelf placement
    /// - Level transitions
    ///
    /// SparkObjectShelf remains responsible for movement
    /// and electrical safety.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelShelfController : MonoBehaviour
    {
        // ============================================================
        // LEVEL CONFIGURATION
        // ============================================================

        [Serializable]
        public sealed class LevelShelfConfiguration
        {
            public SparkLevelDefinition level;

            public GameObject[] shelfVisuals;

            public SparkShelfItem[] availableItems;

            // Optional scene slots or manually assigned slots.
            // Runtime-generated slots are discovered separately.
            public SparkShelfSlot[] workstationSlots;
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("Existing Systems")]
        [SerializeField]
        private LevelGamePlayManager levelManager;

        [SerializeField]
        private SparkObjectShelf objectShelf;

        [Header("Manual Placement")]
        [SerializeField]
        private SparkManualShelfPlacement manualPlacement;

        // ============================================================
        // LEVEL INVENTORY
        // ============================================================

        [Header("Level Inventory")]
        [SerializeField]
        private LevelShelfConfiguration[] configurations =
            Array.Empty<LevelShelfConfiguration>();

        // ============================================================
        // RUNTIME WORKSTATION SLOTS
        // ============================================================

        [Header("Runtime Workstation Slots")]
        [SerializeField]
        private bool autoDiscoverRuntimeSlots = true;

        [Tooltip(
            "Parent of runtime-generated workstation slots. " +
            "Can be assigned by the level layout loader at runtime.")]
        [SerializeField]
        private Transform workstationSlotRoot;

        [SerializeField]
        private bool includeInactiveRuntimeSlots = false;

        [Tooltip(
            "When enabled, also uses workstation slots " +
            "assigned in the level configuration.")]
        [SerializeField]
        private bool includeConfiguredSlots = true;

        [Tooltip(
            "If the layout is generated after level activation, " +
            "call NotifyRuntimeLayoutReady from the layout loader.")]
        [SerializeField]
        private bool logMissingRuntimeSlots = true;

        // ============================================================
        // BEHAVIOR
        // ============================================================

        [Header("Behavior")]
        [SerializeField]
        private bool hideUnlistedItems = true;

        [SerializeField]
        private bool hideUnlistedShelfVisuals = true;

        [SerializeField]
        private bool returnItemsToStorage = true;

        [SerializeField]
        private bool logChanges = true;

        // ============================================================
        // RUNTIME COLLECTIONS
        // ============================================================

        private readonly HashSet<SparkShelfItem> knownItems =
            new HashSet<SparkShelfItem>();

        private readonly HashSet<GameObject> knownVisuals =
            new HashSet<GameObject>();

        private readonly HashSet<SparkShelfItem> desiredItems =
            new HashSet<SparkShelfItem>();

        private readonly HashSet<GameObject> desiredVisuals =
            new HashSet<GameObject>();

        private readonly HashSet<SparkShelfSlot> runtimeSlots =
            new HashSet<SparkShelfSlot>();

        private Coroutine applying;

        private int generation;

        private bool inventoryReady;

        private SparkLevelDefinition configuredLevel;

        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        public bool InventoryReady => inventoryReady;

        public Transform WorkstationSlotRoot =>
            workstationSlotRoot;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ResolveReferences();
            BuildKnownInventory();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (levelManager == null)
            {
                Debug.LogWarning(
                    "[LevelShelf] LevelGamePlayManager not found.",
                    this);

                return;
            }

            levelManager.LevelStarted -= HandleLevelStarted;
            levelManager.LevelStarted += HandleLevelStarted;

            if (levelManager.ActiveLevel != null)
            {
                ApplyLevel(levelManager.ActiveLevel);
            }
        }

        private void OnDisable()
        {
            if (levelManager != null)
            {
                levelManager.LevelStarted -= HandleLevelStarted;
            }

            generation++;

            if (applying != null)
            {
                StopCoroutine(applying);
                applying = null;
            }

            inventoryReady = false;

            manualPlacement?.ClearLevel();
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            if (levelManager == null)
            {
                levelManager =
                    FindFirstObjectByType<LevelGamePlayManager>();
            }

            if (objectShelf == null)
            {
                objectShelf =
                    FindFirstObjectByType<SparkObjectShelf>();
            }

            if (manualPlacement == null)
            {
                manualPlacement =
                    GetComponent<SparkManualShelfPlacement>();

                if (manualPlacement == null)
                {
                    manualPlacement =
                        FindFirstObjectByType<
                            SparkManualShelfPlacement>();
                }
            }
        }

        // ============================================================
        // KNOWN INVENTORY
        // ============================================================

        private void BuildKnownInventory()
        {
            knownItems.Clear();
            knownVisuals.Clear();

            if (configurations == null)
                return;

            foreach (LevelShelfConfiguration config
                     in configurations)
            {
                if (config == null)
                    continue;

                if (config.availableItems != null)
                {
                    foreach (SparkShelfItem item
                             in config.availableItems)
                    {
                        if (item != null)
                            knownItems.Add(item);
                    }
                }

                if (config.shelfVisuals != null)
                {
                    foreach (GameObject visual
                             in config.shelfVisuals)
                    {
                        if (visual != null)
                            knownVisuals.Add(visual);
                    }
                }
            }
        }

        // ============================================================
        // LEVEL EVENTS
        // ============================================================

        private void HandleLevelStarted(
            SparkLevelDefinition level)
        {
            ApplyLevel(level);
        }

        // ============================================================
        // APPLY LEVEL
        // ============================================================

        public void ApplyLevel(
            SparkLevelDefinition level)
        {
            generation++;

            inventoryReady = false;
            configuredLevel = null;

            manualPlacement?.ClearLevel();

            if (applying != null)
            {
                StopCoroutine(applying);
                applying = null;
            }

            // Prevent old runtime slots from carrying over
            // when a different level starts.
            runtimeSlots.Clear();

            applying =
                StartCoroutine(
                    ApplyRoutine(level, generation));
        }

        private IEnumerator ApplyRoutine(
            SparkLevelDefinition level,
            int token)
        {
            desiredItems.Clear();
            desiredVisuals.Clear();

            LevelShelfConfiguration match =
                FindConfiguration(level);

            if (match == null)
            {
                Debug.LogWarning(
                    "[LevelShelf] No shelf configuration for " +
                    (level != null
                        ? level.name
                        : "null level") +
                    ". Existing shelf items are left unchanged.",
                    this);

                applying = null;
                yield break;
            }

            // --------------------------------------------
            // COLLECT DESIRED ITEMS
            // --------------------------------------------

            if (match.availableItems != null)
            {
                foreach (SparkShelfItem item
                         in match.availableItems)
                {
                    if (item != null)
                        desiredItems.Add(item);
                }
            }

            // --------------------------------------------
            // COLLECT DESIRED VISUALS
            // --------------------------------------------

            if (match.shelfVisuals != null)
            {
                foreach (GameObject visual
                         in match.shelfVisuals)
                {
                    if (visual != null)
                        desiredVisuals.Add(visual);
                }
            }

            HashSet<SparkShelfItem> activeItems =
                new HashSet<SparkShelfItem>(
                    desiredItems);

            HashSet<GameObject> activeVisuals =
                new HashSet<GameObject>(
                    desiredVisuals);

            // --------------------------------------------
            // UPDATE SHELF ITEMS
            // --------------------------------------------

            foreach (SparkShelfItem item in knownItems)
            {
                if (token != generation)
                    yield break;

                if (item == null)
                    continue;

                bool wanted =
                    activeItems.Contains(item);

                if (wanted)
                {
                    item.gameObject.SetActive(true);
                    item.SetTouchEnabled(false);
                }
                else if (hideUnlistedItems)
                {
                    item.SetTouchEnabled(false);
                }

                // ----------------------------------------
                // RETURN TO STORAGE SAFELY
                // ----------------------------------------

                if (returnItemsToStorage &&
                    item.IsAtWorkbench)
                {
                    if (objectShelf == null ||
                        !objectShelf.TryReturnToShelf(item))
                    {
                        item.gameObject.SetActive(true);

                        Debug.LogWarning(
                            "[LevelShelf] Cannot reset " +
                            item.ItemId +
                            ". Disconnect its wires before " +
                            "switching inventory.",
                            item);

                        // Preserve existing behavior:
                        // do not forcibly move wired items.
                        continue;
                    }
                }

                // ----------------------------------------
                // WAIT FOR MOVEMENT
                // ----------------------------------------

                while (item != null &&
                       item.IsMoving)
                {
                    if (token != generation)
                        yield break;

                    yield return null;
                }

                if (token != generation)
                    yield break;

                if (item == null)
                    continue;

                // ----------------------------------------
                // APPLY AVAILABILITY
                // ----------------------------------------

                if (wanted)
                {
                    item.gameObject.SetActive(true);
                    item.SetTouchEnabled(true);
                }
                else if (hideUnlistedItems)
                {
                    item.gameObject.SetActive(false);
                }
            }

            if (token != generation)
                yield break;

            // --------------------------------------------
            // UPDATE SHELF VISUALS
            // --------------------------------------------

            if (hideUnlistedShelfVisuals)
            {
                foreach (GameObject visual in knownVisuals)
                {
                    if (visual != null)
                    {
                        visual.SetActive(
                            activeVisuals.Contains(visual));
                    }
                }
            }
            else
            {
                foreach (GameObject visual in activeVisuals)
                {
                    if (visual != null)
                        visual.SetActive(true);
                }
            }

            if (token != generation)
                yield break;

            // --------------------------------------------
            // MARK INVENTORY READY
            // --------------------------------------------

            configuredLevel = level;
            inventoryReady = true;

            // Discover slots that already exist.
            // If the layout spawns later, its loader
            // must call NotifyRuntimeLayoutReady().
            RefreshRuntimeWorkstationSlots();

            if (logChanges)
            {
                Debug.Log(
                    "[LevelShelf] Activated " +
                    (level != null
                        ? level.name
                        : "Unknown") +
                    " with " +
                    activeItems.Count +
                    " available shelf items.",
                    this);
            }

            applying = null;
        }

        // ============================================================
        // CONFIGURATION LOOKUP
        // ============================================================

        private LevelShelfConfiguration FindConfiguration(
            SparkLevelDefinition level)
        {
            if (configurations == null)
                return null;

            foreach (LevelShelfConfiguration config
                     in configurations)
            {
                if (config != null &&
                    config.level == level)
                {
                    return config;
                }
            }

            return null;
        }

        // ============================================================
        // RUNTIME SLOT ROOT
        // ============================================================

        /// <summary>
        /// Assign the parent of generated workstation slots.
        /// Does not trigger registration until refresh.
        /// </summary>
        public void SetWorkstationSlotRoot(
            Transform root)
        {
            workstationSlotRoot = root;
        }

        // ============================================================
        // LAYOUT READY NOTIFICATION
        // ============================================================

        /// <summary>
        /// Call after the runtime layout finishes spawning.
        ///
        /// This can be called before or after inventory setup.
        /// If inventory is not ready, the slot root is retained
        /// and discovered when setup finishes.
        /// </summary>
        public void NotifyRuntimeLayoutReady(
            Transform generatedLevelRoot)
        {
            workstationSlotRoot = generatedLevelRoot;

            if (logChanges)
            {
                Debug.Log(
                    "[LevelShelf] Runtime layout ready: " +
                    (generatedLevelRoot != null
                        ? generatedLevelRoot.name
                        : "NULL"),
                    this);
            }

            if (inventoryReady)
            {
                RefreshRuntimeWorkstationSlots();
            }
        }

        // ============================================================
        // RUNTIME SLOT DISCOVERY
        // ============================================================

        private SparkShelfSlot[]
            FindRuntimeWorkstationSlots()
        {
            if (!autoDiscoverRuntimeSlots)
                return Array.Empty<SparkShelfSlot>();

            if (workstationSlotRoot == null)
            {
                if (logMissingRuntimeSlots)
                {
                    Debug.LogWarning(
                        "[LevelShelf] Workstation slot root " +
                        "has not been assigned yet.",
                        this);
                }

                return Array.Empty<SparkShelfSlot>();
            }

            SparkShelfSlot[] discovered =
                workstationSlotRoot
                    .GetComponentsInChildren<SparkShelfSlot>(
                        includeInactiveRuntimeSlots);

            List<SparkShelfSlot> results =
                new List<SparkShelfSlot>();

            foreach (SparkShelfSlot slot in discovered)
            {
                if (slot == null)
                    continue;

                if (slot.Kind !=
                    SparkShelfSlotKind.Workstation)
                {
                    continue;
                }

                if (!results.Contains(slot))
                    results.Add(slot);
            }

            return results.ToArray();
        }

        // ============================================================
        // COLLECT ALLOWED SLOTS
        // ============================================================

        private SparkShelfSlot[] CollectAllowedSlots(
            LevelShelfConfiguration config)
        {
            HashSet<SparkShelfSlot> result =
                new HashSet<SparkShelfSlot>();

            // --------------------------------------------
            // CONFIGURED SLOTS
            // --------------------------------------------

            if (includeConfiguredSlots &&
                config != null &&
                config.workstationSlots != null)
            {
                foreach (SparkShelfSlot slot
                         in config.workstationSlots)
                {
                    if (slot != null &&
                        slot.Kind ==
                            SparkShelfSlotKind.Workstation)
                    {
                        result.Add(slot);
                    }
                }
            }

            // --------------------------------------------
            // DISCOVERED RUNTIME SLOTS
            // --------------------------------------------

            SparkShelfSlot[] discovered =
                FindRuntimeWorkstationSlots();

            foreach (SparkShelfSlot slot in discovered)
            {
                if (slot != null)
                    result.Add(slot);
            }

            // --------------------------------------------
            // EXPLICITLY REGISTERED RUNTIME SLOTS
            // --------------------------------------------

            runtimeSlots.RemoveWhere(
                slot => slot == null);

            foreach (SparkShelfSlot slot in runtimeSlots)
            {
                if (slot != null &&
                    slot.Kind ==
                        SparkShelfSlotKind.Workstation)
                {
                    result.Add(slot);
                }
            }

            SparkShelfSlot[] slots =
                new SparkShelfSlot[result.Count];

            result.CopyTo(slots);

            return slots;
        }

        // ============================================================
        // REFRESH MANUAL PLACEMENT
        // ============================================================

        /// <summary>
        /// Refreshes the manual placement destination list.
        /// Safe to call after runtime workstation spawning.
        ///
        /// Note: Configure cancels the current shelf selection.
        /// Call this after layout changes, not on every frame.
        /// </summary>
        public void RefreshRuntimeWorkstationSlots()
        {
            ResolveReferences();

            if (!inventoryReady)
            {
                if (logChanges)
                {
                    Debug.Log(
                        "[LevelShelf] Slot refresh deferred " +
                        "until inventory is ready.",
                        this);
                }

                return;
            }

            if (manualPlacement == null)
            {
                Debug.LogWarning(
                    "[LevelShelf] SparkManualShelfPlacement missing.",
                    this);

                return;
            }

            LevelShelfConfiguration config =
                FindConfiguration(configuredLevel);

            if (config == null)
            {
                Debug.LogWarning(
                    "[LevelShelf] Cannot refresh slots: " +
                    "active level configuration missing.",
                    this);

                return;
            }

            SparkShelfSlot[] activeSlots =
                CollectAllowedSlots(config);

            manualPlacement.Configure(
                config.availableItems,
                activeSlots);

            if (logChanges)
            {
                Debug.Log(
                    "[LevelShelf] Registered " +
                    activeSlots.Length +
                    " workstation slots for " +
                    (configuredLevel != null
                        ? configuredLevel.name
                        : "Unknown"),
                    this);

                foreach (SparkShelfSlot slot in activeSlots)
                {
                    if (slot != null)
                    {
                        Debug.Log(
                            "[LevelShelf] Allowed workstation: " +
                            slot.SlotId +
                            " | Category: " +
                            slot.Category,
                            slot);
                    }
                }
            }
        }

        // ============================================================
        // REGISTER INDIVIDUAL RUNTIME SLOT
        // ============================================================

        /// <summary>
        /// Registers a newly spawned workstation slot.
        ///
        /// Useful when slots are spawned individually
        /// rather than beneath one layout root.
        /// </summary>
        public bool RegisterRuntimeWorkstationSlot(
            SparkShelfSlot slot)
        {
            if (slot == null)
                return false;

            if (slot.Kind !=
                SparkShelfSlotKind.Workstation)
            {
                Debug.LogWarning(
                    "[LevelShelf] Cannot register " +
                    slot.SlotId +
                    ": slot Kind must be Workstation.",
                    slot);

                return false;
            }

            bool added = runtimeSlots.Add(slot);

            if (added && inventoryReady)
            {
                RefreshRuntimeWorkstationSlots();
            }

            return added;
        }

        // ============================================================
        // UNREGISTER RUNTIME SLOT
        // ============================================================

        public void UnregisterRuntimeWorkstationSlot(
            SparkShelfSlot slot)
        {
            if (slot == null)
                return;

            if (runtimeSlots.Remove(slot) &&
                inventoryReady)
            {
                RefreshRuntimeWorkstationSlots();
            }
        }

        // ============================================================
        // DEBUG
        // ============================================================

        [ContextMenu("Debug / Refresh Workstation Slots")]
        private void DebugRefreshWorkstationSlots()
        {
            RefreshRuntimeWorkstationSlots();
        }

        [ContextMenu("Debug / Print Runtime Slots")]
        private void DebugPrintRuntimeSlots()
        {
            SparkShelfSlot[] slots =
                FindRuntimeWorkstationSlots();

            Debug.Log(
                "[LevelShelf] Discovered " +
                slots.Length +
                " runtime workstation slots.",
                this);

            foreach (SparkShelfSlot slot in slots)
            {
                if (slot != null)
                {
                    Debug.Log(
                        "[LevelShelf] " +
                        slot.SlotId +
                        " | " +
                        slot.Kind +
                        " | " +
                        slot.Category,
                        slot);
                }
            }
        }
    }
}

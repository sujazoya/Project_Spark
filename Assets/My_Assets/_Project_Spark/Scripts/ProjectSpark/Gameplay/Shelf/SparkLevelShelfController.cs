using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Level-driven visibility and availability for a persistent shelf.
    /// Does not modify electrical connections or replace the shelf manager.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelShelfController : MonoBehaviour
    {
        [Serializable]
        public sealed class LevelShelfConfiguration
        {
            public SparkLevelDefinition level;
            public GameObject[] shelfVisuals;
            public SparkShelfItem[] availableItems;
            public SparkShelfSlot[] workstationSlots;
        }

        [Header("Existing Systems")]
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkObjectShelf objectShelf;

        [Header("Level Inventory")]
        [SerializeField] private LevelShelfConfiguration[] configurations =
            Array.Empty<LevelShelfConfiguration>();

        [Header("Manual Placement")]
        [SerializeField] private SparkManualShelfPlacement manualPlacement;

        [Header("Behavior")]
        [SerializeField] private bool hideUnlistedItems = true;
        [SerializeField] private bool hideUnlistedShelfVisuals = true;
        [SerializeField] private bool returnItemsToStorage = true;
        [SerializeField] private bool logChanges;

        private readonly HashSet<SparkShelfItem> knownItems = new HashSet<SparkShelfItem>();
        private readonly HashSet<GameObject> knownVisuals = new HashSet<GameObject>();
        private readonly HashSet<SparkShelfItem> desiredItems = new HashSet<SparkShelfItem>();
        private readonly HashSet<GameObject> desiredVisuals = new HashSet<GameObject>();
        private Coroutine applying;
        private int generation;

        private void Awake()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>();
            if (objectShelf == null)
                objectShelf = FindFirstObjectByType<SparkObjectShelf>();

            foreach (LevelShelfConfiguration config in configurations)
            {
                if (config == null) continue;
                if (config.availableItems != null)
                    foreach (SparkShelfItem item in config.availableItems)
                        if (item != null) knownItems.Add(item);
                if (config.shelfVisuals != null)
                    foreach (GameObject visual in config.shelfVisuals)
                        if (visual != null) knownVisuals.Add(visual);
            }
        }

        private void OnEnable()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>();
            if (levelManager != null)
            {
                levelManager.LevelStarted -= HandleLevelStarted;
                levelManager.LevelStarted += HandleLevelStarted;
                if (levelManager.ActiveLevel != null)
                    ApplyLevel(levelManager.ActiveLevel);
            }
            else
                Debug.LogWarning("[LevelShelf] LevelGamePlayManager not found.", this);
        }

        private void OnDisable()
        {
            if (levelManager != null)
                levelManager.LevelStarted -= HandleLevelStarted;
            generation++;
            if (applying != null) StopCoroutine(applying);
            applying = null;
        }

        private void HandleLevelStarted(SparkLevelDefinition level) => ApplyLevel(level);

        public void ApplyLevel(SparkLevelDefinition level)
        {
            generation++;
            if (manualPlacement != null) manualPlacement.ClearLevel();
            if (applying != null) StopCoroutine(applying);
            applying = StartCoroutine(ApplyRoutine(level, generation));
        }

        private IEnumerator ApplyRoutine(SparkLevelDefinition level, int token)
        {
            desiredItems.Clear();
            desiredVisuals.Clear();
            LevelShelfConfiguration match = null;
            foreach (LevelShelfConfiguration config in configurations)
                if (config != null && config.level == level) { match = config; break; }

            if (match == null)
            {
                Debug.LogWarning("[LevelShelf] No shelf configuration for " +
                    (level != null ? level.name : "null level") +
                    ". Existing shelf items are left unchanged.", this);
                applying = null;
                yield break;
            }

            if (match.availableItems != null)
                foreach (SparkShelfItem item in match.availableItems)
                    if (item != null) desiredItems.Add(item);
            if (match.shelfVisuals != null)
                foreach (GameObject visual in match.shelfVisuals)
                    if (visual != null) desiredVisuals.Add(visual);

            // Store desired state locally so a newer level cannot change this routine's sets.
            var activeItems = new HashSet<SparkShelfItem>(desiredItems);
            var activeVisuals = new HashSet<GameObject>(desiredVisuals);

            foreach (SparkShelfItem item in knownItems)
            {
                if (token != generation) yield break;
                if (item == null) continue;

                bool wanted = activeItems.Contains(item);
                if (wanted)
                {
                    item.gameObject.SetActive(true);
                    item.SetTouchEnabled(false);
                }
                else if (hideUnlistedItems)
                {
                    item.SetTouchEnabled(false);
                }

                // Return any used item to storage before hiding or making it available again.
                if (returnItemsToStorage && item.IsAtWorkbench)
                {
                    if (objectShelf == null || !objectShelf.TryReturnToShelf(item))
                    {
                        // Typically still wired. Keep it visible, don't invalidate the circuit.
                        item.gameObject.SetActive(true);
                        Debug.LogWarning("[LevelShelf] Cannot reset " + item.ItemId +
                            ". Disconnect its wires before switching inventory.", item);
                        continue;
                    }
                }

                while (item != null && item.IsMoving)
                {
                    if (token != generation) yield break;
                    yield return null;
                }
                if (token != generation) yield break;
                if (item == null) continue;

                if (wanted)
                {
                    item.gameObject.SetActive(true);
                    item.SetTouchEnabled(true);
                }
                else if (hideUnlistedItems)
                    item.gameObject.SetActive(false);
            }

            if (hideUnlistedShelfVisuals)
                foreach (GameObject visuals in knownVisuals)
                    if (visuals != null) visuals.SetActive(activeVisuals.Contains(visuals));
            else
                foreach (GameObject visual in activeVisuals)
                    if (visual != null) visual.SetActive(true);

            if (manualPlacement != null && token == generation)
                manualPlacement.Configure(match.availableItems, match.workstationSlots);

            if (logChanges)
                Debug.Log("[LevelShelf] Activated " + level.name +
                    " with " + activeItems.Count + " items.", this);
            applying = null;
        }
    }
}

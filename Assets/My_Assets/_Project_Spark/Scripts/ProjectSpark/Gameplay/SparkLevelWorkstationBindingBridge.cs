using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Bridges level-owned logical IDs to whatever item occupies a workstation.
    /// Smart slots own their IDs; legacy manual mappings remain supported.
    /// Attach to the original SparkLevelLayout prefab root.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelWorkstationBindingBridge : MonoBehaviour
    {
        [Serializable]
        public sealed class WorkstationBinding
        {
            public string slotId;
            public string componentBindingId;
            public string inputTerminalBindingId;
            public string outputTerminalBindingId;
            public string electronicObjectBindingId;
            [Header("Legacy object discovery")]
            public string electricalTypeName = "SparkResistor";
            public string electronicTypeName = "";
            public string inputTerminalName = "_Input";
            public string outputTerminalName = "_Output";
            public bool exactTerminalNames;
        }

        [Header("Existing Systems")]
        [SerializeField] private SparkLevelLayoutLoader layoutLoader;
        [SerializeField] private SparkObjectShelf shelf;
        [Header("Smart Slots")]
        [SerializeField] private bool useSmartSlots = true;
        [Tooltip("Legacy rules for slots without smart binding enabled. A smart slot takes priority for the same SlotId.")]
        [SerializeField] private WorkstationBinding[] workstations = Array.Empty<WorkstationBinding>();
        [Header("Reliability")]
        [SerializeField, Min(0.1f)] private float resyncSeconds = .25f;
        [SerializeField] private bool logChanges = true;

        private sealed class Rule
        {
            public string slotId, componentId, aId, bId, electronicId;
            public string electricalType, electronicType, aMatch, bMatch;
            public bool exact;
            public SparkShelfSlot slot;
        }
        private sealed class State
        {
            public SparkLevelSceneBindings bindings;
            public Rule rule;
            public SparkShelfItem item;
            public SparkElectricalComponent component;
            public SparkTerminal a, b;
            public SparkElectronicObject electronic;
        }

        private readonly Dictionary<string, State> applied = new Dictionary<string, State>(StringComparer.Ordinal);
        private readonly HashSet<SparkShelfSlot> watched = new HashSet<SparkShelfSlot>();
        private readonly HashSet<string> warnings = new HashSet<string>();
        private SparkLevelLayout layout;
        private LevelGamePlayManager gameplay;
        private SparkObjectShelf subscribedShelf;
        private float nextSync;
        private bool syncing;

        private void Awake() { Resolve(); }
        private void OnEnable() { Resolve(); SubscribeShelf(); Synchronize(); }
        private void Start() { Resolve(); SubscribeShelf(); Synchronize(); }
        private void LateUpdate()
        {
            if (Time.unscaledTime < nextSync) return;
            nextSync = Time.unscaledTime + Mathf.Max(.1f, resyncSeconds);
            Resolve(); SubscribeShelf(); Synchronize();
        }
        private void OnDisable() { UnsubscribeAll(); ClearApplied(); }
        private void OnDestroy() { UnsubscribeAll(); ClearApplied(); }

        private void Resolve()
        {
            if (layout == null) layout = GetComponentInParent<SparkLevelLayout>();
            if (layoutLoader == null) layoutLoader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (shelf == null) shelf = FindFirstObjectByType<SparkObjectShelf>();
            if (gameplay == null) gameplay = FindFirstObjectByType<LevelGamePlayManager>();
        }
        private void SubscribeShelf()
        {
            if (subscribedShelf == shelf) return;
            if (subscribedShelf != null) subscribedShelf.ItemPlaced -= OnItemPlaced;
            subscribedShelf = shelf;
            if (subscribedShelf != null) subscribedShelf.ItemPlaced += OnItemPlaced;
        }
        private void UnsubscribeAll()
        {
            if (subscribedShelf != null) subscribedShelf.ItemPlaced -= OnItemPlaced;
            subscribedShelf = null;
            foreach (SparkShelfSlot slot in watched)
                if (slot != null) slot.BindingRefreshRequested -= OnSlotRefresh;
            watched.Clear();
        }
        private void OnItemPlaced(SparkShelfItem item, SparkShelfSlot slot) => Synchronize();
        private void OnSlotRefresh(SparkShelfSlot slot) => Synchronize();

        private void WatchSlots(IReadOnlyList<SparkShelfSlot> all)
        {
            var desired = new HashSet<SparkShelfSlot>();
            if (all != null && useSmartSlots)
                foreach (SparkShelfSlot slot in all)
                    if (slot != null && slot.EnableSmartBinding &&
                        slot.transform.IsChildOf(layout.transform)) desired.Add(slot);
            var remove = new List<SparkShelfSlot>();
            foreach (SparkShelfSlot slot in watched)
                if (slot == null || !desired.Contains(slot)) remove.Add(slot);
            foreach (SparkShelfSlot slot in remove)
            {
                if (slot != null) slot.BindingRefreshRequested -= OnSlotRefresh;
                watched.Remove(slot);
            }
            foreach (SparkShelfSlot slot in desired)
                if (watched.Add(slot)) slot.BindingRefreshRequested += OnSlotRefresh;
        }

        [ContextMenu("Project Spark / Synchronize Workstation Bindings")]
        public void Synchronize()
        {
            if (!Application.isPlaying || syncing) return;
            if (layout == null || layoutLoader == null || shelf == null ||
                layoutLoader.ActiveLayout != layout) return;
            SparkLevelSceneBindings bindings = layoutLoader.ActiveBindings;
            if (bindings == null) return;
            syncing = true;
            try
            {
                IReadOnlyList<SparkShelfSlot> all = shelf.RegisteredSlots;
                WatchSlots(all);
                var rules = BuildRules(all);
                var owners = new HashSet<string>(StringComparer.Ordinal);
                var usable = new List<Rule>();
                var slotIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (Rule rule in rules)
                {
                    if (!slotIds.Add(rule.slotId))
                    {
                        Warn("Duplicate workstation SlotId '" + rule.slotId + "'. Fix duplicate slots/rules.");
                        if (applied.Count > 0) { ClearApplied(); gameplay?.QueueEvaluation(); }
                        return;
                    }
                    string issue = Validate(rule, bindings);
                    if (issue != null)
                    {
                        Warn(rule.slotId + ": " + issue);
                        if (applied.Count > 0) { ClearApplied(); gameplay?.QueueEvaluation(); }
                        return;
                    }
                    // Validate all aliases BEFORE claiming any. A failed rule must
                    // not partially occupy the alias set for subsequent slots.
                    if (!CanClaim(owners, rule))
                    {
                        Warn("Duplicate logical binding ID at " + rule.slotId + ". Check slot IDs and legacy rules.");
                        // Fail closed. An invalid mapping must not keep old bindings alive.
                        if (applied.Count > 0)
                        {
                            ClearApplied();
                            gameplay?.QueueEvaluation();
                        }
                        return;
                    }
                    ClaimAll(owners, rule);
                    usable.Add(rule);
                }
                bool changed = false;
                var currentKeys = new HashSet<string>(StringComparer.Ordinal);
                foreach (Rule rule in usable) currentKeys.Add(rule.slotId);

                // IMPORTANT: Clear removed mappings BEFORE writing new mappings.
                // Otherwise an old slot can mask an alias that now belongs to
                // another workstation in this same synchronization pass.
                foreach (string key in new List<string>(applied.Keys))
                {
                    if (currentKeys.Contains(key)) continue;
                    Mask(applied[key]);
                    applied.Remove(key);
                    changed = true;
                }
                foreach (Rule rule in usable)
                    changed |= SynchronizeOne(rule, bindings);

                if (changed && gameplay != null) gameplay.QueueEvaluation();
            }
            finally { syncing = false; }
        }

       
private List<Rule> BuildRules(
    IReadOnlyList<SparkShelfSlot> all)
{
    var result = new List<Rule>();

    var smartIds =
        new HashSet<string>(StringComparer.Ordinal);

    // --------------------------------------------------
    // SMART SLOTS
    // --------------------------------------------------

    if (useSmartSlots && all != null)
    {
        foreach (SparkShelfSlot slot in all)
        {
            if (slot == null ||
                !slot.isActiveAndEnabled ||
                !slot.transform.IsChildOf(layout.transform) ||
                !slot.EnableSmartBinding)
            {
                continue;
            }

            bool usesAnodeCathode =
                slot.Category == SparkShelfCategory.LED ||
                slot.Category == SparkShelfCategory.Diode;

            string inputMatch =
                usesAnodeCathode
                    ? "_Anode"
                    : "_Input";

            string outputMatch =
                usesAnodeCathode
                    ? "_Cathode"
                    : "_Output";

            result.Add(new Rule
            {
                slotId = slot.SlotId,
                slot = slot,

                componentId = slot.RuntimeComponentId,
                aId = slot.RuntimeTerminalAId,
                bId = slot.RuntimeTerminalBId,

                aMatch = inputMatch,
                bMatch = outputMatch,

                electricalType = "",
                electronicType = "",
                exact = false
            });

            smartIds.Add(slot.SlotId);
        }
    }

    // --------------------------------------------------
    // LEGACY FALLBACK
    // --------------------------------------------------

    if (workstations != null)
    {
        foreach (WorkstationBinding legacy in workstations)
        {
            if (legacy == null ||
                string.IsNullOrWhiteSpace(legacy.slotId) ||
                smartIds.Contains(legacy.slotId))
            {
                continue;
            }

            result.Add(new Rule
            {
                slotId = legacy.slotId,

                componentId = legacy.componentBindingId,
                aId = legacy.inputTerminalBindingId,
                bId = legacy.outputTerminalBindingId,

                electronicId =
                    legacy.electronicObjectBindingId,

                electricalType =
                    legacy.electricalTypeName,

                electronicType =
                    legacy.electronicTypeName,

                aMatch = legacy.inputTerminalName,
                bMatch = legacy.outputTerminalName,

                exact = legacy.exactTerminalNames
            });
        }
    }

    return result;
}


        private string Validate(Rule rule, SparkLevelSceneBindings bindings)
        {
            if (string.IsNullOrWhiteSpace(rule.slotId)) return "Missing SlotId";
            if (string.IsNullOrWhiteSpace(rule.componentId) && string.IsNullOrWhiteSpace(rule.aId) &&
                string.IsNullOrWhiteSpace(rule.bId) && string.IsNullOrWhiteSpace(rule.electronicId)) return "All runtime IDs are blank";
            if (!string.IsNullOrWhiteSpace(rule.componentId) && !bindings.IsComponentIdDeclared(rule.componentId)) return "Undeclared component ID: " + rule.componentId;
            if (!string.IsNullOrWhiteSpace(rule.aId) && !bindings.IsTerminalIdDeclared(rule.aId)) return "Undeclared terminal ID: " + rule.aId;
            if (!string.IsNullOrWhiteSpace(rule.bId) && !bindings.IsTerminalIdDeclared(rule.bId)) return "Undeclared terminal ID: " + rule.bId;
            if (!string.IsNullOrWhiteSpace(rule.electronicId) && !bindings.IsElectronicObjectIdDeclared(rule.electronicId)) return "Undeclared electronic ID: " + rule.electronicId;
            if (!string.IsNullOrWhiteSpace(rule.aId) && rule.aId == rule.bId) return "Terminal IDs must differ";
            return null;
        }
        private static bool CanClaim(HashSet<string> owners, Rule rule)
        {
            var local = new HashSet<string>(StringComparer.Ordinal);
            return CanClaimId(owners, local, "C:", rule.componentId) &&
                   CanClaimId(owners, local, "T:", rule.aId) &&
                   CanClaimId(owners, local, "T:", rule.bId) &&
                   CanClaimId(owners, local, "E:", rule.electronicId);
        }
        private static bool CanClaimId(HashSet<string> owners,
            HashSet<string> local, string kind, string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return true;
            string key = kind + id;
            return !owners.Contains(key) && local.Add(key);
        }
        private static void ClaimAll(HashSet<string> owners, Rule rule)
        {
            AddClaim(owners, "C:", rule.componentId);
            AddClaim(owners, "T:", rule.aId);
            AddClaim(owners, "T:", rule.bId);
            AddClaim(owners, "E:", rule.electronicId);
        }
        private static void AddClaim(HashSet<string> owners, string kind, string id)
        {
            if (!string.IsNullOrWhiteSpace(id)) owners.Add(kind + id);
        }

        private SparkShelfSlot FindSlot(string id, out bool ambiguous)
        {
            ambiguous = false;
            SparkShelfSlot found = null;
            IReadOnlyList<SparkShelfSlot> all = shelf.RegisteredSlots;
            if (all == null) return null;
            foreach (SparkShelfSlot slot in all)
            {
                if (slot == null || !slot.isActiveAndEnabled || slot.Kind != SparkShelfSlotKind.Workstation ||
                    !string.Equals(slot.SlotId, id, StringComparison.Ordinal)) continue;
                if (found != null && found != slot) { ambiguous = true; return null; }
                found = slot;
            }
            return found;
        }
        private bool SynchronizeOne(Rule rule, SparkLevelSceneBindings bindings)
        {
            bool ambiguous;
            SparkShelfSlot slot = FindSlot(rule.slotId, out ambiguous);
            if (ambiguous) Warn("Ambiguous active slot: " + rule.slotId);
            SparkShelfItem item = Installed(slot);
            SparkElectricalComponent component = null;
            SparkTerminal a = null, b = null;
            SparkElectronicObject electronic = null;
            if (item != null)
            {
                if (!string.IsNullOrWhiteSpace(rule.componentId))
                    component = UniqueComponent<SparkElectricalComponent>(item, rule.electricalType);
                if (!string.IsNullOrWhiteSpace(rule.aId)) a = UniqueTerminal(item, rule.aMatch, rule.exact);
                if (!string.IsNullOrWhiteSpace(rule.bId)) b = UniqueTerminal(item, rule.bMatch, rule.exact);
                if (!string.IsNullOrWhiteSpace(rule.electronicId))
                    electronic = UniqueComponent<SparkElectronicObject>(item, rule.electronicType);
                if ((!string.IsNullOrWhiteSpace(rule.componentId) && component == null) ||
                    (!string.IsNullOrWhiteSpace(rule.aId) && a == null) ||
                    (!string.IsNullOrWhiteSpace(rule.bId) && b == null) ||
                    (!string.IsNullOrWhiteSpace(rule.electronicId) && electronic == null) || (a != null && a == b))
                {
                    Warn(rule.slotId + ": Installed item '" + item.name + "' does not uniquely match component/pin roles; binding masked.");
                    component = null; a = null; b = null; electronic = null;
                }
            }
            applied.TryGetValue(rule.slotId, out State old);
            bool different = old == null || old.bindings != bindings ||
                old.rule.componentId != rule.componentId || old.rule.aId != rule.aId ||
                old.rule.bId != rule.bId || old.rule.electronicId != rule.electronicId ||
                old.item != item || old.component != component || old.a != a ||
                old.b != b || old.electronic != electronic;
            if (!different) return false;
            if (old != null) Mask(old); // mask old aliases, including when IDs change
            Apply(rule, bindings, component, a, b, electronic);
            applied[rule.slotId] = new State {
                bindings = bindings, rule = rule, item = item, component = component,
                a = a, b = b, electronic = electronic
            };
            if (logChanges)
                Debug.Log("[SmartSlotBridge] " + rule.slotId + " | " +
                    (item != null ? item.ItemId : "EMPTY") + " | " +
                    (component != null ? "BOUND" : "UNBOUND"), this);
            return true;
        }
        private static SparkShelfItem Installed(SparkShelfSlot slot)
        {
            if (slot == null) return null;
            // Shelf-owned occupancy is authoritative. Do not bind a reserved
            // destination until movement has completed and CurrentSlot agrees.
            SparkShelfItem item = slot.Occupant;
            return item != null && !item.IsMoving && item.CurrentSlot == slot &&
                   item.isActiveAndEnabled && item.gameObject.activeInHierarchy &&
                   item.IsAtWorkbench ? item : null;
        }
        private static T UniqueComponent<T>(SparkShelfItem item, string typeName) where T : Component
        {
            T found = null;
            foreach (T candidate in item.GetComponentsInChildren<T>(true))
            {
                if (candidate == null) continue;
                Type type = candidate.GetType();
                if (!string.IsNullOrWhiteSpace(typeName) && type.Name != typeName && type.FullName != typeName) continue;
                if (found != null) return null;
                found = candidate;
            }
            return found;
        }
        private static SparkTerminal UniqueTerminal(SparkShelfItem item, string match, bool exact)
        {
            if (string.IsNullOrWhiteSpace(match)) return null;
            SparkTerminal found = null;
            foreach (SparkTerminal terminal in item.GetComponentsInChildren<SparkTerminal>(true))
            {
                if (terminal == null) continue;
                string name = terminal.gameObject.name;
                bool matches = exact ? string.Equals(name, match, StringComparison.Ordinal)
                    : name.EndsWith(match, StringComparison.OrdinalIgnoreCase);
                if (!matches) continue;
                if (found != null) return null;
                found = terminal;
            }
            return found;
        }
        private static void Apply(Rule rule, SparkLevelSceneBindings bindings,
            SparkElectricalComponent component, SparkTerminal a, SparkTerminal b, SparkElectronicObject electronic)
        {
            if (bindings == null) return;
            if (!string.IsNullOrWhiteSpace(rule.componentId)) bindings.SetRuntimeComponentBinding(rule.componentId, component);
            if (!string.IsNullOrWhiteSpace(rule.aId)) bindings.SetRuntimeTerminal(rule.aId, a);
            if (!string.IsNullOrWhiteSpace(rule.bId)) bindings.SetRuntimeTerminal(rule.bId, b);
            if (!string.IsNullOrWhiteSpace(rule.electronicId)) bindings.SetRuntimeElectronicObject(rule.electronicId, electronic);
        }
        private static void Mask(State state)
        {
            if (state == null || state.bindings == null || state.rule == null) return;
            Apply(state.rule, state.bindings, null, null, null, null);
        }
        private void ClearApplied()
        {
            foreach (State state in applied.Values) Mask(state);
            applied.Clear();
        }
        private void Warn(string message)
        {
            if (warnings.Add(message)) Debug.LogWarning("[SmartSlotBridge] " + message, this);
        }

        [ContextMenu("Project Spark / Synchronize AND REPORT")]
        public void SynchronizeNow() { Resolve(); SubscribeShelf(); Synchronize(); LogDiagnostics(); }
        [ContextMenu("Project Spark / Diagnose Workstation Bindings")]
        public void LogDiagnostics()
        {
            Resolve();
            var sb = new StringBuilder("==== SMART SLOT BINDINGS ====\n");
            sb.AppendLine("Active layout: " + (layoutLoader != null && layoutLoader.ActiveLayout == layout));
            sb.AppendLine("Shelf: " + (shelf != null ? shelf.name : "MISSING"));
            var bindings = layoutLoader != null ? layoutLoader.ActiveBindings : null;
            if (shelf != null && bindings != null)
            {
                foreach (Rule rule in BuildRules(shelf.RegisteredSlots))
                {
                    bool ambiguous;
                    SparkShelfSlot slot = FindSlot(rule.slotId, out ambiguous);
                    SparkShelfItem item = Installed(slot);
                    sb.AppendLine(rule.slotId + " | slot=" + (slot != null ? "FOUND" : "MISSING") +
                        " | occupant=" + (slot != null && slot.Occupant != null ? slot.Occupant.ItemId : "NONE") +
                        " | installed=" + (item != null ? item.ItemId : "NONE") +
                        " | rule=" + (Validate(rule, bindings) ?? "VALID") +
                        (ambiguous ? " | DUPLICATE SLOT" : ""));
                    if (!string.IsNullOrWhiteSpace(rule.componentId))
                    {
                        SparkElectricalComponent c;
                        sb.AppendLine("  " + rule.componentId + " => " + (bindings.TryGetComponent(rule.componentId, out c) && c != null ? c.name : "UNBOUND"));
                    }
                    if (!string.IsNullOrWhiteSpace(rule.aId))
                    {
                        SparkTerminal t;
                        sb.AppendLine("  " + rule.aId + " => " + (bindings.TryGetTerminal(rule.aId, out t) && t != null ? t.name : "UNBOUND"));
                    }
                    if (!string.IsNullOrWhiteSpace(rule.bId))
                    {
                        SparkTerminal t;
                        sb.AppendLine("  " + rule.bId + " => " + (bindings.TryGetTerminal(rule.bId, out t) && t != null ? t.name : "UNBOUND"));
                    }
                }
            }
            Debug.Log(sb.ToString(), this);
        }
    }
}

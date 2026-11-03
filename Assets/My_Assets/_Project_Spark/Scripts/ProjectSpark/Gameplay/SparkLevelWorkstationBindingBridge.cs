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
    /// Level-authored workstation mappings, applicable to ANY level. Add to the
    /// SparkLevelLayout prefab (not the persistent shelf). No level numbers or
    /// object names are hardcoded in the implementation.
    ///
    /// The level owns the logical IDs; the shelf owns physical objects.
    /// This component connects the two ONLY through the active layout binding.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelWorkstationBindingBridge : MonoBehaviour
    {
        [Serializable]
        public sealed class WorkstationBinding
        {
            [Tooltip("Exact SparkShelfSlot.SlotId of the workstation (not a hierarchy path). Must be unique among active registered slots.")]
            public string slotId;
            [Tooltip("Declared runtime electrical component ID on this level's SceneBindings.")]
            public string componentBindingId;
            [Tooltip("Declared runtime input terminal ID; optional for components without terminals.")]
            public string inputTerminalBindingId;
            [Tooltip("Declared runtime output terminal ID; optional for components without terminals.")]
            public string outputTerminalBindingId;
            [Tooltip("Optional declared runtime SparkElectronicObject ID.")]
            public string electronicObjectBindingId;

            [Header("Object discovery within installed SparkShelfItem")]
            [Tooltip("Match a unique component type name or full name; blank means any SparkElectricalComponent, which must be unique.")]
            public string electricalTypeName = "SparkResistor";
            [Tooltip("Match a unique component type name or full name; blank means any SparkElectronicObject. Used only when Electronic Object ID is set.")]
            public string electronicTypeName = "";
            [Tooltip("Exact terminal GameObject name, or suffix if Exact Terminal Names is disabled.")]
            public string inputTerminalName = "_Input";
            public string outputTerminalName = "_Output";
            public bool exactTerminalNames = false;

            [NonSerialized] internal SparkLevelSceneBindings appliedTo;
            [NonSerialized] internal SparkShelfSlot appliedSlot;
            [NonSerialized] internal SparkShelfItem appliedItem;
            [NonSerialized] internal SparkElectricalComponent appliedComponent;
            [NonSerialized] internal SparkTerminal appliedInput;
            [NonSerialized] internal SparkTerminal appliedOutput;
            [NonSerialized] internal SparkElectronicObject appliedElectronic;
            [NonSerialized] internal bool initialized;
            [NonSerialized] internal string lastError;
        }

        [Header("Existing Systems")]
        [SerializeField] private SparkLevelLayoutLoader layoutLoader;
        [SerializeField] private SparkObjectShelf shelf;
        [Header("Level-specific mapping (no level code required)")]
        [SerializeField] private WorkstationBinding[] workstations = Array.Empty<WorkstationBinding>();
        [Header("Reliability")]
        [SerializeField, Min(0.1f)] private float resyncSeconds = 0.25f;
        [SerializeField] private bool logChanges = true;

        private SparkLevelLayout layout;
        private LevelGamePlayManager gameplay;
        private bool subscribed;
        private float nextSync;

        private void Awake() { layout = GetComponentInParent<SparkLevelLayout>(); Resolve(); }
        private void OnEnable() { Resolve(); Subscribe(); Synchronize(); }
        private void Start() { Resolve(); Subscribe(); Synchronize(); }
        private void LateUpdate()
        {
            if (Time.unscaledTime < nextSync) return;
            nextSync = Time.unscaledTime + Mathf.Max(0.1f, resyncSeconds);
            Resolve(); Subscribe(); Synchronize();
        }
        private void OnDisable() { Unsubscribe(); ClearApplied(); }
        private void OnDestroy() { Unsubscribe(); ClearApplied(); }

        private void Resolve()
        {
            if (layout == null) layout = GetComponentInParent<SparkLevelLayout>();
            if (layoutLoader == null) layoutLoader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (shelf == null) shelf = FindFirstObjectByType<SparkObjectShelf>();
            if (gameplay == null) gameplay = FindFirstObjectByType<LevelGamePlayManager>();
        }
        private void Subscribe()
        {
            if (subscribed || shelf == null) return;
            shelf.ItemPlaced += OnItemPlaced;
            subscribed = true;
        }
        private void Unsubscribe()
        {
            if (!subscribed) return;
            if (shelf != null) shelf.ItemPlaced -= OnItemPlaced;
            subscribed = false;
        }
        private void OnItemPlaced(SparkShelfItem item, SparkShelfSlot slot) => Synchronize();

        [ContextMenu("Project Spark / Synchronize Workstation Bindings")]
        public void Synchronize()
        {
            if (!Application.isPlaying) return;
            if (layout == null || layoutLoader == null || shelf == null ||
                layoutLoader.ActiveLayout != layout) return;

            SparkLevelSceneBindings bindings = layoutLoader.ActiveBindings;
            if (bindings == null) return;

            if (workstations == null) return;
            // Reject ambiguous ownership of the same logical ID. Two slots must
            // never compete to write one binding in the same level.
            var owners = new HashSet<string>(StringComparer.Ordinal);
            foreach (WorkstationBinding rule in workstations)
            {
                if (rule == null) continue;
                if (!Claim(owners, "C:", rule.componentBindingId) ||
                    !Claim(owners, "T:", rule.inputTerminalBindingId) ||
                    !Claim(owners, "T:", rule.outputTerminalBindingId) ||
                    !Claim(owners, "E:", rule.electronicObjectBindingId))
                {
                    Debug.LogError("[Workstation Bindings] Duplicate runtime binding ID across workstation rules. Each alias must have exactly one owner.", this);
                    return;
                }
            }
            bool changed = false;
            foreach (WorkstationBinding rule in workstations)
            {
                if (rule == null) continue;
                changed |= SynchronizeOne(rule, bindings);
            }
            // Changing the object behind an objective must re-evaluate even
            // when the electrical graph/topology itself has not changed.
            if (changed && gameplay != null)
                gameplay.QueueEvaluation();
        }

        private static bool Claim(HashSet<string> claimed, string kind, string id)
        {
            return string.IsNullOrWhiteSpace(id) || claimed.Add(kind + id);
        }

        private bool SynchronizeOne(WorkstationBinding rule, SparkLevelSceneBindings bindings)
        {
            string problem = ValidateRule(rule, bindings);
            if (problem != null)
            {
                ReportOnce(rule, problem);
                return false;
            }

            SparkShelfSlot slot = FindUniqueSlot(rule.slotId, out string slotError);
            if (slotError != null) ReportOnce(rule, slotError);
            SparkShelfItem item = ResolveInstalledItem(slot, out string itemState);

            SparkElectricalComponent component = null;
            SparkTerminal input = null;
            SparkTerminal output = null;
            SparkElectronicObject electronic = null;
            if (item != null)
            {
                if (!string.IsNullOrWhiteSpace(rule.componentBindingId))
                    component = UniqueComponent<SparkElectricalComponent>(item, rule.electricalTypeName);
                if (!string.IsNullOrWhiteSpace(rule.inputTerminalBindingId))
                    input = UniqueTerminal(item, rule.inputTerminalName, rule.exactTerminalNames);
                if (!string.IsNullOrWhiteSpace(rule.outputTerminalBindingId))
                    output = UniqueTerminal(item, rule.outputTerminalName, rule.exactTerminalNames);
                if (!string.IsNullOrWhiteSpace(rule.electronicObjectBindingId))
                    electronic = UniqueComponent<SparkElectronicObject>(item, rule.electronicTypeName);

                if ((!string.IsNullOrWhiteSpace(rule.componentBindingId) && component == null) ||
                    (!string.IsNullOrWhiteSpace(rule.inputTerminalBindingId) && input == null) ||
                    (!string.IsNullOrWhiteSpace(rule.outputTerminalBindingId) && output == null) ||
                    (!string.IsNullOrWhiteSpace(rule.electronicObjectBindingId) && electronic == null) ||
                    (input != null && input == output))
                {
                    ReportOnce(rule, "Installed item '" + item.name + "' does not uniquely match configured component/terminal roles; masking binding until fixed.");
                    component = null; input = null; output = null; electronic = null;
                }
            }

            bool changed = !rule.initialized || rule.appliedTo != bindings ||
                           rule.appliedSlot != slot || rule.appliedItem != item ||
                           rule.appliedComponent != component || rule.appliedInput != input ||
                           rule.appliedOutput != output || rule.appliedElectronic != electronic;
            if (!changed) return false;

            // Only clear the old instance when switching layouts. Never clear other rules.
            if (rule.initialized && rule.appliedTo != null && rule.appliedTo != bindings)
                Mask(rule, rule.appliedTo);

            // Null is deliberate: it blocks fallthrough to retired/serialized objects.
            Apply(rule, bindings, component, input, output, electronic);
            rule.appliedTo = bindings;
            rule.appliedSlot = slot;
            rule.appliedItem = item;
            rule.appliedComponent = component;
            rule.appliedInput = input;
            rule.appliedOutput = output;
            rule.appliedElectronic = electronic;
            rule.initialized = true;
            rule.lastError = null;
            if (logChanges)
                Debug.Log("[Workstation Bindings] " + layout.LevelDefinition.LevelId +
                          " | slot=" + rule.slotId + " | item=" +
                          (item != null ? item.name : "EMPTY") + " | source=" +
                          itemState + " | bound=" +
                          (component != null ? component.name : electronic != null ? electronic.name : "NONE"), this);
            return true;
        }

        /// <summary>
        /// Shelf occupancy is authoritative when consistent. For existing
        /// scene-authored workstation setups, recover only a SINGLE settled
        /// item that positively belongs to the slot. Never mutate shelf data,
        /// never bind items in motion, and never accept an item assigned to a
        /// different slot.
        /// </summary>
        private SparkShelfItem ResolveInstalledItem(SparkShelfSlot slot, out string source)
        {
            if (slot == null)
            {
                source = "SLOT_MISSING";
                return null;
            }

            SparkShelfItem occupant = slot.Occupant;
            if (occupant != null)
            {
                if (occupant.IsMoving)
                {
                    source = "RESERVED_BUT_MOVING";
                    return null;
                }
                if (occupant.CurrentSlot != slot)
                {
                    source = "OCCUPANT_CURRENT_SLOT_MISMATCH";
                    return null;
                }
                if (!occupant.IsAtWorkbench || !occupant.gameObject.activeInHierarchy)
                {
                    source = "OCCUPANT_NOT_READY_AT_WORKBENCH";
                    return null;
                }
                source = "SLOT_OCCUPANT";
                return occupant;
            }

            // Recovery A: some shelf placement paths establish CurrentSlot but
            // fail to populate Slot.Occupant. Require a unique registered item.
            SparkShelfItem byCurrentSlot = null;
            if (shelf != null && shelf.RegisteredItems != null)
            {
                foreach (SparkShelfItem candidate in shelf.RegisteredItems)
                {
                    if (candidate == null || candidate.CurrentSlot != slot ||
                        candidate.IsMoving || !candidate.IsAtWorkbench ||
                        !candidate.gameObject.activeInHierarchy)
                        continue;

                    if (byCurrentSlot != null && byCurrentSlot != candidate)
                    {
                        source = "AMBIGUOUS_REGISTERED_ITEMS";
                        return null;
                    }
                    byCurrentSlot = candidate;
                }
            }
            if (byCurrentSlot != null)
            {
                source = "RECOVERED_FROM_ITEM_CURRENT_SLOT";
                return byCurrentSlot;
            }

            // Recovery B: an item was authored/placed directly under its slot
            // in the persistent shelf hierarchy. Only accept a unique, active,
            // stationary workstation item with no conflicting CurrentSlot.
            SparkShelfItem nested = null;
            bool wrongSlot = false;
            bool moving = false;
            bool notReady = false;
            foreach (SparkShelfItem candidate in slot.GetComponentsInChildren<SparkShelfItem>(true))
            {
                if (candidate == null) continue;
                if (candidate.IsMoving) { moving = true; continue; }
                if (!candidate.IsAtWorkbench || !candidate.gameObject.activeInHierarchy)
                { notReady = true; continue; }
                if (candidate.CurrentSlot != null && candidate.CurrentSlot != slot)
                { wrongSlot = true; continue; }
                if (nested != null && nested != candidate)
                {
                    source = "AMBIGUOUS_CHILD_ITEMS";
                    return null;
                }
                nested = candidate;
            }
            if (nested != null)
            {
                // Do not conceal a shelf-state bug by changing Occupant or
                // CurrentSlot here. This read-only lookup only publishes IDs.
                source = "RECOVERED_FROM_SLOT_CHILD";
                return nested;
            }
            source = wrongSlot ? "CHILD_ASSIGNED_TO_ANOTHER_SLOT" :
                     moving ? "CHILD_STILL_MOVING" :
                     notReady ? "CHILD_NOT_AT_WORKBENCH" :
                     "NO_SHELF_ITEM_AT_SLOT";
            return null;
        }

        /// <summary>
        /// The Inspector's manual button calls this rather than the silent
        /// auto-sync. It ALWAYS prints the reason for binding or non-binding.
        /// </summary>
        [ContextMenu("Project Spark / Synchronize AND REPORT Workstation Bindings")]
        public void SynchronizeNow()
        {
            Resolve();
            Subscribe();
            Synchronize();
            LogDiagnostics();
        }

        [ContextMenu("Project Spark / Diagnose Workstation Bindings")]
        public void LogDiagnostics()
        {
            Resolve();
            var report = new StringBuilder(2048);
            report.AppendLine("========== PROJECT SPARK / WORKSTATION BINDING DIAGNOSTIC ==========");
            report.AppendLine("Playing: " + Application.isPlaying);
            report.AppendLine("Layout component: " + (layout != null ? layout.name : "MISSING"));
            report.AppendLine("Active layout matches: " + (layoutLoader != null && layoutLoader.ActiveLayout == layout));
            report.AppendLine("Shelf: " + (shelf != null ? shelf.name : "MISSING"));
            SparkLevelSceneBindings bindings = layoutLoader != null ? layoutLoader.ActiveBindings : null;
            report.AppendLine("Primary bindings: " + (bindings != null ? bindings.name : "MISSING"));
            if (workstations != null)
            {
                foreach (WorkstationBinding rule in workstations)
                {
                    if (rule == null) continue;
                    report.AppendLine("--- Slot ID: " + rule.slotId + " ---");
                    if (shelf == null) continue;
                    SparkShelfSlot slot = FindUniqueSlot(rule.slotId, out string error);
                    report.AppendLine("Slot lookup: " + (slot != null ? "FOUND" : "NOT FOUND") +
                                      (error != null ? " / " + error : ""));
                    if (slot != null)
                    {
                        report.AppendLine("Slot path: " + HierarchyPath(slot.transform));
                        report.AppendLine("Slot.Occupant: " + Describe(slot.Occupant));
                        if (slot.Occupant != null)
                            report.AppendLine("Occupant.CurrentSlot: " +
                                (slot.Occupant.CurrentSlot != null ? slot.Occupant.CurrentSlot.SlotId : "NULL"));
                        SparkShelfItem selected = ResolveInstalledItem(slot, out string source);
                        report.AppendLine("Chosen item: " + Describe(selected) + " / " + source);
                        foreach (SparkShelfItem child in slot.GetComponentsInChildren<SparkShelfItem>(true))
                        {
                            if (child == null) continue;
                            report.AppendLine("Child: " + Describe(child) +
                                " | IsMoving=" + child.IsMoving +
                                " | IsAtWorkbench=" + child.IsAtWorkbench +
                                " | CurrentSlot=" + (child.CurrentSlot != null ? child.CurrentSlot.SlotId : "NULL"));
                        }
                    }
                    if (bindings != null)
                    {
                        report.AppendLine("Rule: " + (ValidateRule(rule, bindings) ?? "VALID"));
                        if (!string.IsNullOrWhiteSpace(rule.componentBindingId))
                        {
                            SparkElectricalComponent comp;
                            bool ok = bindings.TryGetComponent(rule.componentBindingId, out comp);
                            report.AppendLine(rule.componentBindingId + " => " + (ok ? Describe(comp) : "UNBOUND"));
                        }
                        if (!string.IsNullOrWhiteSpace(rule.inputTerminalBindingId))
                        {
                            SparkTerminal input;
                            bool ok = bindings.TryGetTerminal(rule.inputTerminalBindingId, out input);
                            report.AppendLine(rule.inputTerminalBindingId + " => " + (ok ? Describe(input) : "UNBOUND"));
                        }
                        if (!string.IsNullOrWhiteSpace(rule.outputTerminalBindingId))
                        {
                            SparkTerminal output;
                            bool ok = bindings.TryGetTerminal(rule.outputTerminalBindingId, out output);
                            report.AppendLine(rule.outputTerminalBindingId + " => " + (ok ? Describe(output) : "UNBOUND"));
                        }
                    }
                }
            }
            Debug.Log(report.ToString(), this);
        }

        private static string Describe(Component c) => c != null ? HierarchyPath(c.transform) : "NULL";
        private static string HierarchyPath(Transform t)
        {
            if (t == null) return "<destroyed>";
            string path = t.name;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }

        private string ValidateRule(WorkstationBinding rule, SparkLevelSceneBindings bindings)
        {
            if (string.IsNullOrWhiteSpace(rule.slotId)) return "Workstation Slot ID is empty.";
            if (string.IsNullOrWhiteSpace(rule.componentBindingId) &&
                string.IsNullOrWhiteSpace(rule.inputTerminalBindingId) &&
                string.IsNullOrWhiteSpace(rule.outputTerminalBindingId) &&
                string.IsNullOrWhiteSpace(rule.electronicObjectBindingId))
                return "Rule has no binding IDs.";
            if (!string.IsNullOrWhiteSpace(rule.componentBindingId) && !bindings.IsComponentIdDeclared(rule.componentBindingId))
                return "Component ID not declared on primary layout: " + rule.componentBindingId;
            if (!string.IsNullOrWhiteSpace(rule.inputTerminalBindingId) && !bindings.IsTerminalIdDeclared(rule.inputTerminalBindingId))
                return "Input terminal ID not declared: " + rule.inputTerminalBindingId;
            if (!string.IsNullOrWhiteSpace(rule.outputTerminalBindingId) && !bindings.IsTerminalIdDeclared(rule.outputTerminalBindingId))
                return "Output terminal ID not declared: " + rule.outputTerminalBindingId;
            if (!string.IsNullOrWhiteSpace(rule.electronicObjectBindingId) && !bindings.IsElectronicObjectIdDeclared(rule.electronicObjectBindingId))
                return "Electronic object ID not declared: " + rule.electronicObjectBindingId;
            return null;
        }

        private SparkShelfSlot FindUniqueSlot(string id, out string error)
        {
            error = null;
            SparkShelfSlot result = null;
            IReadOnlyList<SparkShelfSlot> all = shelf.RegisteredSlots;
            if (all == null) return null;
            foreach (SparkShelfSlot slot in all)
            {
                if (slot == null || !slot.isActiveAndEnabled ||
                    slot.Kind != SparkShelfSlotKind.Workstation ||
                    !string.Equals(slot.SlotId, id, StringComparison.Ordinal)) continue;
                if (result != null && result != slot)
                {
                    error = "Ambiguous active workstation SlotId '" + id + "'. Make IDs unique; binding masked.";
                    return null;
                }
                result = slot;
            }
            if (result == null)
                error = "No active registered workstation slot with SlotId '" + id + "'.";
            return result;
        }

        private static T UniqueComponent<T>(SparkShelfItem item, string typeName) where T : Component
        {
            T found = null;
            foreach (T component in item.GetComponentsInChildren<T>(true))
            {
                if (component == null) continue;
                Type type = component.GetType();
                if (!string.IsNullOrWhiteSpace(typeName) &&
                    !string.Equals(type.Name, typeName, StringComparison.Ordinal) &&
                    !string.Equals(type.FullName, typeName, StringComparison.Ordinal)) continue;
                if (found != null) return null;
                found = component;
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
                bool matches = exact
                    ? string.Equals(name, match, StringComparison.Ordinal)
                    : name.EndsWith(match, StringComparison.OrdinalIgnoreCase);
                if (!matches) continue;
                if (found != null) return null;
                found = terminal;
            }
            return found;
        }
        private static void Apply(WorkstationBinding rule, SparkLevelSceneBindings bindings,
            SparkElectricalComponent component, SparkTerminal input, SparkTerminal output,
            SparkElectronicObject electronic)
        {
            if (!string.IsNullOrWhiteSpace(rule.componentBindingId))
                bindings.SetRuntimeComponentBinding(rule.componentBindingId, component);
            if (!string.IsNullOrWhiteSpace(rule.inputTerminalBindingId))
                bindings.SetRuntimeTerminal(rule.inputTerminalBindingId, input);
            if (!string.IsNullOrWhiteSpace(rule.outputTerminalBindingId))
                bindings.SetRuntimeTerminal(rule.outputTerminalBindingId, output);
            if (!string.IsNullOrWhiteSpace(rule.electronicObjectBindingId))
                bindings.SetRuntimeElectronicObject(rule.electronicObjectBindingId, electronic);
        }
        private static void Mask(WorkstationBinding rule, SparkLevelSceneBindings bindings) =>
            Apply(rule, bindings, null, null, null, null);
        private void ClearApplied()
        {
            if (workstations == null) return;
            foreach (WorkstationBinding rule in workstations)
            {
                if (rule == null || !rule.initialized) continue;
                if (rule.appliedTo != null) Mask(rule, rule.appliedTo);
                rule.appliedTo = null; rule.appliedSlot = null; rule.appliedItem = null;
                rule.appliedComponent = null; rule.appliedInput = null;
                rule.appliedOutput = null; rule.appliedElectronic = null;
                rule.initialized = false;
            }
        }
        private void ReportOnce(WorkstationBinding rule, string message)
        {
            if (string.Equals(rule.lastError, message, StringComparison.Ordinal)) return;
            rule.lastError = message;
            Debug.LogWarning("[Workstation Bindings] " + message, this);
        }
    }
}

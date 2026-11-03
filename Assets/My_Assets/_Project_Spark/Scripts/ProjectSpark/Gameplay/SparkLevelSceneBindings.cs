using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// One authoritative binding table per level layout.
    /// Fixed objects are serialized scene references; replaceable objects are
    /// declared as runtime IDs without fragile references to disposable prefabs.
    /// No search of unrelated scene bindings, no implicit hierarchy name binding.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelSceneBindings : MonoBehaviour
    {
        [Serializable] private sealed class TerminalBinding
        {
            [SerializeField] private string id;
            [SerializeField] private SparkTerminal terminal;
            public string Id => id;
            public SparkTerminal Terminal => terminal;
        }
        [Serializable] private sealed class ComponentBinding
        {
            [SerializeField] private string id;
            [SerializeField] private SparkElectricalComponent component;
            public string Id => id;
            public SparkElectricalComponent Component => component;
        }
        [Serializable] private sealed class ElectronicObjectBinding
        {
            [SerializeField] private string id;
            [SerializeField] private SparkElectronicObject electronicObject;
            public string Id => id;
            public SparkElectronicObject ElectronicObject => electronicObject;
        }
        [Serializable] private sealed class GameObjectBinding
        {
            [SerializeField] private string id;
            [SerializeField] private GameObject target;
            public string Id => id;
            public GameObject Target => target;
        }

        // Serialized names unchanged: compatible with your existing custom Inspector.
        [Header("Terminal Bindings — Fixed Scene Objects")]
        [SerializeField] private TerminalBinding[] terminals = Array.Empty<TerminalBinding>();
        [Header("Electrical Component Bindings — Fixed Scene Objects")]
        [SerializeField] private ComponentBinding[] components = Array.Empty<ComponentBinding>();
        [Header("Electronic Object Bindings — Fixed Scene Objects")]
        [SerializeField] private ElectronicObjectBinding[] electronicObjects = Array.Empty<ElectronicObjectBinding>();
        [Header("GameObject Output Bindings")]
        [SerializeField] private GameObjectBinding[] outputs = Array.Empty<GameObjectBinding>();

        [Header("Runtime IDs — Replaceable Workstation Objects")]
        [Tooltip("Stable terminal IDs, declared without scene references (e.g. R2_Input, R2_Output).")]
        [SerializeField] private string[] runtimeTerminalIds = Array.Empty<string>();
        [Tooltip("Stable electrical component IDs (e.g. R2_Component).")]
        [SerializeField] private string[] runtimeComponentIds = Array.Empty<string>();
        [Tooltip("Optional stable electronic-object IDs for replaceable instruments.")]
        [SerializeField] private string[] runtimeElectronicObjectIds = Array.Empty<string>();

        private Dictionary<string, SparkTerminal> terminalLookup;
        private Dictionary<string, SparkElectricalComponent> componentLookup;
        private Dictionary<string, SparkElectronicObject> electronicObjectLookup;
        private Dictionary<string, GameObject> outputLookup;
        private HashSet<string> declaredRuntimeTerminals;
        private HashSet<string> declaredRuntimeComponents;
        private HashSet<string> declaredRuntimeElectronicObjects;
        private bool lookupCacheBuilt;

        private readonly Dictionary<string, SparkTerminal> runtimeTerminalOverrides =
            new Dictionary<string, SparkTerminal>(StringComparer.Ordinal);
        private readonly Dictionary<string, SparkElectricalComponent> runtimeComponentOverrides =
            new Dictionary<string, SparkElectricalComponent>(StringComparer.Ordinal);
        private readonly Dictionary<string, SparkElectronicObject> runtimeElectronicOverrides =
            new Dictionary<string, SparkElectronicObject>(StringComparer.Ordinal);
        private readonly HashSet<string> retiredTerminalIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> retiredComponentIds = new HashSet<string>(StringComparer.Ordinal);

        private void Awake() => BuildLookupCache();
        private void OnEnable() => BuildLookupCache();
#if UNITY_EDITOR
        private void OnValidate() { lookupCacheBuilt = false; }
#endif
        private void OnDestroy()
        {
            runtimeTerminalOverrides.Clear(); runtimeComponentOverrides.Clear();
            runtimeElectronicOverrides.Clear(); retiredTerminalIds.Clear(); retiredComponentIds.Clear();
        }

        private static HashSet<string> Collect(string[] source)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            if (source != null)
                foreach (string id in source)
                    if (!string.IsNullOrWhiteSpace(id)) set.Add(id);
            return set;
        }
        private void BuildLookupCache()
        {
            terminalLookup = new Dictionary<string, SparkTerminal>(StringComparer.Ordinal);
            componentLookup = new Dictionary<string, SparkElectricalComponent>(StringComparer.Ordinal);
            electronicObjectLookup = new Dictionary<string, SparkElectronicObject>(StringComparer.Ordinal);
            outputLookup = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            declaredRuntimeTerminals = Collect(runtimeTerminalIds);
            declaredRuntimeComponents = Collect(runtimeComponentIds);
            declaredRuntimeElectronicObjects = Collect(runtimeElectronicObjectIds);
            if (terminals != null) foreach (TerminalBinding binding in terminals)
                if (binding != null && !string.IsNullOrWhiteSpace(binding.Id) && !terminalLookup.ContainsKey(binding.Id))
                    terminalLookup.Add(binding.Id, binding.Terminal);
            if (components != null) foreach (ComponentBinding binding in components)
                if (binding != null && !string.IsNullOrWhiteSpace(binding.Id) && !componentLookup.ContainsKey(binding.Id))
                    componentLookup.Add(binding.Id, binding.Component);
            if (electronicObjects != null) foreach (ElectronicObjectBinding binding in electronicObjects)
                if (binding != null && !string.IsNullOrWhiteSpace(binding.Id) && !electronicObjectLookup.ContainsKey(binding.Id))
                    electronicObjectLookup.Add(binding.Id, binding.ElectronicObject);
            if (outputs != null) foreach (GameObjectBinding binding in outputs)
                if (binding != null && !string.IsNullOrWhiteSpace(binding.Id) && !outputLookup.ContainsKey(binding.Id))
                    outputLookup.Add(binding.Id, binding.Target);
            lookupCacheBuilt = true;
        }
        private void EnsureLookupCache()
        {
            if (!lookupCacheBuilt || terminalLookup == null || componentLookup == null ||
                electronicObjectLookup == null || outputLookup == null || declaredRuntimeTerminals == null)
                BuildLookupCache();
        }

        public bool IsTerminalIdDeclared(string id)
        {
            EnsureLookupCache();
            return !string.IsNullOrWhiteSpace(id) &&
                (declaredRuntimeTerminals.Contains(id) || terminalLookup.ContainsKey(id));
        }
        public bool IsComponentIdDeclared(string id)
        {
            EnsureLookupCache();
            return !string.IsNullOrWhiteSpace(id) &&
                (declaredRuntimeComponents.Contains(id) || componentLookup.ContainsKey(id));
        }
        public bool IsElectronicObjectIdDeclared(string id)
        {
            EnsureLookupCache();
            return !string.IsNullOrWhiteSpace(id) &&
                (declaredRuntimeElectronicObjects.Contains(id) || electronicObjectLookup.ContainsKey(id));
        }

        public bool TryGetTerminal(string id, out SparkTerminal terminal)
        {
            EnsureLookupCache(); terminal = null;
            if (string.IsNullOrWhiteSpace(id)) return false;
            if (runtimeTerminalOverrides.TryGetValue(id, out terminal)) return terminal != null;
            // A declared runtime ID is intentionally empty until a slot is populated.
            if (declaredRuntimeTerminals.Contains(id) || retiredTerminalIds.Contains(id)) return false;
            return terminalLookup.TryGetValue(id, out terminal) && terminal != null;
        }
        public bool HasTerminal(string id) => TryGetTerminal(id, out _);

        public bool TryGetComponent(string id, out SparkElectricalComponent component)
        {
            EnsureLookupCache(); component = null;
            if (string.IsNullOrWhiteSpace(id)) return false;
            if (runtimeComponentOverrides.TryGetValue(id, out component)) return component != null;
            if (declaredRuntimeComponents.Contains(id) || retiredComponentIds.Contains(id)) return false;
            return componentLookup.TryGetValue(id, out component) && component != null;
        }
        public bool HasComponent(string id) => TryGetComponent(id, out _);

        public bool TryGetElectronicObject(string id, out SparkElectronicObject electronicObject)
        {
            EnsureLookupCache(); electronicObject = null;
            if (string.IsNullOrWhiteSpace(id)) return false;
            if (runtimeElectronicOverrides.TryGetValue(id, out electronicObject)) return electronicObject != null;
            if (declaredRuntimeElectronicObjects.Contains(id)) return false;
            return electronicObjectLookup.TryGetValue(id, out electronicObject) && electronicObject != null;
        }
        public bool HasElectronicObject(string id) => TryGetElectronicObject(id, out _);

        public bool TryGetObject(string id, out GameObject target)
        {
            EnsureLookupCache(); target = null;
            return !string.IsNullOrWhiteSpace(id) && outputLookup.TryGetValue(id, out target) && target != null;
        }
        public bool HasObject(string id) => TryGetObject(id, out _);

        // Compatibility with older SparkLevelLayout builds that call this method.
        // Deliberately does NOT merge lookups: the primary layout remains authoritative.
        public void SetAdditionalBindingSources(IReadOnlyList<SparkLevelSceneBindings> sources)
        {
            // Scene-wide bindings may be displayed for discovery, not used by level targets.
        }

        // Generic future-proof runtime binding API. Null masks a removed item.
        public bool SetRuntimeTerminal(string id, SparkTerminal terminal)
        {
            if (!IsTerminalIdDeclared(id)) return false;
            runtimeTerminalOverrides[id] = terminal; return true;
        }
        public bool SetRuntimeComponentBinding(string id, SparkElectricalComponent component)
        {
            if (!IsComponentIdDeclared(id)) return false;
            runtimeComponentOverrides[id] = component; return true;
        }
        public bool SetRuntimeElectronicObject(string id, SparkElectronicObject electronicObject)
        {
            if (!IsElectronicObjectIdDeclared(id)) return false;
            runtimeElectronicOverrides[id] = electronicObject; return true;
        }
        public bool RegisterRuntimeComponent(string id, SparkElectricalComponent component) =>
            component != null && SetRuntimeComponentBinding(id, component);
        public bool ClearRuntimeComponentBinding(string id) => SetRuntimeComponentBinding(id, null);
        public bool ClearRuntimeElectronicObjectBinding(string id) => SetRuntimeElectronicObject(id, null);
        public bool UnregisterRuntimeComponent(string id, SparkElectricalComponent expectedComponent)
        {
            if (string.IsNullOrWhiteSpace(id) ||
                !runtimeComponentOverrides.TryGetValue(id, out SparkElectricalComponent actual) ||
                actual != expectedComponent) return false;
            runtimeComponentOverrides[id] = null; return true;
        }
        public void ClearRuntimeComponentOverrides() => runtimeComponentOverrides.Clear();

        /// <summary>Compatibility for the existing recycle flow. Only runtime instance refs are retired.</summary>
        public void RetireRuntimeBindingsOwnedBy(Transform objectRoot)
        {
            if (objectRoot == null) return;
            EnsureLookupCache();
            if (terminals != null) foreach (TerminalBinding entry in terminals)
                if (entry != null && !string.IsNullOrWhiteSpace(entry.Id) && entry.Terminal != null &&
                    (entry.Terminal.transform == objectRoot || entry.Terminal.transform.IsChildOf(objectRoot)))
                { retiredTerminalIds.Add(entry.Id); runtimeTerminalOverrides[entry.Id] = null; }
            if (components != null) foreach (ComponentBinding entry in components)
                if (entry != null && !string.IsNullOrWhiteSpace(entry.Id) && entry.Component != null &&
                    (entry.Component.transform == objectRoot || entry.Component.transform.IsChildOf(objectRoot)))
                { retiredComponentIds.Add(entry.Id); runtimeComponentOverrides[entry.Id] = null; }
        }

        private static bool ValidateIds(IEnumerable<string> values, HashSet<string> ids, string kind, out string error)
        {
            foreach (string id in values)
            {
                if (string.IsNullOrWhiteSpace(id)) { error = kind + " has an empty binding ID."; return false; }
                if (!ids.Add(id)) { error = kind + " has a duplicate ID '" + id + "'."; return false; }
            }
            error = string.Empty; return true;
        }
        public bool Validate(out string error)
        {
            if (!ValidateTerminalBindings(out error)) return false;
            if (!ValidateComponentBindings(out error)) return false;
            if (!ValidateElectronicObjectBindings(out error)) return false;
            if (!ValidateOutputBindings(out error)) return false;
            return true;
        }
        private bool ValidateTerminalBindings(out string error)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (terminals != null) foreach (TerminalBinding binding in terminals)
            {
                if (binding == null) { error = "Null terminal binding."; return false; }
                if (string.IsNullOrWhiteSpace(binding.Id) || !ids.Add(binding.Id))
                { error = "Invalid/duplicate terminal binding ID: '" + binding.Id + "'."; return false; }
                if (binding.Terminal == null && !retiredTerminalIds.Contains(binding.Id))
                { error = "Terminal binding '" + binding.Id + "' has no SparkTerminal assigned."; return false; }
            }
            return ValidateIds(runtimeTerminalIds ?? Array.Empty<string>(), ids, "Runtime terminal", out error);
        }
        private bool ValidateComponentBindings(out string error)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (components != null) foreach (ComponentBinding binding in components)
            {
                if (binding == null) { error = "Null electrical component binding."; return false; }
                if (string.IsNullOrWhiteSpace(binding.Id) || !ids.Add(binding.Id))
                { error = "Invalid/duplicate component binding ID: '" + binding.Id + "'."; return false; }
                if (binding.Component == null && !retiredComponentIds.Contains(binding.Id))
                { error = "Electrical component binding '" + binding.Id + "' has no SparkElectricalComponent assigned."; return false; }
            }
            return ValidateIds(runtimeComponentIds ?? Array.Empty<string>(), ids, "Runtime component", out error);
        }
        private bool ValidateElectronicObjectBindings(out string error)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (electronicObjects != null) foreach (ElectronicObjectBinding binding in electronicObjects)
            {
                if (binding == null) { error = "Null electronic object binding."; return false; }
                if (string.IsNullOrWhiteSpace(binding.Id) || !ids.Add(binding.Id))
                { error = "Invalid/duplicate electronic object binding ID: '" + binding.Id + "'."; return false; }
                if (binding.ElectronicObject == null)
                { error = "Electronic object binding '" + binding.Id + "' has no SparkElectronicObject assigned."; return false; }
            }
            return ValidateIds(runtimeElectronicObjectIds ?? Array.Empty<string>(), ids, "Runtime electronic object", out error);
        }
        private bool ValidateOutputBindings(out string error)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (outputs != null) foreach (GameObjectBinding binding in outputs)
            {
                if (binding == null) { error = "Null GameObject output binding."; return false; }
                if (string.IsNullOrWhiteSpace(binding.Id) || !ids.Add(binding.Id))
                { error = "Invalid/duplicate output binding ID: '" + binding.Id + "'."; return false; }
                if (binding.Target == null)
                { error = "GameObject output binding '" + binding.Id + "' has no GameObject assigned."; return false; }
            }
            error = string.Empty; return true;
        }
        [ContextMenu("Validate Scene Bindings")]
        private void ValidateSceneBindings()
        {
            if (Validate(out string error)) Debug.Log("[SceneBindings] Valid.", this);
            else Debug.LogError("[SceneBindings] " + error, this);
        }
        public int TerminalBindingCount => terminals != null ? terminals.Length : 0;
        public int ComponentBindingCount => components != null ? components.Length : 0;
        public int ElectronicObjectBindingCount => electronicObjects != null ? electronicObjects.Length : 0;
        public int OutputBindingCount => outputs != null ? outputs.Length : 0;
    }
}

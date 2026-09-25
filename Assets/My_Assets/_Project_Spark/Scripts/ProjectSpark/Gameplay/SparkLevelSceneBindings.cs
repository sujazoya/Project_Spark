using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevelSceneBindings : MonoBehaviour
    {
        // ============================================================
        // TERMINAL BINDING
        // ============================================================

        [Serializable]
        private sealed class TerminalBinding
        {
            [SerializeField] private string id;
            [SerializeField] private SparkTerminal terminal;

            public string Id => id;
            public SparkTerminal Terminal => terminal;
        }

        // ============================================================
        // ELECTRICAL COMPONENT BINDING
        // Existing system - PRESERVED
        // ============================================================

        [Serializable]
        private sealed class ComponentBinding
        {
            [SerializeField] private string id;
            [SerializeField] private SparkElectricalComponent component;

            public string Id => id;
            public SparkElectricalComponent Component => component;
        }

        // ============================================================
        // ELECTRONIC OBJECT BINDING
        // New path for instruments such as SparkMultimeter
        // ============================================================

        [Serializable]
        private sealed class ElectronicObjectBinding
        {
            [SerializeField] private string id;
            [SerializeField] private SparkElectronicObject electronicObject;

            public string Id => id;
            public SparkElectronicObject ElectronicObject => electronicObject;
        }

        // ============================================================
        // GAMEOBJECT OUTPUT BINDING
        // ============================================================

        [Serializable]
        private sealed class GameObjectBinding
        {
            [SerializeField] private string id;
            [SerializeField] private GameObject target;

            public string Id => id;
            public GameObject Target => target;
        }

        // ============================================================
        // INSPECTOR DATA
        // ============================================================

        [Header("Terminal Bindings")]
        [SerializeField]
        private TerminalBinding[] terminals = Array.Empty<TerminalBinding>();

        [Header("Electrical Component Bindings")]
        [SerializeField]
        private ComponentBinding[] components = Array.Empty<ComponentBinding>();

        [Header("Electronic Object Bindings")]
        [SerializeField]
        private ElectronicObjectBinding[] electronicObjects =
            Array.Empty<ElectronicObjectBinding>();

        [Header("GameObject Output Bindings")]
        [SerializeField]
        private GameObjectBinding[] outputs = Array.Empty<GameObjectBinding>();

        // ============================================================
        // LOOKUP CACHE
        // ============================================================

        private Dictionary<string, SparkTerminal> terminalLookup;

        private Dictionary<string, SparkElectricalComponent> componentLookup;

        private Dictionary<string, SparkElectronicObject>
            electronicObjectLookup;

        private Dictionary<string, GameObject> outputLookup;

        private bool lookupCacheBuilt;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            BuildLookupCache();
        }

        private void OnEnable()
        {
            BuildLookupCache();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Validate(out _);

            lookupCacheBuilt = false;
        }
#endif

        // ============================================================
        // CACHE
        // ============================================================

        private void BuildLookupCache()
        {
            terminalLookup =
                new Dictionary<string, SparkTerminal>(
                    StringComparer.Ordinal);

            componentLookup =
                new Dictionary<string, SparkElectricalComponent>(
                    StringComparer.Ordinal);

            electronicObjectLookup =
                new Dictionary<string, SparkElectronicObject>(
                    StringComparer.Ordinal);

            outputLookup =
                new Dictionary<string, GameObject>(
                    StringComparer.Ordinal);

            // --------------------------------------------------------
            // TERMINALS
            // --------------------------------------------------------

            if (terminals != null)
            {
                for (int i = 0; i < terminals.Length; i++)
                {
                    TerminalBinding binding = terminals[i];

                    if (binding == null)
                        continue;

                    string id = binding.Id;

                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    SparkTerminal terminal = binding.Terminal;

                    if (terminal == null)
                        continue;

                    if (terminalLookup.ContainsKey(id))
                    {
                        Debug.LogWarning(
                            $"[{nameof(SparkLevelSceneBindings)}] " +
                            $"Duplicate terminal binding ID '{id}'. " +
                            $"The first binding is kept.",
                            this);

                        continue;
                    }

                    terminalLookup.Add(id, terminal);
                }
            }

            // --------------------------------------------------------
            // ELECTRICAL COMPONENTS
            // Existing behavior preserved
            // --------------------------------------------------------

            if (components != null)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    ComponentBinding binding = components[i];

                    if (binding == null)
                        continue;

                    string id = binding.Id;

                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    SparkElectricalComponent component =
                        binding.Component;

                    if (component == null)
                        continue;

                    if (componentLookup.ContainsKey(id))
                    {
                        Debug.LogWarning(
                            $"[{nameof(SparkLevelSceneBindings)}] " +
                            $"Duplicate electrical component binding ID '{id}'. " +
                            $"The first binding is kept.",
                            this);

                        continue;
                    }

                    componentLookup.Add(id, component);
                }
            }

            // --------------------------------------------------------
            // ELECTRONIC OBJECTS
            // Multimeter and other SparkElectronicObject types
            // --------------------------------------------------------

            if (electronicObjects != null)
            {
                for (int i = 0; i < electronicObjects.Length; i++)
                {
                    ElectronicObjectBinding binding =
                        electronicObjects[i];

                    if (binding == null)
                        continue;

                    string id = binding.Id;

                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    SparkElectronicObject electronicObject =
                        binding.ElectronicObject;

                    if (electronicObject == null)
                        continue;

                    if (electronicObjectLookup.ContainsKey(id))
                    {
                        Debug.LogWarning(
                            $"[{nameof(SparkLevelSceneBindings)}] " +
                            $"Duplicate electronic object binding ID '{id}'. " +
                            $"The first binding is kept.",
                            this);

                        continue;
                    }

                    electronicObjectLookup.Add(
                        id,
                        electronicObject);
                }
            }

            // --------------------------------------------------------
            // GAMEOBJECT OUTPUTS
            // --------------------------------------------------------

            if (outputs != null)
            {
                for (int i = 0; i < outputs.Length; i++)
                {
                    GameObjectBinding binding = outputs[i];

                    if (binding == null)
                        continue;

                    string id = binding.Id;

                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    GameObject target = binding.Target;

                    if (target == null)
                        continue;

                    if (outputLookup.ContainsKey(id))
                    {
                        Debug.LogWarning(
                            $"[{nameof(SparkLevelSceneBindings)}] " +
                            $"Duplicate output binding ID '{id}'. " +
                            $"The first binding is kept.",
                            this);

                        continue;
                    }

                    outputLookup.Add(id, target);
                }
            }

            lookupCacheBuilt = true;
        }

        // ============================================================
        // CACHE VALIDATION
        // ============================================================

        private void EnsureLookupCache()
        {
            if (!lookupCacheBuilt ||
                terminalLookup == null ||
                componentLookup == null ||
                electronicObjectLookup == null ||
                outputLookup == null)
            {
                BuildLookupCache();
            }
        }

        // ============================================================
        // TERMINAL API
        // ============================================================

        public bool TryGetTerminal(
            string id,
            out SparkTerminal terminal)
        {
            EnsureLookupCache();

            terminal = null;

            if (string.IsNullOrWhiteSpace(id))
                return false;

            return terminalLookup.TryGetValue(
                id,
                out terminal);
        }

        public bool HasTerminal(string id)
        {
            return TryGetTerminal(
                id,
                out _);
        }

        // ============================================================
        // ELECTRICAL COMPONENT API
        // Existing API preserved
        // ============================================================

        public bool TryGetComponent(
            string id,
            out SparkElectricalComponent component)
        {
            EnsureLookupCache();

            component = null;

            if (string.IsNullOrWhiteSpace(id))
                return false;

            return componentLookup.TryGetValue(
                id,
                out component);
        }

        public bool HasComponent(string id)
        {
            return TryGetComponent(
                id,
                out _);
        }

        // ============================================================
        // ELECTRONIC OBJECT API
        // New API
        // ============================================================

        public bool TryGetElectronicObject(
            string id,
            out SparkElectronicObject electronicObject)
        {
            EnsureLookupCache();

            electronicObject = null;

            if (string.IsNullOrWhiteSpace(id))
                return false;

            return electronicObjectLookup.TryGetValue(
                id,
                out electronicObject);
        }

        public bool HasElectronicObject(string id)
        {
            return TryGetElectronicObject(
                id,
                out _);
        }

        // ============================================================
        // GAMEOBJECT API
        // ============================================================

        public bool TryGetObject(
            string id,
            out GameObject target)
        {
            EnsureLookupCache();

            target = null;

            if (string.IsNullOrWhiteSpace(id))
                return false;

            return outputLookup.TryGetValue(
                id,
                out target);
        }

        public bool HasObject(string id)
        {
            return TryGetObject(
                id,
                out _);
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(out string error)
        {
            error = string.Empty;

            if (!ValidateTerminalBindings(out error))
                return false;

            if (!ValidateComponentBindings(out error))
                return false;

            if (!ValidateElectronicObjectBindings(out error))
                return false;

            if (!ValidateOutputBindings(out error))
                return false;

            return true;
        }

        // ============================================================
        // TERMINAL VALIDATION
        // ============================================================

        private bool ValidateTerminalBindings(
            out string error)
        {
            error = string.Empty;

            if (terminals == null)
                return true;

            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (int i = 0; i < terminals.Length; i++)
            {
                TerminalBinding binding = terminals[i];

                if (binding == null)
                {
                    error =
                        $"Terminal binding at index {i} is null.";

                    return false;
                }

                string id = binding.Id;

                if (string.IsNullOrWhiteSpace(id))
                {
                    error =
                        $"Terminal binding at index {i} " +
                        $"has an empty ID.";

                    return false;
                }

                if (binding.Terminal == null)
                {
                    error =
                        $"Terminal binding '{id}' " +
                        $"has no SparkTerminal assigned.";

                    return false;
                }

                if (!ids.Add(id))
                {
                    error =
                        $"Duplicate terminal binding ID '{id}'.";

                    return false;
                }
            }

            return true;
        }

        // ============================================================
        // ELECTRICAL COMPONENT VALIDATION
        // Existing validation preserved
        // ============================================================

        private bool ValidateComponentBindings(
            out string error)
        {
            error = string.Empty;

            if (components == null)
                return true;

            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (int i = 0; i < components.Length; i++)
            {
                ComponentBinding binding =
                    components[i];

                if (binding == null)
                {
                    error =
                        $"Electrical component binding " +
                        $"at index {i} is null.";

                    return false;
                }

                string id = binding.Id;

                if (string.IsNullOrWhiteSpace(id))
                {
                    error =
                        $"Electrical component binding " +
                        $"at index {i} has an empty ID.";

                    return false;
                }

                if (binding.Component == null)
                {
                    error =
                        $"Electrical component binding '{id}' " +
                        $"has no SparkElectricalComponent assigned.";

                    return false;
                }

                if (!ids.Add(id))
                {
                    error =
                        $"Duplicate electrical component " +
                        $"binding ID '{id}'.";

                    return false;
                }
            }

            return true;
        }

        // ============================================================
        // ELECTRONIC OBJECT VALIDATION
        // ============================================================

        private bool ValidateElectronicObjectBindings(
            out string error)
        {
            error = string.Empty;

            if (electronicObjects == null)
                return true;

            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (int i = 0;
                 i < electronicObjects.Length;
                 i++)
            {
                ElectronicObjectBinding binding =
                    electronicObjects[i];

                if (binding == null)
                {
                    error =
                        $"Electronic object binding " +
                        $"at index {i} is null.";

                    return false;
                }

                string id = binding.Id;

                if (string.IsNullOrWhiteSpace(id))
                {
                    error =
                        $"Electronic object binding " +
                        $"at index {i} has an empty ID.";

                    return false;
                }

                if (binding.ElectronicObject == null)
                {
                    error =
                        $"Electronic object binding '{id}' " +
                        $"has no SparkElectronicObject assigned.";

                    return false;
                }

                if (!ids.Add(id))
                {
                    error =
                        $"Duplicate electronic object " +
                        $"binding ID '{id}'.";

                    return false;
                }
            }

            return true;
        }

        // ============================================================
        // OUTPUT VALIDATION
        // ============================================================

        private bool ValidateOutputBindings(
            out string error)
        {
            error = string.Empty;

            if (outputs == null)
                return true;

            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (int i = 0; i < outputs.Length; i++)
            {
                GameObjectBinding binding =
                    outputs[i];

                if (binding == null)
                {
                    error =
                        $"GameObject output binding " +
                        $"at index {i} is null.";

                    return false;
                }

                string id = binding.Id;

                if (string.IsNullOrWhiteSpace(id))
                {
                    error =
                        $"GameObject output binding " +
                        $"at index {i} has an empty ID.";

                    return false;
                }

                if (binding.Target == null)
                {
                    error =
                        $"GameObject output binding '{id}' " +
                        $"has no GameObject assigned.";

                    return false;
                }

                if (!ids.Add(id))
                {
                    error =
                        $"Duplicate GameObject output " +
                        $"binding ID '{id}'.";

                    return false;
                }
            }

            return true;
        }

        // ============================================================
        // EDITOR VALIDATION
        // ============================================================

        [ContextMenu("Validate Scene Bindings")]
        private void ValidateSceneBindings()
        {
            if (Validate(out string error))
            {
                Debug.Log(
                    $"[{nameof(SparkLevelSceneBindings)}] " +
                    $"Scene bindings are valid.",
                    this);

                return;
            }

            Debug.LogError(
                $"[{nameof(SparkLevelSceneBindings)}] " +
                $"Scene binding validation failed: {error}",
                this);
        }

        // ============================================================
        // COUNTS
        // ============================================================

        public int TerminalBindingCount
        {
            get
            {
                return terminals != null
                    ? terminals.Length
                    : 0;
            }
        }

        public int ComponentBindingCount
        {
            get
            {
                return components != null
                    ? components.Length
                    : 0;
            }
        }

        public int ElectronicObjectBindingCount
        {
            get
            {
                return electronicObjects != null
                    ? electronicObjects.Length
                    : 0;
            }
        }

        public int OutputBindingCount
        {
            get
            {
                return outputs != null
                    ? outputs.Length
                    : 0;
            }
        }
    }
}
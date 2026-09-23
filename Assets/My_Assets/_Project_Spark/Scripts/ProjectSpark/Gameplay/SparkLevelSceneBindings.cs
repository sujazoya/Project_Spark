using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Scene-side bindings for a SparkLevelDefinition.
    ///
    /// SparkLevelDefinition is a ScriptableObject and therefore must remain
    /// independent from scene objects.
    ///
    /// This component stores the actual scene references and exposes them
    /// through stable string IDs used by the level definition.
    ///
    /// Architecture:
    ///
    ///     SparkLevelDefinition
    ///             |
    ///             | IDs
    ///             v
    ///     SparkLevelSceneBindings
    ///             |
    ///             | actual scene references
    ///             v
    ///     SparkTerminal / SparkElectricalComponent / GameObject
    ///
    /// This class does NOT evaluate circuits and does NOT own gameplay logic.
    /// It is only the scene binding layer.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelSceneBindings : MonoBehaviour
    {
        // ================================================================
        // TERMINAL BINDING
        // ================================================================

        [Serializable]
        private sealed class TerminalBinding
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private SparkTerminal terminal;

            public string Id => id;

            public SparkTerminal Terminal => terminal;
        }

        // ================================================================
        // ELECTRICAL COMPONENT BINDING
        // ================================================================

        [Serializable]
        private sealed class ComponentBinding
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private SparkElectricalComponent component;

            public string Id => id;

            public SparkElectricalComponent Component => component;
        }

        // ================================================================
        // GAMEOBJECT OUTPUT BINDING
        // ================================================================

        [Serializable]
        private sealed class GameObjectBinding
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private GameObject target;

            public string Id => id;

            public GameObject Target => target;
        }

        // ================================================================
        // INSPECTOR DATA
        // ================================================================

        [Header("Terminal Bindings")]
        [Tooltip(
            "Maps stable level terminal IDs to actual SparkTerminal components in this scene.")]
        [SerializeField]
        private TerminalBinding[] terminals = Array.Empty<TerminalBinding>();

        [Header("Electrical Component Bindings")]
        [Tooltip(
            "Maps stable level component IDs to actual SparkElectricalComponent components in this scene.")]
        [SerializeField]
        private ComponentBinding[] components = Array.Empty<ComponentBinding>();

        [Header("GameObject Output Bindings")]
        [Tooltip(
            "Maps stable output IDs to GameObjects used for level success/failure presentation.")]
        [SerializeField]
        private GameObjectBinding[] outputs = Array.Empty<GameObjectBinding>();

        // ================================================================
        // RUNTIME LOOKUP CACHE
        // ================================================================

        private Dictionary<string, SparkTerminal> terminalLookup;

        private Dictionary<string, SparkElectricalComponent> componentLookup;

        private Dictionary<string, GameObject> outputLookup;

        private bool lookupCacheBuilt;

        // ================================================================
        // UNITY LIFECYCLE
        // ================================================================

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

    /*
     * Do not depend on the runtime dictionaries while editing.
     * They will be rebuilt when the object becomes active.
     */
    lookupCacheBuilt = false;
}
#endif

        // ================================================================
        // CACHE BUILD
        // ================================================================

        /// <summary>
        /// Builds the runtime lookup tables.
        ///
        /// IDs are case-sensitive and must be unique within their own
        /// binding category.
        /// </summary>
        private void BuildLookupCache()
        {
            terminalLookup = new Dictionary<string, SparkTerminal>(
                StringComparer.Ordinal);

            componentLookup = new Dictionary<string, SparkElectricalComponent>(
                StringComparer.Ordinal);

            outputLookup = new Dictionary<string, GameObject>(
                StringComparer.Ordinal);

            lookupCacheBuilt = true;

            // ------------------------------------------------------------
            // TERMINALS
            // ------------------------------------------------------------

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

                    if (binding.Terminal == null)
                        continue;

                    if (terminalLookup.ContainsKey(id))
                    {
                        Debug.LogError(
                            $"[LEVEL BINDINGS] Duplicate terminal ID '{id}' " +
                            $"found on '{name}'. The first valid binding is retained.",
                            this);

                        continue;
                    }

                    terminalLookup.Add(id, binding.Terminal);
                }
            }

            // ------------------------------------------------------------
            // COMPONENTS
            // ------------------------------------------------------------

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

                    if (binding.Component == null)
                        continue;

                    if (componentLookup.ContainsKey(id))
                    {
                        Debug.LogError(
                            $"[LEVEL BINDINGS] Duplicate component ID '{id}' " +
                            $"found on '{name}'. The first valid binding is retained.",
                            this);

                        continue;
                    }

                    componentLookup.Add(id, binding.Component);
                }
            }

            // ------------------------------------------------------------
            // OUTPUTS
            // ------------------------------------------------------------

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

                    if (binding.Target == null)
                        continue;

                    if (outputLookup.ContainsKey(id))
                    {
                        Debug.LogError(
                            $"[LEVEL BINDINGS] Duplicate output ID '{id}' " +
                            $"found on '{name}'. The first valid binding is retained.",
                            this);

                        continue;
                    }

                    outputLookup.Add(id, binding.Target);
                }
            }
        }

        // ================================================================
        // CACHE SAFETY
        // ================================================================

        private void EnsureLookupCache()
        {
            if (!lookupCacheBuilt ||
                terminalLookup == null ||
                componentLookup == null ||
                outputLookup == null)
            {
                BuildLookupCache();
            }
        }

        // ================================================================
        // TERMINAL LOOKUP
        // ================================================================

        /// <summary>
        /// Resolves a terminal ID to its actual scene SparkTerminal.
        /// </summary>
        public bool TryGetTerminal(
            string id,
            out SparkTerminal terminal)
        {
            EnsureLookupCache();

            terminal = null;

            if (string.IsNullOrWhiteSpace(id))
                return false;

            return terminalLookup.TryGetValue(id, out terminal) &&
                   terminal != null;
        }

        /// <summary>
        /// Returns true when a valid terminal binding exists for the ID.
        /// </summary>
        public bool HasTerminal(string id)
        {
            return TryGetTerminal(id, out _);
        }

        // ================================================================
        // COMPONENT LOOKUP
        // ================================================================

        /// <summary>
        /// Resolves a component ID to its actual scene
        /// SparkElectricalComponent.
        /// </summary>
        public bool TryGetComponent(
            string id,
            out SparkElectricalComponent component)
        {
            EnsureLookupCache();

            component = null;

            if (string.IsNullOrWhiteSpace(id))
                return false;

            return componentLookup.TryGetValue(id, out component) &&
                   component != null;
        }

        /// <summary>
        /// Returns true when a valid electrical component binding exists.
        /// </summary>
        public bool HasComponent(string id)
        {
            return TryGetComponent(id, out _);
        }

        // ================================================================
        // GAMEOBJECT OUTPUT LOOKUP
        // ================================================================

        /// <summary>
        /// Resolves an output ID to its actual scene GameObject.
        /// </summary>
        public bool TryGetObject(
            string id,
            out GameObject target)
        {
            EnsureLookupCache();

            target = null;

            if (string.IsNullOrWhiteSpace(id))
                return false;

            return outputLookup.TryGetValue(id, out target) &&
                   target != null;
        }

        /// <summary>
        /// Returns true when a valid GameObject output binding exists.
        /// </summary>
        public bool HasObject(string id)
        {
            return TryGetObject(id, out _);
        }

        // ================================================================
        // VALIDATION
        // ================================================================

        /// <summary>
        /// Validates all scene bindings.
        ///
        /// This does not validate whether the bindings are appropriate for
        /// a particular level asset. It only validates the binding table
        /// itself.
        /// </summary>
        public bool Validate(out string error)
        {
            error = string.Empty;

            // ------------------------------------------------------------
            // TERMINALS
            // ------------------------------------------------------------

            if (!ValidateTerminalBindings(out error))
                return false;

            // ------------------------------------------------------------
            // COMPONENTS
            // ------------------------------------------------------------

            if (!ValidateComponentBindings(out error))
                return false;

            // ------------------------------------------------------------
            // OUTPUTS
            // ------------------------------------------------------------

            if (!ValidateOutputBindings(out error))
                return false;

            return true;
        }

        // ================================================================
        // TERMINAL VALIDATION
        // ================================================================

        private bool ValidateTerminalBindings(out string error)
        {
            error = string.Empty;

            if (terminals == null)
                return true;

            HashSet<string> ids = new HashSet<string>(
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

                if (string.IsNullOrWhiteSpace(binding.Id))
                {
                    error =
                        $"Terminal binding at index {i} has an empty ID.";

                    return false;
                }

                string id = binding.Id.Trim();

                if (!ids.Add(id))
                {
                    error =
                        $"Duplicate terminal binding ID '{id}'.";

                    return false;
                }

                if (binding.Terminal == null)
                {
                    error =
                        $"Terminal binding '{id}' has no SparkTerminal assigned.";

                    return false;
                }
            }

            return true;
        }

        // ================================================================
        // COMPONENT VALIDATION
        // ================================================================

        private bool ValidateComponentBindings(out string error)
        {
            error = string.Empty;

            if (components == null)
                return true;

            HashSet<string> ids = new HashSet<string>(
                StringComparer.Ordinal);

            for (int i = 0; i < components.Length; i++)
            {
                ComponentBinding binding = components[i];

                if (binding == null)
                {
                    error =
                        $"Component binding at index {i} is null.";

                    return false;
                }

                if (string.IsNullOrWhiteSpace(binding.Id))
                {
                    error =
                        $"Component binding at index {i} has an empty ID.";

                    return false;
                }

                string id = binding.Id.Trim();

                if (!ids.Add(id))
                {
                    error =
                        $"Duplicate component binding ID '{id}'.";

                    return false;
                }

                if (binding.Component == null)
                {
                    error =
                        $"Component binding '{id}' has no " +
                        $"SparkElectricalComponent assigned.";

                    return false;
                }
            }

            return true;
        }

        // ================================================================
        // OUTPUT VALIDATION
        // ================================================================

        private bool ValidateOutputBindings(out string error)
        {
            error = string.Empty;

            if (outputs == null)
                return true;

            HashSet<string> ids = new HashSet<string>(
                StringComparer.Ordinal);

            for (int i = 0; i < outputs.Length; i++)
            {
                GameObjectBinding binding = outputs[i];

                if (binding == null)
                {
                    error =
                        $"Output binding at index {i} is null.";

                    return false;
                }

                if (string.IsNullOrWhiteSpace(binding.Id))
                {
                    error =
                        $"Output binding at index {i} has an empty ID.";

                    return false;
                }

                string id = binding.Id.Trim();

                if (!ids.Add(id))
                {
                    error =
                        $"Duplicate output binding ID '{id}'.";

                    return false;
                }

                if (binding.Target == null)
                {
                    error =
                        $"Output binding '{id}' has no GameObject assigned.";

                    return false;
                }
            }

            return true;
        }

        // ================================================================
        // DEBUG VALIDATION
        // ================================================================

        [ContextMenu("Validate Scene Bindings")]
        private void ValidateSceneBindings()
        {
            if (Validate(out string error))
            {
                Debug.Log(
                    $"[LEVEL BINDINGS] Validation successful on '{name}'.",
                    this);

                return;
            }

            Debug.LogError(
                $"[LEVEL BINDINGS] Validation failed on '{name}': {error}",
                this);
        }

        // ================================================================
        // DEBUG INFORMATION
        // ================================================================

        /// <summary>
        /// Number of terminal bindings currently configured.
        /// </summary>
        public int TerminalBindingCount
        {
            get
            {
                return terminals != null
                    ? terminals.Length
                    : 0;
            }
        }

        /// <summary>
        /// Number of electrical component bindings currently configured.
        /// </summary>
        public int ComponentBindingCount
        {
            get
            {
                return components != null
                    ? components.Length
                    : 0;
            }
        }

        /// <summary>
        /// Number of GameObject output bindings currently configured.
        /// </summary>
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
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Registry of solver definitions used by SparkElectricalSolver.
    ///
    /// Responsibilities:
    /// - Match discovered electrical components to solver definitions.
    /// - Cache component -> definition lookups.
    /// - Identify components that are intentionally non-solver objects.
    /// - Validate the configured definition list.
    ///
    /// Important:
    /// Automatic component discovery happens in SparkElectricalSolver.
    /// This registry only decides whether a discovered component
    /// actually needs an electrical solver definition.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkElectricalSolverRegistry",
        menuName = "Project Spark/Electrical/Solver Registry")]
    public sealed class SparkElectricalSolverRegistry :
        ScriptableObject
    {
        [Header("Solver Definitions")]
        [SerializeField]
        private List<SparkElectricalSolverDefinition> definitions =
            new List<SparkElectricalSolverDefinition>();

        [Header("Ignored Component Types")]
        [Tooltip(
            "Components in this list are discovered normally but "
            + "are intentionally ignored by the electrical solver.")]
        [SerializeField]
        private List<string> ignoredComponentTypeNames =
            new List<string>
            {
                "SparkTemporaryObject"
            };

        private readonly Dictionary<
            SparkElectricalComponent,
            SparkElectricalSolverDefinition>
            runtimeCache =
                new Dictionary<
                    SparkElectricalComponent,
                    SparkElectricalSolverDefinition>();

        private readonly HashSet<string>
            ignoredTypeCache =
                new HashSet<string>();

        public IReadOnlyList<SparkElectricalSolverDefinition>
            Definitions =>
            definitions;

        /// <summary>
        /// Returns true when the component is intentionally ignored
        /// by the electrical solver.
        /// </summary>
        public bool IsIgnored(
            SparkElectricalComponent component)
        {
            if (component == null)
                return true;

            string typeName =
                component.GetType().Name;

            return IsIgnoredTypeName(typeName);
        }

        /// <summary>
        /// Gets the solver definition for a component.
        ///
        /// Returns false for intentionally ignored components.
        /// Returns false for real components that have no registered
        /// solver definition.
        /// </summary>
        public bool TryGetDefinition(
            SparkElectricalComponent component,
            out SparkElectricalSolverDefinition definition)
        {
            definition = null;

            if (component == null)
                return false;

            /*
             * Temporary/helper components do not need solver
             * definitions.
             */
            if (IsIgnored(component))
                return false;

            if (runtimeCache.TryGetValue(
                    component,
                    out definition))
            {
                return definition != null;
            }

            definition =
                FindDefinition(component);

            if (definition == null)
                return false;

            runtimeCache[component] =
                definition;

            return true;
        }

        /// <summary>
        /// Determines whether the component has a registered
        /// solver definition.
        ///
        /// Ignored components return true because they intentionally
        /// require no solver definition.
        /// </summary>
        public bool HasSolverDefinition(
            SparkElectricalComponent component)
        {
            if (component == null)
                return false;

            if (IsIgnored(component))
                return true;

            return TryGetDefinition(
                component,
                out _);
        }

        /// <summary>
        /// Clears component -> definition runtime cache.
        /// </summary>
        public void ClearRuntimeCache()
        {
            runtimeCache.Clear();
        }

        /// <summary>
        /// Validates the configured registry.
        /// </summary>
        public bool ValidateRegistry(
            out string error)
        {
            error = string.Empty;

            if (definitions == null ||
                definitions.Count == 0)
            {
                error =
                    "SparkElectricalSolverRegistry contains " +
                    "no solver definitions.";

                return false;
            }

            for (int i = 0;
                 i < definitions.Count;
                 i++)
            {
                SparkElectricalSolverDefinition definition =
                    definitions[i];

                if (definition == null)
                {
                    error =
                        $"Solver definition at index {i} is null.";

                    return false;
                }

                if (definition.SupportedComponentType == null)
                {
                    error =
                        $"Solver definition " +
                        $"'{definition.name}' does not declare " +
                        "a supported component type.";

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Finds a definition for a component.
        ///
        /// Highest priority wins.
        /// </summary>
        private SparkElectricalSolverDefinition FindDefinition(
            SparkElectricalComponent component)
        {
            SparkElectricalSolverDefinition bestDefinition =
                null;

            int bestPriority =
                int.MinValue;

            if (definitions == null)
                return null;

            for (int i = 0;
                 i < definitions.Count;
                 i++)
            {
                SparkElectricalSolverDefinition candidate =
                    definitions[i];

                if (candidate == null)
                    continue;

                if (candidate.SupportedComponentType == null)
                    continue;

                if (!candidate.SupportedComponentType
                        .IsAssignableFrom(
                            component.GetType()))
                {
                    continue;
                }

                if (!candidate.CanHandle(component))
                    continue;

                if (candidate.Priority < bestPriority)
                    continue;

                bestDefinition =
                    candidate;

                bestPriority =
                    candidate.Priority;
            }

            return bestDefinition;
        }

        private bool IsIgnoredTypeName(
            string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return false;

            if (ignoredTypeCache.Contains(typeName))
                return true;

            if (ignoredComponentTypeNames == null)
                return false;

            for (int i = 0;
                 i < ignoredComponentTypeNames.Count;
                 i++)
            {
                string ignoredType =
                    ignoredComponentTypeNames[i];

                if (string.IsNullOrWhiteSpace(
                        ignoredType))
                {
                    continue;
                }

                if (!string.Equals(
                        ignoredType,
                        typeName,
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                ignoredTypeCache.Add(typeName);
                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            runtimeCache.Clear();
            ignoredTypeCache.Clear();
            RebuildIgnoredTypeCache();
        }

        private void OnValidate()
        {
            if (definitions == null)
            {
                definitions =
                    new List<SparkElectricalSolverDefinition>();
            }

            for (int i = definitions.Count - 1;
                 i >= 0;
                 i--)
            {
                if (definitions[i] == null)
                    definitions.RemoveAt(i);
            }

            if (ignoredComponentTypeNames == null)
            {
                ignoredComponentTypeNames =
                    new List<string>();
            }

            RemoveInvalidIgnoredNames();

            runtimeCache.Clear();
            ignoredTypeCache.Clear();

            RebuildIgnoredTypeCache();
        }

        private void RebuildIgnoredTypeCache()
        {
            if (ignoredComponentTypeNames == null)
                return;

            for (int i = 0;
                 i < ignoredComponentTypeNames.Count;
                 i++)
            {
                string typeName =
                    ignoredComponentTypeNames[i];

                if (string.IsNullOrWhiteSpace(
                        typeName))
                {
                    continue;
                }

                ignoredTypeCache.Add(
                    typeName.Trim());
            }
        }

        private void RemoveInvalidIgnoredNames()
        {
            for (int i =
                     ignoredComponentTypeNames.Count - 1;
                 i >= 0;
                 i--)
            {
                string value =
                    ignoredComponentTypeNames[i];

                if (string.IsNullOrWhiteSpace(value))
                {
                    ignoredComponentTypeNames.RemoveAt(i);
                }
            }
        }
    }
}
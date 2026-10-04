using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Central registry of electrical solver definitions.
    ///
    /// The registry contains ScriptableObject configuration only.
    /// Runtime electrical state remains inside the solver/components.
    ///
    /// A component is matched through its definition's CanHandle()
    /// method. Priority resolves multiple definitions that can handle
    /// the same component.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkElectricalSolverRegistry",
        menuName = "Project Spark/Electrical/Solver Registry")]
    public sealed class SparkElectricalSolverRegistry :
        ScriptableObject
    {
        [SerializeField]
        private List<SparkElectricalSolverDefinition> definitions =
            new List<SparkElectricalSolverDefinition>();

        private readonly Dictionary<
            SparkElectricalComponent,
            SparkElectricalSolverDefinition>
            runtimeCache =
                new Dictionary<
                    SparkElectricalComponent,
                    SparkElectricalSolverDefinition>();

        public IReadOnlyList<
            SparkElectricalSolverDefinition>
            Definitions =>
            definitions;

        /// <summary>
        /// Finds the highest-priority definition that can handle
        /// the supplied runtime component.
        /// </summary>
        public bool TryGetDefinition(
            SparkElectricalComponent component,
            out SparkElectricalSolverDefinition definition)
        {
            definition = null;

            if (component == null)
            {
                return false;
            }

            if (runtimeCache.TryGetValue(
                    component,
                    out definition))
            {
                return definition != null;
            }

            definition =
                FindDefinition(component);

            if (definition == null)
            {
                return false;
            }

            runtimeCache[component] =
                definition;

            return true;
        }

        /// <summary>
        /// Clears runtime component-to-definition mappings.
        ///
        /// Call this when the solver's component set changes or
        /// when the registry configuration needs to be re-evaluated.
        /// </summary>
        public void ClearRuntimeCache()
        {
            runtimeCache.Clear();
        }

        /// <summary>
        /// Validates that every registry entry is usable.
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
                        $"Solver definition '{definition.name}' " +
                        "does not declare a supported component type.";

                    return false;
                }
            }

            return true;
        }

        private SparkElectricalSolverDefinition FindDefinition(
            SparkElectricalComponent component)
        {
            SparkElectricalSolverDefinition bestDefinition =
                null;

            int bestPriority =
                int.MinValue;

            for (int i = 0;
                 i < definitions.Count;
                 i++)
            {
                SparkElectricalSolverDefinition candidate =
                    definitions[i];

                if (candidate == null)
                {
                    continue;
                }

                if (!candidate.CanHandle(component))
                {
                    continue;
                }

                if (candidate.Priority < bestPriority)
                {
                    continue;
                }

                bestDefinition =
                    candidate;

                bestPriority =
                    candidate.Priority;
            }

            return bestDefinition;
        }

        private void OnEnable()
        {
            runtimeCache.Clear();
        }

        private void OnValidate()
        {
            if (definitions == null)
            {
                definitions =
                    new List<
                        SparkElectricalSolverDefinition>();
            }

            for (int i = definitions.Count - 1;
                 i >= 0;
                 i--)
            {
                if (definitions[i] == null)
                {
                    definitions.RemoveAt(i);
                }
            }

            runtimeCache.Clear();
        }
    }
}
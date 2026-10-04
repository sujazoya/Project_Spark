using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Base ScriptableObject definition for one electrical solver type.
    ///
    /// Definitions contain solver configuration and device-specific
    /// numerical behavior. They do not store runtime electrical state.
    /// </summary>
    public abstract class SparkElectricalSolverDefinition :
        ScriptableObject
    {
        [SerializeField]
        private string solverId = string.Empty;

        [SerializeField]
        private int priority = 0;

        // ---------------------------------------------------------------------
        // Identity
        // ---------------------------------------------------------------------

        public string SolverId =>
            solverId;

        public int Priority =>
            priority;

        public abstract Type SupportedComponentType
        {
            get;
        }

        // ---------------------------------------------------------------------
        // Component matching
        // ---------------------------------------------------------------------

        public abstract bool CanHandle(
            SparkElectricalComponent component);

        // ---------------------------------------------------------------------
        // Topology
        // ---------------------------------------------------------------------

        public virtual void RegisterTopology(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
        }

        // ---------------------------------------------------------------------
        // Matrix stamping
        // ---------------------------------------------------------------------

        public abstract void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context);

        // ---------------------------------------------------------------------
        // Solved state
        // ---------------------------------------------------------------------

        public abstract void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result);

        // ---------------------------------------------------------------------
        // Terminal access
        // ---------------------------------------------------------------------

        public virtual bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (graph == null ||
                component == null)
            {
                return false;
            }

            return graph.TryGetPrimaryComponentTerminals(
                component,
                out terminalA,
                out terminalB);
        }

        // ---------------------------------------------------------------------
        // Reference / ground terminal
        // ---------------------------------------------------------------------

        public virtual bool TryGetReferenceTerminal(
            SparkElectricalComponent component,
            out SparkTerminal terminal)
        {
            terminal = null;
            return false;
        }

        // ---------------------------------------------------------------------
        // Voltage source
        // ---------------------------------------------------------------------

        public virtual bool TryGetSourceVoltage(
            SparkElectricalComponent component,
            out float voltage)
        {
            voltage = 0f;
            return false;
        }

        // ---------------------------------------------------------------------
        // Continuous solving
        // ---------------------------------------------------------------------

        public virtual bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            return false;
        }

        // ---------------------------------------------------------------------
        // Transient state
        // ---------------------------------------------------------------------

        public virtual void CommitTransientState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
        }

        public virtual void ResetRuntimeState(
            SparkElectricalComponent component)
        {
        }

        // ---------------------------------------------------------------------
        // Current calculation
        // ---------------------------------------------------------------------

        /// <summary>
        /// Calculates the solved current through this component.
        ///
        /// The core solver calls this method without knowing the concrete
        /// electrical component type.
        ///
        /// Return false when this definition does not provide a current
        /// calculation for the component.
        /// </summary>
        public virtual bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;
            return false;
        }

        // ---------------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------------

        protected virtual void OnValidate()
        {
            priority =
                Mathf.Max(
                    0,
                    priority);

            if (string.IsNullOrWhiteSpace(
                    solverId))
            {
                solverId = name;
            }
        }
    }
}
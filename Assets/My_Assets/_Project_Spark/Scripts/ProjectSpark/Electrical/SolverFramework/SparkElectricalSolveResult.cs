using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Immutable result of one completed electrical solve.
    ///
    /// Contains:
    /// - Full graph node voltages.
    /// - Full reduced MNA solution.
    /// - Access to the solve context for persistent solver state.
    /// - Nonlinear component state.
    /// - Generic voltage-source currents.
    /// - Indexed voltage-source currents.
    /// - Source information.
    ///
    /// Runtime state remains owned by SparkElectricalSolver.
    /// This class only exposes the completed solve result.
    /// </summary>
    public sealed class SparkElectricalSolveResult
    {
        private readonly SparkElectricalSolveContext context;
        private readonly float[] nodeVoltages;
        private readonly double[] reducedSolution;

        // ---------------------------------------------------------------------
        // CONSTRUCTOR
        // ---------------------------------------------------------------------

        public SparkElectricalSolveResult(
            SparkElectricalSolveContext context,
            float[] nodeVoltages,
            double[] reducedSolution)
        {
            this.context =
                context ??
                throw new ArgumentNullException(
                    nameof(context));

            this.nodeVoltages =
                nodeVoltages ??
                throw new ArgumentNullException(
                    nameof(nodeVoltages));

            this.reducedSolution =
                reducedSolution ??
                Array.Empty<double>();
        }

        // ---------------------------------------------------------------------
        // BASIC INFORMATION
        // ---------------------------------------------------------------------

        public SparkElectricalSolveContext Context =>
            context;

        public int NodeCount =>
            nodeVoltages.Length;

        public int MatrixSize =>
            reducedSolution.Length;

        public float TimeStep =>
            context.TimeStep;

        public bool SimulateTransientDevices =>
            context.SimulateTransientDevices;

        // ---------------------------------------------------------------------
        // NODE VOLTAGE
        // ---------------------------------------------------------------------

        public float GetNodeVoltage(
            int node)
        {
            if (node < 0 ||
                node >= nodeVoltages.Length)
            {
                return 0f;
            }

            return nodeVoltages[node];
        }

        public bool TryGetNodeVoltage(
            int node,
            out float voltage)
        {
            if (node < 0 ||
                node >= nodeVoltages.Length)
            {
                voltage = 0f;
                return false;
            }

            voltage =
                nodeVoltages[node];

            return true;
        }

        // ---------------------------------------------------------------------
        // TERMINAL VOLTAGE
        // ---------------------------------------------------------------------

        public float GetTerminalVoltage(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return 0f;

            if (!context.Graph.TryGetNode(
                    terminal,
                    out int node))
            {
                return 0f;
            }

            return GetNodeVoltage(node);
        }

        public bool TryGetTerminalVoltage(
            SparkTerminal terminal,
            out float voltage)
        {
            if (terminal == null)
            {
                voltage = 0f;
                return false;
            }

            if (!context.Graph.TryGetNode(
                    terminal,
                    out int node))
            {
                voltage = 0f;
                return false;
            }

            voltage =
                GetNodeVoltage(node);

            return true;
        }

        // ---------------------------------------------------------------------
        // DIFFERENTIAL VOLTAGE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Returns:
        ///
        /// V(positive) - V(negative)
        /// </summary>
        public float GetVoltage(
            SparkTerminal positive,
            SparkTerminal negative)
        {
            if (positive == null ||
                negative == null)
            {
                return 0f;
            }

            return
                GetTerminalVoltage(positive)
                -
                GetTerminalVoltage(negative);
        }

        // ---------------------------------------------------------------------
        // RESISTIVE CURRENT
        // ---------------------------------------------------------------------

        /// <summary>
        /// Calculates current using:
        ///
        /// I = V / R
        ///
        /// Positive current flows from positive terminal
        /// toward negative terminal.
        /// </summary>
        public float CalculateResistiveCurrent(
            SparkTerminal positive,
            SparkTerminal negative,
            float resistance)
        {
            resistance =
                Mathf.Max(
                    context.MinimumResistance,
                    resistance);

            float voltage =
                GetVoltage(
                    positive,
                    negative);

            return voltage / resistance;
        }

        // ---------------------------------------------------------------------
        // NONLINEAR STATE
        // ---------------------------------------------------------------------

        public bool GetNonlinearState(
            SparkElectricalComponent component)
        {
            if (component == null)
                return false;

            return context.GetNonlinearState(
                component);
        }

        // ---------------------------------------------------------------------
        // SOURCE INFORMATION
        // ---------------------------------------------------------------------

        /// <summary>
        /// Returns the highest absolute active source voltage
        /// reported by registered solver definitions.
        /// </summary>
        public float GetMaximumActiveSourceVoltage()
        {
            return context.GetMaximumActiveSourceVoltage();
        }

        // ---------------------------------------------------------------------
        // PREVIOUS TRANSIENT STATE
        // ---------------------------------------------------------------------

        public bool TryGetPreviousComponentVoltage(
            SparkElectricalComponent component,
            out float voltage)
        {
            if (component == null)
            {
                voltage = 0f;
                return false;
            }

            return context.TryGetPreviousComponentVoltage(
                component,
                out voltage);
        }

        public float GetPreviousComponentVoltage(
            SparkElectricalComponent component)
        {
            if (component == null)
                return 0f;

            return context.GetPreviousComponentVoltage(
                component);
        }

        /// <summary>
        /// Commits the latest component voltage into the
        /// persistent transient-state dictionary.
        ///
        /// This is intended for solver definitions that own
        /// transient devices such as capacitors.
        /// </summary>
        public void CommitPreviousComponentVoltage(
            SparkElectricalComponent component,
            float voltage)
        {
            if (component == null)
                return;

            context.SetPreviousComponentVoltage(
                component,
                voltage);
        }

        // ---------------------------------------------------------------------
        // GENERIC VOLTAGE SOURCE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Returns the MNA auxiliary current associated with
        /// a registered voltage-source component.
        ///
        /// The current orientation is determined by the
        /// voltage-source stamping performed by the solver
        /// definition.
        /// </summary>
        public float GetVoltageSourceCurrent(
            SparkElectricalComponent component)
        {
            if (component == null)
                return 0f;

            if (!context.TryGetVoltageSourceIndex(
                    component,
                    out int sourceIndex))
            {
                return 0f;
            }

            if (sourceIndex < 0 ||
                sourceIndex >= reducedSolution.Length)
            {
                return 0f;
            }

            return
                (float)reducedSolution[sourceIndex];
        }

        public bool TryGetVoltageSourceCurrent(
            SparkElectricalComponent component,
            out float current)
        {
            current = 0f;

            if (component == null)
                return false;

            if (!context.TryGetVoltageSourceIndex(
                    component,
                    out int sourceIndex))
            {
                return false;
            }

            if (sourceIndex < 0 ||
                sourceIndex >= reducedSolution.Length)
            {
                return false;
            }

            current =
                (float)reducedSolution[sourceIndex];

            return true;
        }

        // ---------------------------------------------------------------------
        // INDEXED VOLTAGE SOURCE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Returns the MNA current through an indexed voltage source.
        ///
        /// This remains available for SparkSwitchIndex.
        /// </summary>
        public float GetIndexedSourceCurrent(
            SparkSwitchIndex indexedSwitch)
        {
            if (indexedSwitch == null)
                return 0f;

            return GetVoltageSourceCurrent(
                indexedSwitch);
        }

        public bool TryGetIndexedSourceCurrent(
            SparkSwitchIndex indexedSwitch,
            out float current)
        {
            if (indexedSwitch == null)
            {
                current = 0f;
                return false;
            }

            return TryGetVoltageSourceCurrent(
                indexedSwitch,
                out current);
        }

        // ---------------------------------------------------------------------
        // MAXIMUM ACTIVE SOURCE
        // ---------------------------------------------------------------------

        public float GetMaximumActiveSupplyVoltage()
        {
            // Compatibility with older solver definitions.
            //
            // New definitions should use:
            // GetMaximumActiveSourceVoltage()
            return GetMaximumActiveSourceVoltage();
        }
    }
}
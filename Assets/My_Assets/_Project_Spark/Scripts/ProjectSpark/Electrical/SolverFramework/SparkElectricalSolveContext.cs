
using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Per-solve assembly context for the Project Spark electrical solver.
    ///
    /// Responsibilities:
    /// - Maps circuit graph nodes to the reduced MNA matrix.
    /// - Owns matrix/RHS assembly.
    /// - Stores indexed voltage-source allocations.
    /// - Provides nonlinear device state during iterative solving.
    /// - Provides previous transient component state.
    /// - Provides the current iteration voltage estimate.
    ///
    /// Runtime state is supplied by SparkElectricalSolver.
    /// ScriptableObject solver definitions contain configuration only.
    /// </summary>
    public sealed class SparkElectricalSolveContext
    {
        private readonly SparkElectricalNetworkGraph graph;
        private readonly int referenceNode;
        private readonly int reducedNodeCount;
        private readonly int nodeCount;

        private readonly SparkElectricalSolverRegistry registry;
        private readonly IReadOnlyList<SparkElectricalComponent> components;

        private readonly Dictionary<SparkElectricalComponent, bool>
            nonlinearStates;

        private readonly Dictionary<SparkElectricalComponent, float>
            previousComponentVoltages;

        private readonly Dictionary<
    SparkElectricalComponent,
    int>
    voltageSourceIndices =
        new Dictionary<
            SparkElectricalComponent,
            int>();

        private float[] iterationVoltages;

        private double[,] matrix;
        private double[] rightHandSide;

        /// <summary>
        /// Creates a new solve context.
        /// </summary>
        public SparkElectricalSolveContext(
            SparkElectricalNetworkGraph graph,
            int referenceNode,
            SparkElectricalSolverRegistry registry,
            IReadOnlyList<SparkElectricalComponent> components,
            Dictionary<SparkElectricalComponent, bool> nonlinearStates,
            Dictionary<SparkElectricalComponent, float> previousComponentVoltages,
            bool simulateTransientDevices,
            float timeStep,
            float minimumResistance,
            float floatingNodeLeakResistance)
        {
            this.graph = graph ??
                throw new ArgumentNullException(nameof(graph));

            this.referenceNode = referenceNode;

            this.registry = registry ??
                throw new ArgumentNullException(nameof(registry));

            this.components = components ??
                throw new ArgumentNullException(nameof(components));

            this.nonlinearStates = nonlinearStates ??
                throw new ArgumentNullException(nameof(nonlinearStates));

            this.previousComponentVoltages =
                previousComponentVoltages ??
                throw new ArgumentNullException(
                    nameof(previousComponentVoltages));

            SimulateTransientDevices =
                simulateTransientDevices;

            TimeStep =
                Mathf.Max(
                    0.000001f,
                    timeStep);

            MinimumResistance =
                Mathf.Max(
                    0.000000001f,
                    minimumResistance);

            FloatingNodeLeakResistance =
                Mathf.Max(
                    0f,
                    floatingNodeLeakResistance);

           nodeCount =
    Mathf.Max(
        0,
        graph.NodeCount);

reducedNodeCount =
    Mathf.Max(
        0,
        nodeCount - 1);
        }

        // ---------------------------------------------------------------------
        // GRAPH
        // ---------------------------------------------------------------------

        public SparkElectricalNetworkGraph Graph =>
            graph;

        public int ReferenceNode =>
            referenceNode;

       public int NodeCount =>
    nodeCount;

public int ReducedNodeCount =>
    reducedNodeCount;

        // ---------------------------------------------------------------------
        // SOLVER SETTINGS
        // ---------------------------------------------------------------------

        public float MinimumResistance
        {
            get;
        }

        public float FloatingNodeLeakResistance
        {
            get;
        }

        public bool SimulateTransientDevices
        {
            get;
        }

        public float TimeStep
        {
            get;
        }

        // ---------------------------------------------------------------------
        // MATRIX
        // ---------------------------------------------------------------------

        public double[,] Matrix =>
            matrix;

        public double[] RightHandSide =>
            rightHandSide;

       public int AuxiliarySourceCount =>
    voltageSourceIndices.Count;

public int MatrixSize =>
    reducedNodeCount +
    voltageSourceIndices.Count;

        /// <summary>
        /// Allocates a clean MNA matrix after all topology
        /// registrations have been completed.
        /// </summary>
        public void InitializeMatrix()
        {
            int size = MatrixSize;

            matrix =
                new double[size, size];

            rightHandSide =
                new double[size];
        }

        public int RegisterVoltageSource(
    SparkElectricalComponent component)
{
    if (component == null)
        return -1;

    if (voltageSourceIndices.TryGetValue(
            component,
            out int existingIndex))
    {
        return existingIndex;
    }

    int sourceIndex =
        reducedNodeCount +
        voltageSourceIndices.Count;

    voltageSourceIndices.Add(
        component,
        sourceIndex);

    return sourceIndex;
}

public bool TryGetVoltageSourceIndex(
    SparkElectricalComponent component,
    out int sourceIndex)
{
    sourceIndex = -1;

    if (component == null)
        return false;

    return voltageSourceIndices.TryGetValue(
        component,
        out sourceIndex);
}

        /// <summary>
        /// Clears matrix and RHS while preserving topology allocations.
        /// Useful if the same context is reused.
        /// </summary>
        public void ClearMatrix()
        {
            if (matrix == null ||
                rightHandSide == null)
            {
                return;
            }

            Array.Clear(
                matrix,
                0,
                matrix.Length);

            Array.Clear(
                rightHandSide,
                0,
                rightHandSide.Length);
        }

        // ---------------------------------------------------------------------
        // ITERATION VOLTAGES
        // ---------------------------------------------------------------------

        /// <summary>
        /// Sets the voltage estimate used by nonlinear devices
        /// during the current iteration.
        /// </summary>
        public void SetIterationVoltages(
            float[] voltages)
        {
            iterationVoltages =
                voltages;
        }

        /// <summary>
        /// Returns the current iteration voltage of a graph node.
        /// Reference node is always 0V.
        /// </summary>
        public float GetIterationNodeVoltage(
            int node)
        {
            if (node < 0 ||
                node >= graph.NodeCount)
            {
                return 0f;
            }

            if (node == referenceNode)
                return 0f;

            if (iterationVoltages == null ||
                iterationVoltages.Length != graph.NodeCount)
            {
                return 0f;
            }

            return iterationVoltages[node];
        }

        /// <summary>
        /// Returns voltage difference:
        ///
        /// voltage(positive) - voltage(negative)
        /// </summary>
        public float GetIterationVoltage(
            SparkTerminal positive,
            SparkTerminal negative)
        {
            if (positive == null ||
                negative == null)
            {
                return 0f;
            }

            if (!graph.TryGetNode(
                    positive,
                    out int positiveNode))
            {
                return 0f;
            }

            if (!graph.TryGetNode(
                    negative,
                    out int negativeNode))
            {
                return 0f;
            }

            return
                GetIterationNodeVoltage(
                    positiveNode)
                -
                GetIterationNodeVoltage(
                    negativeNode);
        }

        // ---------------------------------------------------------------------
        // NODE MAPPING
        // ---------------------------------------------------------------------

        /// <summary>
        /// Converts a full graph node to a reduced MNA matrix index.
        ///
        /// Reference node returns -1.
        /// </summary>
        public int MapNode(
            int graphNode)
        {
            if (graphNode < 0 ||
                graphNode >= graph.NodeCount)
            {
                return -1;
            }

            if (graphNode == referenceNode)
                return -1;

            return graphNode < referenceNode
                ? graphNode
                : graphNode - 1;
        }

        /// <summary>
        /// Converts a terminal to its reduced MNA index.
        /// </summary>
        public int MapTerminal(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return -1;

            if (!graph.TryGetNode(
                    terminal,
                    out int node))
            {
                return -1;
            }

            return MapNode(node);
        }

        // ---------------------------------------------------------------------
        // NONLINEAR STATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Returns the persistent nonlinear conduction state
        /// of a component.
        ///
        /// Default is false.
        /// </summary>
        public bool GetNonlinearState(
            SparkElectricalComponent component)
        {
            if (component == null)
                return false;

            return nonlinearStates.TryGetValue(
                component,
                out bool state)
                && state;
        }

        /// <summary>
        /// Stores nonlinear conduction state for a component.
        ///
        /// The state survives iterations and future solves.
        /// </summary>
        public void SetNonlinearState(
            SparkElectricalComponent component,
            bool state)
        {
            if (component == null)
                return;

            nonlinearStates[component] =
                state;
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

            return previousComponentVoltages.TryGetValue(
                component,
                out voltage);
        }

        public float GetPreviousComponentVoltage(
            SparkElectricalComponent component)
        {
            if (component == null)
                return 0f;

            return previousComponentVoltages.TryGetValue(
                component,
                out float voltage)
                ? voltage
                : 0f;
        }

        public void SetPreviousComponentVoltage(
            SparkElectricalComponent component,
            float voltage)
        {
            if (component == null)
                return;

            previousComponentVoltages[component] =
                voltage;
        }

        // ---------------------------------------------------------------------
        // INDEXED VOLTAGE SOURCES
        // ---------------------------------------------------------------------

        /// <summary>
        /// Allocates an auxiliary MNA variable for an indexed switch.
        ///
        /// Allocation happens before matrix creation.
        /// </summary>
      public int RegisterIndexedSource(
    SparkSwitchIndex indexedSwitch)
{
    return RegisterVoltageSource(
        indexedSwitch);
}

      public bool TryGetIndexedSourceIndex(
    SparkSwitchIndex indexedSwitch,
    out int sourceIndex)
{
    return TryGetVoltageSourceIndex(
        indexedSwitch,
        out sourceIndex);
}


public void AddFixedVoltageSource(
    SparkTerminal positiveTerminal,
    SparkTerminal negativeTerminal,
    int sourceIndex,
    float voltage)
{
    if (!IsValidMatrixIndex(sourceIndex))
        return;

    int positive =
        MapTerminal(positiveTerminal);

    int negative =
        MapTerminal(negativeTerminal);

    /*
     * KCL:
     *
     * Positive terminal: -I
     * Negative terminal: +I
     *
     * Voltage constraint:
     *
     * Vpositive - Vnegative = voltage
     */

    if (positive >= 0)
    {
        AddMatrixValue(
            positive,
            sourceIndex,
            -1d);
    }

    if (negative >= 0)
    {
        AddMatrixValue(
            negative,
            sourceIndex,
            +1d);
    }

    if (positive >= 0)
    {
        AddMatrixValue(
            sourceIndex,
            positive,
            +1d);
    }

    if (negative >= 0)
    {
        AddMatrixValue(
            sourceIndex,
            negative,
            -1d);
    }

    AddRightHandSide(
        sourceIndex,
        voltage);
}
        // ---------------------------------------------------------------------
        // MATRIX ASSEMBLY
        // ---------------------------------------------------------------------

        private bool IsValidMatrixIndex(
            int index)
        {
            return matrix != null &&
                   index >= 0 &&
                   index < matrix.GetLength(0);
        }

        private bool IsValidRhsIndex(
            int index)
        {
            return rightHandSide != null &&
                   index >= 0 &&
                   index < rightHandSide.Length;
        }

        public void AddMatrixValue(
            int row,
            int column,
            double value)
        {
            if (!IsValidMatrixIndex(row) ||
                !IsValidMatrixIndex(column))
            {
                return;
            }

            matrix[row, column] += value;
        }

        public void AddRightHandSide(
            int row,
            double value)
        {
            if (!IsValidRhsIndex(row))
                return;

            rightHandSide[row] += value;
        }

        /// <summary>
        /// Stamps a conductance between two terminals.
        ///
        /// G:
        ///
        /// +A -A
        /// -A +A
        /// </summary>
        public void AddConductance(
            SparkTerminal terminalA,
            SparkTerminal terminalB,
            double conductance)
        {
            int a =
                MapTerminal(terminalA);

            int b =
                MapTerminal(terminalB);

            AddConductance(
                a,
                b,
                conductance);
        }

        /// <summary>
        /// Stamps a conductance between reduced node indices.
        /// </summary>
        public void AddConductance(
            int nodeA,
            int nodeB,
            double conductance)
        {
            if (conductance <= 0d)
                return;

            if (nodeA >= 0)
                AddMatrixValue(
                    nodeA,
                    nodeA,
                    conductance);

            if (nodeB >= 0)
                AddMatrixValue(
                    nodeB,
                    nodeB,
                    conductance);

            if (nodeA >= 0 &&
                nodeB >= 0 &&
                nodeA != nodeB)
            {
                AddMatrixValue(
                    nodeA,
                    nodeB,
                    -conductance);

                AddMatrixValue(
                    nodeB,
                    nodeA,
                    -conductance);
            }
        }

        /// <summary>
        /// Stamps a conductance plus an equivalent Norton
        /// source generated by a voltage source.
        ///
        /// Current injection:
        /// positive terminal receives current,
        /// negative terminal returns current.
        /// </summary>
        public void AddConductance(
            SparkTerminal positiveTerminal,
            SparkTerminal negativeTerminal,
            double conductance,
            double sourceVoltage)
        {
            AddConductance(
                positiveTerminal,
                negativeTerminal,
                conductance);

            if (conductance <= 0d ||
                Math.Abs(sourceVoltage) <= 0d)
            {
                return;
            }

            double sourceCurrent =
                conductance *
                sourceVoltage;

            AddCurrentSource(
                positiveTerminal,
                negativeTerminal,
                sourceCurrent);
        }

        /// <summary>
        /// Adds a current source injecting current into A
        /// and removing it from B.
        /// </summary>
        public void AddCurrentSource(
            SparkTerminal terminalA,
            SparkTerminal terminalB,
            double current)
        {
            if (Math.Abs(current) <= 0d)
                return;

            int a =
                MapTerminal(terminalA);

            int b =
                MapTerminal(terminalB);

            if (a >= 0)
                AddRightHandSide(
                    a,
                    current);

            if (b >= 0)
                AddRightHandSide(
                    b,
                    -current);
        }

        /// <summary>
        /// Stamps a linearized forward-drop device:
        ///
        /// V = Vdrop + I * R
        ///
        /// equivalent:
        ///
        /// G(Va - Vb) = I + G*Vdrop
        /// </summary>
        public void AddForwardDrop(
            SparkTerminal positiveTerminal,
            SparkTerminal negativeTerminal,
            double forwardVoltage,
            double resistance)
        {
            resistance =
                Math.Max(
                    MinimumResistance,
                    resistance);

            double conductance =
                1.0 /
                resistance;

            AddConductance(
                positiveTerminal,
                negativeTerminal,
                conductance);

            double equivalentCurrent =
                conductance *
                forwardVoltage;

            AddCurrentSource(
                positiveTerminal,
                negativeTerminal,
                equivalentCurrent);
        }

        // ---------------------------------------------------------------------
        // CONTROLLED VOLTAGE SOURCE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Stamps:
        ///
        /// Vout = Vin * ratio
        ///
        /// using one MNA auxiliary current variable.
        /// </summary>
        public void AddControlledVoltageSource(
            SparkTerminal inputTerminal,
            SparkTerminal outputTerminal,
            int sourceIndex,
            float ratio)
        {
            if (!IsValidMatrixIndex(sourceIndex))
                return;

            int input =
                MapTerminal(inputTerminal);

            int output =
                MapTerminal(outputTerminal);

            // KCL:
            //
            // input  -> -I
            // output -> +I
            //
            if (input >= 0)
            {
                AddMatrixValue(
                    input,
                    sourceIndex,
                    -1d);
            }

            if (output >= 0)
            {
                AddMatrixValue(
                    output,
                    sourceIndex,
                    +1d);
            }

            // Constraint:
            //
            // Vout - ratio * Vin = 0
            //
            if (input >= 0)
            {
                AddMatrixValue(
                    sourceIndex,
                    input,
                    -ratio);
            }

            if (output >= 0)
            {
                AddMatrixValue(
                    sourceIndex,
                    output,
                    +1d);
            }
        }

        // ---------------------------------------------------------------------
        // FLOATING NODE LEAK
        // ---------------------------------------------------------------------

        /// <summary>
        /// Adds a very small conductance from every non-reference
        /// node to the reference node.
        ///
        /// Prevents completely floating matrices from becoming singular.
        /// </summary>
        public void AddFloatingNodeLeaks()
        {
            if (FloatingNodeLeakResistance <= 0f)
                return;

            double conductance =
                1.0 /
                Math.Max(
                    MinimumResistance,
                    FloatingNodeLeakResistance);

            for (int graphNode = 0;
                 graphNode < graph.NodeCount;
                 graphNode++)
            {
                if (graphNode == referenceNode)
                    continue;

                int reduced =
                    MapNode(graphNode);

                if (reduced >= 0)
                {
                    AddMatrixValue(
                        reduced,
                        reduced,
                        conductance);
                }
            }
        }

        // ---------------------------------------------------------------------
        // SOURCE INFORMATION
        // ---------------------------------------------------------------------

        /// <summary>
        /// Finds the maximum active source voltage registered
        /// by the solver definitions.
        ///
        /// This intentionally contains no concrete device type checks.
        /// </summary>
     public float GetMaximumActiveSourceVoltage()
{
    float maximum =
        0f;

    if (registry == null ||
        components == null)
    {
        return maximum;
    }

    for (int i = 0;
         i < components.Count;
         i++)
    {
        SparkElectricalComponent component =
            components[i];

        if (component == null ||
            !component.ElectricalEnabled)
        {
            continue;
        }

        if (!registry.TryGetDefinition(
                component,
                out SparkElectricalSolverDefinition definition))
        {
            continue;
        }

        if (!definition.TryGetSourceVoltage(
                component,
                out float voltage))
        {
            continue;
        }

        maximum =
            Mathf.Max(
                maximum,
                Mathf.Abs(voltage));
    }

    return maximum;
}

        // ---------------------------------------------------------------------
        // COMPONENT TERMINALS
        // ---------------------------------------------------------------------

        public bool TryGetComponentTerminals(
            SparkElectricalComponent component,
            out SparkTerminal[] terminals)
        {
            return graph.TryGetComponentTerminals(
                component,
                out terminals);
        }

        public bool TryGetPrimaryComponentTerminals(
            SparkElectricalComponent component,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            return graph.TryGetPrimaryComponentTerminals(
                component,
                out terminalA,
                out terminalB);
        }

        // ---------------------------------------------------------------------
        // SOLUTION RESTORATION
        // ---------------------------------------------------------------------

        /// <summary>
        /// Restores the complete node voltage array from
        /// the reduced MNA solution.
        /// </summary>
        public void RestoreNodeVoltages(
            double[] reducedSolution,
            float[] nodeVoltages)
        {
            if (nodeVoltages == null ||
                nodeVoltages.Length != graph.NodeCount)
            {
                return;
            }

            Array.Clear(
                nodeVoltages,
                0,
                nodeVoltages.Length);

            if (reducedSolution == null)
                return;

            nodeVoltages[referenceNode] =
                0f;

            for (int graphNode = 0;
                 graphNode < graph.NodeCount;
                 graphNode++)
            {
                if (graphNode == referenceNode)
                    continue;

                int reduced =
                    MapNode(graphNode);

                if (reduced < 0 ||
                    reduced >= reducedSolution.Length)
                {
                    continue;
                }

                nodeVoltages[graphNode] =
                    (float)reducedSolution[reduced];
            }
        }
    }
}



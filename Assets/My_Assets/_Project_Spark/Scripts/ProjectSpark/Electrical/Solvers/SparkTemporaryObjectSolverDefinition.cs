using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Minimal solver definition for SparkTemporaryObject.
    ///
    /// SparkTemporaryObject has no terminals and produces no
    /// electrical behavior.
    ///
    /// This definition exists only so the automatic electrical
    /// component discovery system can safely encounter the object
    /// without producing a missing-definition error.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkTemporaryObjectSolverDefinition",
        menuName =
            "Project Spark/Electrical/Solver Definitions/Temporary Object")]
    public sealed class SparkTemporaryObjectSolverDefinition :
        SparkElectricalSolverDefinition
    {
        public override Type SupportedComponentType =>
            typeof(SparkTemporaryObject);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkTemporaryObject;
        }

        public override void RegisterTopology(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            // No electrical topology.
        }

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            // No electrical stamp.
        }

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            // No electrical state to apply.
        }

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;
            return false;
        }

        public override bool TryGetSourceVoltage(
            SparkElectricalComponent component,
            out float voltage)
        {
            voltage = 0f;
            return false;
        }

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;
            return false;
        }

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            return false;
        }

        public override void ResetRuntimeState(
            SparkElectricalComponent component)
        {
            // No runtime solver state.
        }
    }
}
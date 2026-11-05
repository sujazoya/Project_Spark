using System;
using UnityEngine;
using ProjectSpark.Gameplay;
namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(fileName="SparkLDRSolverDefinition",menuName="Project Spark/Electrical/Solver Definitions/LDR")]
    public sealed class SparkLDRSolverDefinition : SparkElectricalSolverDefinition
    {
        public override Type SupportedComponentType => typeof(SparkLDR);
        public override bool CanHandle(SparkElectricalComponent c) => c is SparkLDR;
        public override bool TryGetTerminals(SparkElectricalComponent c, SparkElectricalNetworkGraph graph,
            out SparkTerminal a, out SparkTerminal b)
        {
            a = null; b = null;
            if (!(c is SparkLDR ldr)) return false;
            a = ldr.TerminalA; b = ldr.TerminalB;
            return a != null && b != null && a != b;
        }
        public override void Stamp(SparkElectricalComponent c, SparkElectricalSolveContext context)
        {
            if (context == null || !(c is SparkLDR ldr) || !ldr.ElectricalEnabled) return;
            if (!TryGetTerminals(ldr, context.Graph, out var a, out var b) ||
                !context.Graph.ContainsTerminal(a) || !context.Graph.ContainsTerminal(b)) return;
            context.AddConductance(a,b, 1.0/Mathf.Max(1f,ldr.EffectiveResistance));
        }
        public override void ApplySolvedState(SparkElectricalComponent c, SparkElectricalSolveResult result)
        {
            if (!(c is SparkLDR ldr) || result == null) return;
            if (!ldr.ElectricalEnabled || !TryGetTerminals(ldr,result.Context.Graph,out var a,out var b) ||
                !result.TryGetVoltage(a,b,out float v))
            { ldr.ApplyElectricalState(new SparkElectricalState(0,0,0,SparkConductionState.NonConducting)); return; }
            float i = v/Mathf.Max(1f,ldr.EffectiveResistance);
            ldr.ApplyElectricalState(new SparkElectricalState(v,i,v*i,
                Mathf.Abs(i)>0.000001f?SparkConductionState.Conducting:SparkConductionState.NonConducting));
        }
        public override bool TryCalculateCurrent(SparkElectricalComponent c,SparkElectricalSolveResult result,out float i)
        { i=0; if (!(c is SparkLDR ldr) || result==null) return false;
          if (!ldr.ElectricalEnabled) return true;
          if (!TryGetTerminals(ldr,result.Context.Graph,out var a,out var b) || !result.TryGetVoltage(a,b,out float v)) return false;
          i=v/Mathf.Max(1f,ldr.EffectiveResistance);return true; }
    }
}

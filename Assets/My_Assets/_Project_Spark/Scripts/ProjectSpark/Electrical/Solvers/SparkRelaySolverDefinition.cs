using System;
using UnityEngine;
using ProjectSpark.Gameplay;
namespace ProjectSpark.Electrical
{
    /// <summary>Educational relay model: resistive coil and COM switching between NO and NC.
    /// No flyback transient, mechanical delay or contact bounce in this first stage.</summary>
    [CreateAssetMenu(fileName="SparkRelaySolverDefinition",menuName="Project Spark/Electrical/Solver Definitions/Relay")]
    public sealed class SparkRelaySolverDefinition : SparkElectricalSolverDefinition
    {
        [SerializeField,Min(1f)] private float coilResistanceOhms=100f;
        [SerializeField,Min(0.0001f)] private float closedContactResistanceOhms=0.05f;
        [SerializeField,Min(1f)] private float openContactResistanceOhms=100000000f;
        public override Type SupportedComponentType=>typeof(SparkRelay);
        public override bool CanHandle(SparkElectricalComponent c)=>c is SparkRelay;
        public override bool TryGetTerminals(SparkElectricalComponent c,SparkElectricalNetworkGraph graph,out SparkTerminal a,out SparkTerminal b)
        {a=null;b=null;if(!(c is SparkRelay r))return false;a=r.CoilPositiveTerminal;b=r.CoilNegativeTerminal;
         return a!=null&&b!=null&&r.CommonTerminal!=null&&r.NormallyOpenTerminal!=null&&r.NormallyClosedTerminal!=null;}
        public override void Stamp(SparkElectricalComponent c,SparkElectricalSolveContext context)
        {if(!(c is SparkRelay r)||context==null||!r.ElectricalEnabled)return;
         if(!TryGetTerminals(r,context.Graph,out var a,out var b))return;
         foreach(var t in new[]{a,b,r.CommonTerminal,r.NormallyOpenTerminal,r.NormallyClosedTerminal})
             if(!context.Graph.ContainsTerminal(t))return;
         context.AddConductance(a,b,1.0/coilResistanceOhms);
         context.Graph.TryGetNode(a,out int na);context.Graph.TryGetNode(b,out int nb);
         bool on=Mathf.Abs(context.GetIterationNodeVoltage(na)-context.GetIterationNodeVoltage(nb))>=r.PickupVoltage;
         context.SetNonlinearState(r,on);
         context.AddConductance(r.CommonTerminal,r.NormallyOpenTerminal,1.0/(on?closedContactResistanceOhms:openContactResistanceOhms));
         context.AddConductance(r.CommonTerminal,r.NormallyClosedTerminal,1.0/(on?openContactResistanceOhms:closedContactResistanceOhms));}
        public override void ApplySolvedState(SparkElectricalComponent c,SparkElectricalSolveResult result)
        {if(!(c is SparkRelay r)||result==null)return;
         if(!r.ElectricalEnabled||!TryGetTerminals(r,result.Context.Graph,out var a,out var b)||!result.TryGetVoltage(a,b,out float v))
         {r.SetEnergized(false);r.ApplyElectricalState(new SparkElectricalState(0,0,0,SparkConductionState.NonConducting));return;}
         float i=v/coilResistanceOhms;
         // Applying the coil state after the solve may queue one topology refresh when contacts change.
         r.SetEnergized(result.GetNonlinearState(r));
         r.ApplyElectricalState(new SparkElectricalState(v,i,v*i,Mathf.Abs(i)>0.000001f?SparkConductionState.Conducting:SparkConductionState.NonConducting));}
        public override bool TryCalculateCurrent(SparkElectricalComponent c,SparkElectricalSolveResult result,out float i)
        {i=0;if(!(c is SparkRelay r)||result==null)return false;
         if(!r.ElectricalEnabled)return true;
         if(!TryGetTerminals(r,result.Context.Graph,out var a,out var b)||!result.TryGetVoltage(a,b,out float v))return false;
         i=v/coilResistanceOhms;return true;}
    }
}

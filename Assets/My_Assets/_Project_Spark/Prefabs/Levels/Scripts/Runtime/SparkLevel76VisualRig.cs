using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// Level 76 procedural-art-only movement/visualization adapter. Does NOT simulate electrode
    /// current, Faraday deposition, bath chemistry, true voids, actual SparkTerminals, safety or objectives.
    /// Each setter MUST be called with actual state from an authoritative gameplay coordinator.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel76VisualRig : MonoBehaviour
    {
        [SerializeField] private Light optionalPlatingGlow;
        [SerializeField] private ParticleSystem optionalElectrolyteCirculationVFX;
        [SerializeField] private Light optionalSafetyLight;
        private readonly Dictionary<string, Transform> pivotLookup = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Vector3> starts = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Quaternion> orientations = new Dictionary<string, Quaternion>();
        private float cathodeRpm, recirculationRpm, mixerRpm, dryerRpm;
        private float lift01, chamberLid01, referenceElectrode01;
        private float robotBaseDeg, robotArmDeg, robotToolDeg;
        private float flow01, current01, fill01, defects01, metrology01;
        private float suppressor01, accelerator01, leveler01;
        private bool platingAuthorized, criticalFault;

        public Transform art;
        public Transform[] pivots;
        public Transform fillSample;
        private Vector3 fillStartScale;

        public void Apply(SparkLevel76Plant plant,float time,float transfer,int cleaning,bool wetting,bool plating,bool critical,float scan,bool emergency)
        {
            float sweep=transfer>0?Mathf.Sin(Mathf.Clamp01(transfer/1.8f)*Mathf.PI):0;
            SetActualRPM(plant.Spindle,plant.Pump*185,plant.Flow*61,plant.DryerSpin);
            SetActualActuatorPositions(plant.Lift,plant.Lid,plant.Probe);
            SetWaferRobotPose(36*sweep,-17*sweep,22*sweep);
            SetActualPlatingVisualState(plant.Flow,plant.Current/1.1f,plant.MeanFill,plant.WorstVoid,plating,critical);
            SetActualAdditiveFlows(plant.Suppressor,plant.Accelerator,plant.Leveler);
            SetMetrologyProgress(scan/1.25f);
            Shift("ANIM_PREWET_CHAMBER",Vector3.up*(wetting?.014f:0));
            Shift("ANIM_ELECTROLYTE_SURFACE",Vector3.up*(Mathf.Sin(time*4)*.002f*plant.Flow));
            Rotate("ANIM_WAFER_CLAMP",new Vector3(0,0,plant.Spindle>5?3:0));
            Rotate("ANIM_FLOW_CONTROLLER",new Vector3(0,20*plant.Flow,0));
            Rotate("ANIM_DISTRIBUTION_MANIFOLD",new Vector3(0,0,plant.LinePressure*6));
            Rotate("ANIM_FLOW_NOZZLE_ARRAY",new Vector3(0,0,plant.Flow*5));
            Shift("ANIM_ANODE_SEGMENT_A",Vector3.up*(.006f*plant.Current));
            Shift("ANIM_ANODE_SEGMENT_B",Vector3.up*(-.006f*plant.Current));
            Rotate("ANIM_CURRENT_DISTRIBUTION_RING",new Vector3(0,10*plant.Current,0));
            Rotate("ANIM_ESTOP",new Vector3(emergency?14:0,0,0));
            Shift("ANIM_RINSE_CHAMBER",Vector3.up*(cleaning==1?.012f:0));
            Shift("ANIM_SEED_CONTINUITY_SCANNER",Vector3.right*(scan*.004f));
            if(fillSample!=null)
            {
                fillSample.gameObject.SetActive(plant.MeanFill>.02f);
                fillSample.localScale=Vector3.Scale(fillStartScale,new Vector3(1,.24f+.76f*plant.MeanFill,1));
            }
        }

        private void Awake()
        {
            foreach (Transform t in pivots??GetComponentsInChildren<Transform>(true))
            {
                int prefix=t.name.IndexOf("ANIM_",System.StringComparison.Ordinal);
                if(prefix<0)continue;
                string id=t.name.Substring(prefix);
                if(!pivotLookup.ContainsKey(id)){pivotLookup.Add(id,t);starts.Add(id,t.localPosition);orientations.Add(id,t.localRotation);}
            }
            if(fillSample!=null)fillStartScale=fillSample.localScale;
        }
                private Transform Find(string id) => pivotLookup.TryGetValue(id, out Transform t) ? t : null;
        private void Shift(string id, Vector3 delta)
        { Transform t=Find(id); if(t!=null)t.localPosition=starts[id]+(art!=null?t.parent.InverseTransformVector(art.TransformVector(delta)):delta); }
        private void Rotate(string id, Vector3 euler)
        { Transform t=Find(id); if(t!=null)t.localRotation=orientations[id]*Quaternion.Euler(euler); }
        private void Spin(string id,float rpm,float dt)
        { Transform t=Find(id); if(t!=null)t.Rotate(Vector3.up,rpm*6f*dt,Space.Self); }

        public void SetActualRPM(float actualCathodeRPM,float actualRecirculationPumpRPM,float actualMixRPM,float actualSpinDryerRPM)
        {
            cathodeRpm=actualCathodeRPM; recirculationRpm=actualRecirculationPumpRPM;
            mixerRpm=actualMixRPM; dryerRpm=actualSpinDryerRPM;
        }
        public void SetActualActuatorPositions(float actualCathodeLift01,float actualChamberLidOpen01,float actualReferenceElectrodeInserted01)
        {
            lift01=Mathf.Clamp01(actualCathodeLift01);
            chamberLid01=Mathf.Clamp01(actualChamberLidOpen01);
            referenceElectrode01=Mathf.Clamp01(actualReferenceElectrodeInserted01);
        }
        public void SetWaferRobotPose(float actualBaseDeg,float actualArmDeg,float actualToolDeg)
        { robotBaseDeg=actualBaseDeg; robotArmDeg=actualArmDeg; robotToolDeg=actualToolDeg; }
        public void SetActualPlatingVisualState(float measuredFlowFraction01,float actualCurrentFraction01,
            float modeledTrenchFillFraction01,float modeledDefectFraction01,bool actualPlatingAuthorized,bool actualCriticalFault)
        {
            flow01=Mathf.Clamp01(measuredFlowFraction01);current01=Mathf.Clamp01(actualCurrentFraction01);
            fill01=Mathf.Clamp01(modeledTrenchFillFraction01);defects01=Mathf.Clamp01(modeledDefectFraction01);
            platingAuthorized=actualPlatingAuthorized;criticalFault=actualCriticalFault;
        }
        public void SetActualAdditiveFlows(float actualSuppressor01,float actualAccelerator01,float actualLeveler01)
        { suppressor01=Mathf.Clamp01(actualSuppressor01);accelerator01=Mathf.Clamp01(actualAccelerator01);leveler01=Mathf.Clamp01(actualLeveler01); }
        public void SetMetrologyProgress(float normalizedActualProgress01)
        { metrology01=Mathf.Clamp01(normalizedActualProgress01); }

        private void Update()
        {
            float dt=Time.deltaTime;
            Spin("ANIM_WAFER_CATHODE_ROTATION",cathodeRpm,dt);
            Spin("ANIM_RECIRCULATION_PUMP",recirculationRpm,dt);
            Spin("ANIM_ELECTROLYTE_MIXER",mixerRpm,dt);
            Spin("ANIM_SPIN_DRYER",dryerRpm,dt);
            Spin("ANIM_FILTRATION_PUMP",recirculationRpm*.4f,dt);
            Shift("ANIM_WAFER_CATHODE_Z",Vector3.up*(lift01*.047f));
            Shift("ANIM_PLATING_CHAMBER_LID",Vector3.up*(chamberLid01*.049f));
            Shift("ANIM_REFERENCE_ELECTRODE",Vector3.up*((1f-referenceElectrode01)*.025f));
            Rotate("ANIM_WAFER_ROBOT_BASE",new Vector3(0f,robotBaseDeg,0f));
            Rotate("ANIM_WAFER_ROBOT_ARM",new Vector3(0f,0f,robotArmDeg));
            Rotate("ANIM_WAFER_ROBOT_TOOL",new Vector3(0f,robotToolDeg,0f));
            Shift("ANIM_SUPPRESSOR_METERING",Vector3.up*(-.006f*suppressor01));
            Shift("ANIM_ACCELERATOR_METERING",Vector3.up*(-.006f*accelerator01));
            Shift("ANIM_LEVELER_METERING",Vector3.up*(-.006f*leveler01));
            Shift("ANIM_FOUR_POINT_PROBE",Vector3.up*(-.015f*metrology01));
            Shift("ANIM_COPPER_THICKNESS_SCANNER",Vector3.right*(.014f*metrology01));
            Shift("ANIM_VOID_INSPECTION_HEAD",Vector3.right*(.014f*metrology01));
            Shift("ANIM_OVERBURDEN_SCANNER",Vector3.right*(.013f*metrology01));
            Shift("ANIM_SURFACE_ROUGHNESS_SCANNER",Vector3.right*(.013f*metrology01));
            Shift("ANIM_TRENCH_FILL_TOMOGRAPHY",Vector3.right*(.014f*fill01));
            bool active=platingAuthorized && !criticalFault && flow01>.01f && current01>.01f;
            if(optionalPlatingGlow!=null)
            { optionalPlatingGlow.enabled=active;optionalPlatingGlow.intensity=2.5f*current01;optionalPlatingGlow.color=Color.Lerp(new Color(1f,.43f,.10f),new Color(.26f,.91f,.95f),defects01*.5f); }
            if(optionalSafetyLight!=null)
            { optionalSafetyLight.enabled=criticalFault;optionalSafetyLight.color=Color.red; }
            if(optionalElectrolyteCirculationVFX!=null)
            {
                if(flow01>.03f && !criticalFault && !optionalElectrolyteCirculationVFX.isPlaying) optionalElectrolyteCirculationVFX.Play();
                else if((flow01<=.03f || criticalFault) && optionalElectrolyteCirculationVFX.isPlaying)
                    optionalElectrolyteCirculationVFX.Stop(true,ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}

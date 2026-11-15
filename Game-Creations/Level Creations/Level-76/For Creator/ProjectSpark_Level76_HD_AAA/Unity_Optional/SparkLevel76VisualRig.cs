using System.Collections.Generic;
using UnityEngine;

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
        private readonly Dictionary<string, Transform> pivots = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Vector3> starts = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Quaternion> orientations = new Dictionary<string, Quaternion>();
        private float cathodeRpm, recirculationRpm, mixerRpm, dryerRpm;
        private float lift01, chamberLid01, referenceElectrode01;
        private float robotBaseDeg, robotArmDeg, robotToolDeg;
        private float flow01, current01, fill01, defects01, metrology01;
        private float suppressor01, accelerator01, leveler01;
        private bool platingAuthorized, criticalFault;

        private void Awake()
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("ANIM_")) continue;
                if (!pivots.ContainsKey(t.name))
                { pivots.Add(t.name, t); starts.Add(t.name, t.localPosition); orientations.Add(t.name, t.localRotation); }
            }
        }
        private Transform Find(string id) => pivots.TryGetValue(id, out Transform t) ? t : null;
        private void Shift(string id, Vector3 delta)
        { Transform t=Find(id); if(t!=null)t.localPosition=starts[id]+delta; }
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
            Shift("ANIM_WAFER_CATHODE_Z",Vector3.up*(lift01*.67f));
            Shift("ANIM_PLATING_CHAMBER_LID",Vector3.up*(chamberLid01*.55f));
            Shift("ANIM_REFERENCE_ELECTRODE",Vector3.up*((1f-referenceElectrode01)*.42f));
            Rotate("ANIM_WAFER_ROBOT_BASE",new Vector3(0f,robotBaseDeg,0f));
            Rotate("ANIM_WAFER_ROBOT_ARM",new Vector3(0f,0f,robotArmDeg));
            Rotate("ANIM_WAFER_ROBOT_TOOL",new Vector3(0f,robotToolDeg,0f));
            Shift("ANIM_SUPPRESSOR_METERING",Vector3.up*(-.09f*suppressor01));
            Shift("ANIM_ACCELERATOR_METERING",Vector3.up*(-.09f*accelerator01));
            Shift("ANIM_LEVELER_METERING",Vector3.up*(-.09f*leveler01));
            Shift("ANIM_FOUR_POINT_PROBE",Vector3.up*(-.24f*metrology01));
            Shift("ANIM_COPPER_THICKNESS_SCANNER",Vector3.right*(.20f*metrology01));
            Shift("ANIM_VOID_INSPECTION_HEAD",Vector3.right*(.20f*metrology01));
            Shift("ANIM_OVERBURDEN_SCANNER",Vector3.right*(.18f*metrology01));
            Shift("ANIM_SURFACE_ROUGHNESS_SCANNER",Vector3.right*(.18f*metrology01));
            Shift("ANIM_TRENCH_FILL_TOMOGRAPHY",Vector3.right*(.22f*fill01));
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

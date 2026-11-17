using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// ART-ONLY Level 88 animation facade. All setters accept measured ACTUAL states.
    /// It has no electrical topology, ECLSS physics, PLC, safety or objective authority.
    /// All pressures/gas rates and numerical qualification are handled externally.
    /// </summary>
    public sealed class SparkLevel88VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform artRoot;
        [SerializeField] private Light atmosphereHealthyLight;
        [SerializeField] private Light lowOxygenWarningLight;
        [SerializeField] private Light criticalW145FaultLight;
        [SerializeField] private bool autoBind=true;
        private readonly Dictionary<string,Transform> pivots=new Dictionary<string,Transform>();
        private readonly Dictionary<string,Quaternion> homeRotations=new Dictionary<string,Quaternion>();
        private readonly Dictionary<string,Vector3> homePositions=new Dictionary<string,Vector3>();
        private float mainFan01,backupFan01,adsA01,adsB01,waterPump01,oxygenFlow01,condenserFlow01;
        private readonly float[] dampers=new float[4];
        private float airlockOpen01,CO2ValveAngle,oxygenValveAngle,pressureRegAngle,waterFilter01;
        private bool validW145,criticalFault,atmosphereSafe,lowOxygen;
        private void Awake()
        {
            if(artRoot==null)artRoot=transform.Find("L88_HD_VISUAL_ART");
            if(artRoot==null)artRoot=transform;
            if(autoBind)BindVisuals();
        }
        public void BindVisuals()
        {
            pivots.Clear();homeRotations.Clear();homePositions.Clear();
            foreach(Transform t in artRoot.GetComponentsInChildren<Transform>(true))
            {
                if(t.name.StartsWith("ANIM_")&&!pivots.ContainsKey(t.name))pivots.Add(t.name,t);
            }
            foreach(var entry in pivots)
            {
                homeRotations[entry.Key]=entry.Value.localRotation;
                homePositions[entry.Key]=entry.Value.localPosition;
            }
        }
        private void Rotate(string name,Vector3 axis,float degrees)
        {
            Transform p;Quaternion home;
            if(pivots.TryGetValue(name,out p) && homeRotations.TryGetValue(name,out home))
                p.localRotation=home*Quaternion.AngleAxis(degrees,axis);
        }
        private void Move(string name,Vector3 offset)
        {
            Transform p;Vector3 home;
            if(pivots.TryGetValue(name,out p)&&homePositions.TryGetValue(name,out home))p.localPosition=home+offset;
        }
        public void SetActualCO2Processing(float actualBedASaturation01,float actualBedBSaturation01,float actualValveAngleDegrees)
        {
            adsA01=Mathf.Clamp01(actualBedASaturation01);adsB01=Mathf.Clamp01(actualBedBSaturation01);
            CO2ValveAngle=Mathf.Clamp(actualValveAngleDegrees,-180f,180f);
        }
        public void SetActualOxygenProcess(float actualDeliveryFlow01,float actualRegulatorAngleDegrees)
        {
            oxygenFlow01=Mathf.Clamp01(actualDeliveryFlow01);
            oxygenValveAngle=Mathf.Clamp(actualRegulatorAngleDegrees,0f,90f);
        }
        public void SetActualWaterAndHumidity(float actualWaterPumpSpeed01,float actualCondenserFlow01,float actualFilterDrawerOpen01)
        {
            waterPump01=Mathf.Clamp01(actualWaterPumpSpeed01);
            condenserFlow01=Mathf.Clamp01(actualCondenserFlow01);
            waterFilter01=Mathf.Clamp01(actualFilterDrawerOpen01);
        }
        public void SetActualVentilation(float actualMainFanSpeed01,float actualBackupFanSpeed01,
            float actualDamperAOpen01,float actualDamperBOpen01,float actualDamperCOpen01,float actualDamperDOpen01)
        {
            mainFan01=Mathf.Clamp01(actualMainFanSpeed01);backupFan01=Mathf.Clamp01(actualBackupFanSpeed01);
            dampers[0]=Mathf.Clamp01(actualDamperAOpen01);dampers[1]=Mathf.Clamp01(actualDamperBOpen01);
            dampers[2]=Mathf.Clamp01(actualDamperCOpen01);dampers[3]=Mathf.Clamp01(actualDamperDOpen01);
        }
        public void SetActualSafety(bool actualW145TrustedAndFresh,bool actualCriticalFaultLatched,bool actualAtmosphereQualified,bool actualLowOxygen,bool actualAirlockOpen,float actualPressureRegulatorDegrees)
        {
            validW145=actualW145TrustedAndFresh;criticalFault=actualCriticalFaultLatched;
            atmosphereSafe=actualAtmosphereQualified;lowOxygen=actualLowOxygen;
            airlockOpen01=actualAirlockOpen?1f:0f;pressureRegAngle=Mathf.Clamp(actualPressureRegulatorDegrees,0f,90f);
        }
        private float fanRotA,fanRotB,pumpRot;
        private void LateUpdate()
        {
            if(pivots.Count==0)return;
            fanRotA=Mathf.Repeat(fanRotA+mainFan01*Time.deltaTime*650f,360f);
            fanRotB=Mathf.Repeat(fanRotB+backupFan01*Time.deltaTime*650f,360f);
            pumpRot=Mathf.Repeat(pumpRot+waterPump01*Time.deltaTime*360f,360f);
            Rotate("ANIM_PRIMARY_BLOWER_ROTOR",Vector3.forward,fanRotA);
            Rotate("ANIM_BACKUP_BLOWER_ROTOR",Vector3.forward,fanRotB);
            Rotate("ANIM_RECOVERED_WATER_PUMP",Vector3.forward,pumpRot);
            Rotate("ANIM_WATER_FEED_PUMP",Vector3.forward,pumpRot);
            Rotate("ANIM_ADSORBER_SWITCHING_MANIFOLD",Vector3.up,CO2ValveAngle);
            Rotate("ANIM_O2_DELIVERY_REGULATOR",Vector3.up,oxygenValveAngle);
            Rotate("ANIM_PRESSURE_REGULATOR",Vector3.up,pressureRegAngle);
            string[] names={"ANIM_DAMPER_A","ANIM_DAMPER_B","ANIM_DAMPER_C","ANIM_DAMPER_D"};
            for(int i=0;i<4;i++)Rotate(names[i],Vector3.forward,dampers[i]*90f);
            Move("ANIM_ADSORBENT_A_COLUMN_SCAN",Vector3.up*(adsA01*.3f));
            Move("ANIM_ADSORBENT_B_COLUMN_SCAN",Vector3.up*(adsB01*.3f));
            Move("ANIM_WATER_FILTER_DRAWER",Vector3.right*(waterFilter01*.33f));
            Move("ANIM_AIRLOCK_DOOR",Vector3.right*(airlockOpen01*.50f));
            if(atmosphereHealthyLight!=null)atmosphereHealthyLight.enabled=atmosphereSafe&&!criticalFault;
            if(lowOxygenWarningLight!=null)lowOxygenWarningLight.enabled=lowOxygen;
            if(criticalW145FaultLight!=null)criticalW145FaultLight.enabled=criticalFault||!validW145;
        }
    }
}

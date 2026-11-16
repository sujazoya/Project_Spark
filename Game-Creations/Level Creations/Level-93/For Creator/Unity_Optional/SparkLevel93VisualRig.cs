using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProjectSpark.Level93.Visuals
{
    /// <summary>Visual art facade ONLY. Must receive measured/authoritative values from a
    /// separately implemented level simulator. Does not decide objectives or safety.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel93VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform artRoot;
        private readonly Dictionary<string,Transform> pivots=new Dictionary<string,Transform>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,Quaternion> baseRotation=new Dictionary<string,Quaternion>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,Vector3> basePosition=new Dictionary<string,Vector3>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,float> measuredAngle=new Dictionary<string,float>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,Vector3> measuredDisplacement=new Dictionary<string,Vector3>(StringComparer.OrdinalIgnoreCase);
        private void Awake(){ if(artRoot==null)artRoot=transform.Find("L93_HD_VISUAL_ART");if(artRoot==null)artRoot=transform;Rebind(); }
        public void Rebind()
        {
            pivots.Clear();baseRotation.Clear();basePosition.Clear();
            foreach(var t in artRoot.GetComponentsInChildren<Transform>(true))
            {
                if(!t.name.StartsWith("ANIM_",StringComparison.OrdinalIgnoreCase)||pivots.ContainsKey(t.name))continue;
                pivots.Add(t.name,t);baseRotation.Add(t.name,t.localRotation);basePosition.Add(t.name,t.localPosition);
            }
        }
        public void SetActualAngleDegrees(string pivotName,float measuredDegrees){measuredAngle[pivotName]=measuredDegrees;}
        public void SetActualDisplacement(string pivotName,Vector3 measuredMeters){measuredDisplacement[pivotName]=measuredMeters;}
        private void LateUpdate()
        {
            foreach(var p in measuredAngle)
            {
                Transform t;Quaternion home;
                if(pivots.TryGetValue(p.Key,out t)&&baseRotation.TryGetValue(p.Key,out home))
                    t.localRotation=home*Quaternion.AngleAxis(p.Value,Vector3.up);
            }
            foreach(var p in measuredDisplacement)
            {
                Transform t;Vector3 home;
                if(pivots.TryGetValue(p.Key,out t)&&basePosition.TryGetValue(p.Key,out home))
                    t.localPosition=home+p.Value;
            }
        }
    }
}

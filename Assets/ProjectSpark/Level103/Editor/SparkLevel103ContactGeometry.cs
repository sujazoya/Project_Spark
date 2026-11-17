#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.EditorTools
{
 internal static class SparkLevel103ContactGeometry
 {
  internal static void Align(SparkLevel103Design design,Dictionary<string,GameObject> modules,Transform art)
  {
   var lines=new List<string>{"Contact,SourceMesh,X_mm,Y_mm,Z_mm,Policy"};
   foreach(var part in design.parts)
   {
    var pins=design.contacts.Where(c=>c.reference==part.reference).ToArray();
    var renderers=modules[part.model].GetComponentsInChildren<Renderer>(true);
    bool Conductive(Renderer r)=>r.name.Contains("Gullwing_Lead")||r.name.Contains("Pad_Solder_Fillet")||r.name.Contains("Sensor_Edge_Contact")||r.name.Contains("Plated_Pin")||r.name.Contains("Solder_Contact");
    var contacts=renderers.Where(Conductive).OrderBy(r=>r.name,StringComparer.Ordinal).ToArray();
    if(part.reference is "J1" or "J2")contacts=contacts.Take(2).ToArray(); // Other artwork pins remain decorative NC.
    if(part.reference!="RV1" && contacts.Length!=pins.Length)throw new InvalidOperationException(part.reference+": source geometry has "+contacts.Length+" contacts; expected "+pins.Length);
    for(int i=0;i<pins.Length;i++)
    {
     Vector3 point;string source,policy="SOURCE_CONDUCTIVE_MESH";
     if(part.reference=="RV1" && i==1)
     {
      // The supplied trimmer art exposes only its two end contacts. Add a fixed, accessible wiper land.
      point=new Vector3(part.xMm,.9f,part.zMm+part.depthMm*.5f+.7f);source="Added RV1 wiper land";policy="EDUCATIONAL_ADDED_WIPER_CONTACT";
     }
     else
     {
      int index=part.reference=="RV1"?(i==0?0:1):i;
      point=art.InverseTransformPoint(contacts[index].bounds.center)*1000f;source=contacts[index].name;
     }
     pins[i].positionMm=point;
     lines.Add(string.Join(",",pins[i].id,source,point.x.ToString("F5",CultureInfo.InvariantCulture),point.y.ToString("F5",CultureInfo.InvariantCulture),point.z.ToString("F5",CultureInfo.InvariantCulture),policy));
    }
   }
   EditorUtility.SetDirty(design);
   File.WriteAllLines("Assets/ProjectSpark/Level103/Data/Resolved_Contact_Geometry.csv",lines);
  }
 }
}
#endif

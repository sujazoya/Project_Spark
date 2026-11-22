using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProjectSpark.Gameplay
{
 [CreateAssetMenu(menuName="Project Spark/Level 104/Voltage Guardian Design")]
 public sealed class SparkLevel104Design:ScriptableObject
 {
  public string revision="VGR5-EDU-1";
  [TextArea]public string provenance="User-authorized generic educational schematic; source Level 97 concept retained separately. No commercial IC pin compatibility claimed.";
  public bool drcVerified; public Part[] parts;public Contact[] contacts;public Route[] routes;public Branch[] factory;public Via[] vias;
  public bool IsResolved(out string reason)
  {
   reason="BLOCKED_UNVERIFIED_NETLIST";
   if(revision!="VGR5-EDU-1"||parts?.Length!=50||routes?.Length!=67||vias?.Length!=14)return false;
   var pins=new Dictionary<string,Contact>();
   foreach(var p in contacts)if(string.IsNullOrEmpty(p.id)||!pins.TryAdd(p.id,p)||string.IsNullOrEmpty(p.net))return false;
   foreach(var r in routes)
   {
    if(!pins.TryGetValue(r.from,out var a)||!pins.TryGetValue(r.to,out var b)||r.net.Contains("UNASSIGNED"))return false;
    if(r.id!="W067"&&(a.net!=r.net||b.net!=r.net))return false;
    if(a.role=="NC"||b.role=="NC")return false;
   }
   foreach(var b in factory)if(!pins.ContainsKey(b.from)||!pins.ContainsKey(b.to)||pins[b.from].net!=pins[b.to].net)return false;
   foreach(var v in vias)if(v.startLayer!=1||v.endLayer!=4||string.IsNullOrEmpty(v.net))return false;
   reason="";return true;
  }
  [Serializable]public sealed class Part {public string reference,kind,value,model;public float xMm,zMm,widthMm,depthMm;}
  [Serializable]public sealed class Contact {public string id,reference,net,role,sourceMesh;public Vector3 positionMm;}
  [Serializable]public sealed class Route {public string id,from,to,net,viaA,viaB;public float widthMm=.20f,clearanceMm=.15f;public Vector3[][] geometry;}
  [Serializable]public sealed class Branch {public string from,to;}
  [Serializable]public sealed class Via {public string id,net;public int startLayer=1,endLayer=4;public Vector3 positionMm;}
 }
 [Serializable]public sealed class SparkLevel104Evidence {public int objective,revision,samples;public double timestamp,started;public string proof;}
}


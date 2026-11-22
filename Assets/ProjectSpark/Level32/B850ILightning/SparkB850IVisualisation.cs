using System.Collections.Generic;
using UnityEngine;
namespace ProjectSpark.Level32.B850I {
 public sealed class SparkB850IVisualisation : MonoBehaviour {
  readonly List<Transform> parts=new List<Transform>(); readonly List<Vector3> origins=new List<Vector3>(); readonly List<int> stages=new List<int>(); Quaternion originalRotation; bool cached;
  void Awake(){Cache();}
  void Cache(){if(cached)return;cached=true;originalRotation=transform.localRotation; foreach(Transform t in transform){if(t.name=="DiagnosticTargets_VisualOnly"||t.name=="PCB")continue;parts.Add(t);origins.Add(t.localPosition);stages.Add(IsBack(t.name)?-2:IsArmor(t.name)?3:t.name.StartsWith("VRM_")||t.name.StartsWith("DRMOS")?2:1);}}
  static bool IsBack(string n)=>n.StartsWith("BACK_")||n.StartsWith("M2_2");
  static bool IsArmor(string n)=>n.Contains("HEATSINK")||n.Contains("ARMOR");
  public void SetExplosion(float amount){Cache();amount=Mathf.Clamp01(amount);for(int i=0;i<parts.Count;i++)if(parts[i]){var p=origins[i];parts[i].localPosition=p+new Vector3(p.x*.16f,stages[i]*.022f,p.z*.16f)*amount;}}
  public void SetArmorVisible(bool visible){Cache();foreach(var p in parts)if(p&&IsArmor(p.name))p.gameObject.SetActive(visible);}
  public void SetRearView(bool rear){Cache();transform.localRotation=originalRotation*(rear?Quaternion.Euler(180,0,0):Quaternion.identity);}
  public void SetIsolation(int mode){Cache();foreach(var p in parts)if(p){string n=p.name;bool power=n.StartsWith("VRM_")||n.StartsWith("DRMOS")||n.StartsWith("POLYMER")||n.StartsWith("ATX")||n.StartsWith("PWM");bool storage=n.StartsWith("M2_")||n.StartsWith("SATA")||n.StartsWith("PCIE");p.gameObject.SetActive(mode==0||(mode==1&&power)||(mode==2&&storage)||(mode==3&&IsBack(n)));}}
  public void ResetView(){SetExplosion(0);SetIsolation(0);SetRearView(false);}
 }
}

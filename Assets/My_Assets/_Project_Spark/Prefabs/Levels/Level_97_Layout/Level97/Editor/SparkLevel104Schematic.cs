#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.Globalization;using System.IO;using System.Linq;
using UnityEditor;using UnityEngine;using ProjectSpark.Gameplay;
namespace ProjectSpark.EditorTools
{
 internal static class SparkLevel104Schematic
 {
  internal static SparkLevel104Design Build(string root,Dictionary<string,GameObject> models,Transform art)
  {
   string path=root+"/Data/VGR5_Educational_Design.asset";
   var d=AssetDatabase.LoadAssetAtPath<SparkLevel104Design>(path);bool create=!d;if(create)d=ScriptableObject.CreateInstance<SparkLevel104Design>();
   var bom=File.ReadAllLines(root+"/Data/Component_BOM_50.csv").Skip(1).Where(s=>s.Length>0).Select(s=>s.Split(',')).ToArray();
   float F(string s)=>float.Parse(s,CultureInfo.InvariantCulture);
   var contacts=new List<SparkLevel104Design.Contact>();var parts=new List<SparkLevel104Design.Part>();
   foreach(var row in bom)
   {
    string id=row[0],model=Path.GetFileNameWithoutExtension(row[3]);var go=models[model];
    var rs=go.GetComponentsInChildren<Renderer>(true);var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
    parts.Add(new(){reference=id,kind=row[1],model=model,xMm=F(row[4])*1000,zMm=F(row[5])*1000,widthMm=b.size.x/art.lossyScale.x*1000,depthMm=b.size.z/art.lossyScale.z*1000,value=Value(id)});
    bool Conductive(Renderer r)=>id.StartsWith("U")?r.name.Contains("_solder_"):
      id.StartsWith("Q")?r.name.Contains("_leg_"):
      id.StartsWith("J")?r.name.Contains("_foot_"):
      id=="W067"?r.name.Contains("metal_contact"):
      r.name.Contains("_solder_")||r.name.Contains("_fillet_")||r.name.Contains("_pad_");
    var pads=rs.Where(Conductive).OrderBy(r=>r.name,StringComparer.Ordinal).ToArray();
    int count=id.StartsWith("U")?id is "U1" or "U8"?20:14:id.StartsWith("Q")?3:id.StartsWith("J")?6:2;
    if(pads.Length!=count && id is not("C1" or "C2" or "C8"))throw new InvalidOperationException(id+" contacts "+pads.Length+" expected "+count);
    for(int i=0;i<count;i++)
    {
     Vector3 p=id is "C1" or "C2" or "C8"?new Vector3(F(row[4])*1000+(i==0?-2:2),.92f,F(row[5])*1000):art.InverseTransformPoint(pads[i].bounds.center)*1000;
     contacts.Add(new(){id=P(id,i+1),reference=id,net="NC_"+P(id,i+1),role="NC",positionMm=p,sourceMesh=pads.Length==count?pads[i].name:"AUTHORED_RADIAL_CAP_LAND"});
    }
   }
   d.parts=parts.ToArray();d.contacts=contacts.ToArray();
   var pins=contacts.ToDictionary(p=>p.id);var routes=new List<SparkLevel104Design.Route>();var factory=new List<SparkLevel104Design.Branch>();
   void Assign(string p,string net){var c=pins[p];if(c.role!="NC"&&c.net!=net)throw new InvalidOperationException(p+" net conflict "+c.net+" / "+net);c.net=net;c.role="passive";}
   void Route(string a,string b,string net){Assign(a,net);Assign(b,net);routes.Add(new(){id="W"+(routes.Count+1).ToString("000"),from=a,to=b,net=net});}
   void Factory(string a,string b,string net){Assign(a,net);Assign(b,net);factory.Add(new(){from=a,to=b});}
   Route(P("J1",1),P("D1",1),"VIN12");
   Route(P("D1",2),P("U4",1),"REVERSE_PROTECTED");
   Route(P("U4",3),P("R1",1),"PROTECTED12");
   Route(P("R1",2),P("U5",1),"SHUNT12");
   Route(P("U5",2),P("Q1",2),"SENSED12");
   Route(P("Q1",3),P("U1",1),"BUCK_INPUT");
   Route(P("U1",3),P("L1",1),"SW5");
   Route(P("L1",2),P("U7",3),"V5");
   Route(P("U7",3),P("Q4",2),"V5");
   Route(P("Q4",3),P("J2",1),"SAFE5");
   Route(P("U1",2),P("J1",2),"GND");
   Route(P("U4",2),P("J1",2),"GND");
   Route(P("U7",3),P("U2",1),"V5");
   Route(P("U2",3),P("L2",1),"SW33");
   Route(P("L2",2),P("U3",1),"V33");
   Route(P("U3",3),P("L3",1),"SW18");
   Route(P("L3",2),P("J2",3),"V18");
   Route(P("L2",2),P("J2",2),"V33");
   Route(P("U2",2),P("J1",2),"GND");
   Route(P("U3",2),P("J1",2),"GND");
   Route(P("U6",1),P("L2",2),"V33");
   Route(P("U6",2),P("J1",2),"GND");
   Route(P("U7",1),P("L2",2),"V33");
   Route(P("U7",2),P("J1",2),"GND");
   Route(P("U8",1),P("L2",2),"V33");
   Route(P("U8",2),P("J1",2),"GND");
   Route(P("U7",4),P("W067",1),"PG_OUT");
   Route(P("W067",2),P("U8",3),"PG_IN");
   Route(P("U8",4),P("Q4",1),"SAFE_ENABLE");
   Route(P("U6",3),P("U1",5),"EN5");
   Route(P("U6",4),P("U2",4),"EN33");
   Route(P("U6",5),P("U3",4),"EN18");
   Route(P("U7",3),P("R7",1),"V5");
   Route(P("R7",2),P("U1",4),"FB5");
   Route(P("R8",1),P("U1",4),"FB5");
   Route(P("R8",2),P("J1",2),"GND");
   Route(P("J2",4),P("J1",2),"GND");
   Route(P("U5",4),P("J1",2),"GND");
   Route(P("U5",3),P("U6",6),"CURRENT_MONITOR");
   Route(P("D2",1),P("J1",2),"GND");
   Route(P("D2",2),P("L1",1),"SW5");
   Route(P("D3",1),P("L2",2),"V33");
   Route(P("D3",2),P("U6",7),"DIAGNOSTIC_SUPPLY");
   Route(P("Q2",1),P("U6",4),"EN33");
   Route(P("Q3",1),P("U6",5),"EN18");
   // Capacitor supply branches are player tasks; their individual return branches are documented factory copper.
   for(int i=1;i<=14;i++)
   {
    string anchor=i==1?P("J1",1):i%3==0?P("L3",2):i%3==1?P("L2",2):P("U7",3);
    string net=i==1?"VIN12":i%3==0?"V18":i%3==1?"V33":"V5";
    Route(P("C"+i,1),anchor,net);Factory(P("C"+i,2),P("J1",2),"GND");
   }
   Route(P("R9",1),P("U7",3),"V5");
   Route(P("R9",2),P("LED1",1),"MAIN_LED");
   Route(P("LED1",2),P("J1",2),"GND");
   Route(P("R10",1),P("U8",4),"SAFE_ENABLE");
   Route(P("R10",2),P("LED2",1),"SAFE_LED");
   Route(P("LED2",2),P("J1",2),"GND");
   Route(P("Q1",1),P("U4",4),"INPUT_ENABLE");
   if(routes.Count!=66)throw new InvalidOperationException("Expected 66 authored tasks; got "+routes.Count);
   Factory(P("Q2",2),P("U2",1),"V5");Factory(P("Q2",3),P("U2",5),"AUX33_INPUT");
   Factory(P("Q3",2),P("U3",1),"V33");Factory(P("Q3",3),P("U3",5),"AUX18_INPUT");
   void Passive(string id,string hi,string net,string lo,string low){Factory(P(id,1),hi,net);Factory(P(id,2),lo,low);}
   Passive("R2",P("U6",3),"EN5",P("J1",2),"GND");
   Passive("R3",P("U6",4),"EN33",P("J1",2),"GND");
   Passive("R4",P("L2",2),"V33",P("J1",2),"GND");
   Passive("R5",P("L3",2),"V18",P("J1",2),"GND");
   Passive("R6",P("U6",5),"EN18",P("J1",2),"GND");
   Passive("R11",P("U7",4),"PG_OUT",P("J1",2),"GND");
   Passive("R12",P("U8",3),"PG_IN",P("J1",2),"GND");
   Passive("R13",P("J2",1),"SAFE5",P("J1",2),"GND");
   var viaRows=File.ReadAllLines(root+"/Data/Via_Plan_14.csv").Skip(1).Select(l=>l.Split(',')).ToArray();
   d.vias=new SparkLevel104Design.Via[14];
   int[] viaRoute={0,1,7,13,15,26,27};
   for(int i=0;i<14;i++)
   {
    var row=viaRows[i];var route=routes[viaRoute[i/2]];string id="V"+(i+1).ToString("00");
    d.vias[i]=new(){id=id,net=route.net,positionMm=new Vector3(F(row[1])*1000,.8f,F(row[2])*1000)};
    if(i%2==0)route.viaA=id;else route.viaB=id;
   }
   routes.Add(new(){id="W067",from=P("W067",1),to=P("W067",2),net="TRUSTED_POWER_GOOD_LINK"});
   d.routes=routes.ToArray();d.factory=factory.ToArray();
   if(!d.IsResolved(out string error))throw new InvalidOperationException(error);
   if(create)AssetDatabase.CreateAsset(d,path);else EditorUtility.SetDirty(d);
   var csv=new List<string>{"Pad,Reference,EducationalPinType,Net,X_mm,Y_mm,Z_mm,SourceMesh,Revision"};
   csv.AddRange(contacts.Select(c=>$"{c.id},{c.reference},{c.role},{c.net},{c.positionMm.x.ToString("F5",CultureInfo.InvariantCulture)},{c.positionMm.y.ToString("F5",CultureInfo.InvariantCulture)},{c.positionMm.z.ToString("F5",CultureInfo.InvariantCulture)},{c.sourceMesh},VGR5-EDU-1"));
   File.WriteAllLines(root+"/Data/Resolved_Pin_Netlist.csv",csv);
   File.WriteAllLines(root+"/Data/Resolved_Routes.csv",new[]{"Task,From,To,Net,ViaA,ViaB,SourcePlaceholderResolvedBy"}.Concat(routes.Select(r=>$"{r.id},{r.from},{r.to},{r.net},{r.viaA},{r.viaB},User-approved VGR5-EDU-1")));
   return d;
  }
  internal static string P(string id,int p)=>id+".P"+p.ToString("00");
  static string Value(string id)=>id switch{"R1"=>"0.1 ohm current shunt","R7"=>"52.5 kohm feedback upper","R8"=>"10 kohm feedback lower","R9" or "R10"=>"1 kohm indicator","L1"=>"100 uH / 35 mohm winding","L2" or "L3"=>"22 uH secondary winding","C1"=>"220 uF / 25 V","C2"=>"470 uF / 16 V","W067"=>"Removable trusted PG logic link",_=>id.StartsWith("C")?"10 uF / 16 V":id.StartsWith("R")?"10 kohm":id.StartsWith("U")?"VGR5-EDU generic functional IC":id.StartsWith("Q")?"Generic MOSFET educational G/D/S":id.StartsWith("D")?"Schottky 0.3 V":id.StartsWith("LED")?"Indicator 2.1 V":"6-way educational connector"};
 }
}
#endif


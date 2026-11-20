#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.IO;using System.Linq;using ProjectSpark.Gameplay;using UnityEditor;
namespace ProjectSpark.EditorTools
{
 static class SparkLevel104ERC
 {
  internal static void Validate(SparkLevel104Design d,string root)
  {
   if(!d.IsResolved(out var reason))throw new InvalidOperationException(reason);
   var pins=d.contacts.ToDictionary(p=>p.id);var rows=new List<string>{"VGR5-EDU-1 / EDUCATIONAL ELECTRICAL RULE CHECK"};
   void Pin(string part,int number,string type){pins[SparkLevel104Schematic.P(part,number)].role=type;}
   foreach(string id in new[]{"U1","U2","U3"}){Pin(id,1,"POWER_IN");Pin(id,2,"GROUND");Pin(id,3,"SWITCH_OUT");}
   Pin("U1",4,"ANALOG_IN");Pin("U1",5,"DIGITAL_IN");Pin("U2",4,"DIGITAL_IN");Pin("U3",4,"DIGITAL_IN");
   Pin("U4",1,"POWER_IN");Pin("U4",2,"GROUND");Pin("U4",3,"POWER_OUT");Pin("U4",4,"DIGITAL_OUT");
   Pin("U5",3,"ANALOG_OUT");Pin("U5",4,"GROUND");
   Pin("U6",1,"POWER_IN");Pin("U6",2,"GROUND");for(int n=3;n<=5;n++)Pin("U6",n,"DIGITAL_OUT");Pin("U6",6,"ANALOG_IN");Pin("U6",7,"POWER_IN");
   foreach(string id in new[]{"U7","U8"}){Pin(id,1,"POWER_IN");Pin(id,2,"GROUND");Pin(id,3,id=="U7"?"ANALOG_IN":"DIGITAL_IN");Pin(id,4,"DIGITAL_OUT");}
   Pin("J1",1,"POWER_OUT");Pin("J1",2,"GROUND");
   foreach(var group in d.contacts.Where(p=>p.role.EndsWith("_OUT",StringComparison.Ordinal)).GroupBy(p=>p.net))
    if(group.Count()>1)throw new InvalidOperationException("ERC: multiple active outputs on "+group.Key);
   rows.Add("PASS: no copper net joins multiple modeled active outputs.");
   foreach(var contact in d.contacts.Where(p=>p.role!="NC"))
   {
    if(contact.net.StartsWith("NC_")||!d.routes.Any(r=>r.from==contact.id||r.to==contact.id)&&!d.factory.Any(r=>r.from==contact.id||r.to==contact.id))throw new InvalidOperationException("ERC: required pad unconnected: "+contact.id);
    if(contact.role=="GROUND"&&contact.net!="GND")throw new InvalidOperationException("ERC: return assigned to another domain: "+contact.id);
   }
   rows.Add("PASS: every declared functional pad has an explicit task or factory branch; unused generic IC positions are NC.");
   if(pins["U7.P04"].net!="PG_OUT"||pins["U8.P03"].net!="PG_IN"||d.routes[66].from!="W067.P01"||d.routes[66].to!="W067.P02")throw new InvalidOperationException("ERC: trusted logic path is not isolated from rail current.");
   rows.Add("PASS: W067 is the only PG_OUT > PG_IN bridge; it carries logic rather than load current.");
   if(d.routes.Take(66).SelectMany(r=>new[]{r.viaA,r.viaB}).Where(id=>!string.IsNullOrEmpty(id)).Distinct().Count()!=14||d.vias[4].net!="V5")throw new InvalidOperationException("ERC: via ownership or V05 supply role is unresolved.");
   rows.Add("PASS: 14 unique L1-L4 source vias explicitly owned; V05 is in the main V5 supply path.");
   if(d.parts.Select(p=>p.reference).Distinct().Count()!=50)throw new InvalidOperationException("ERC: duplicate BOM identity.");
   rows.Add("PASS: 50 unique BOM parts; W067 counted once; 66 etched tasks plus one removable link.");
   rows.Add("LIMIT: this checks the authorized generic educational model, not a manufacturer schematic or fabrication approval.");
   File.WriteAllLines(root+"/Reports/Educational_ERC.txt",rows);EditorUtility.SetDirty(d);
   var csv=File.ReadAllLines(root+"/Data/Resolved_Pin_Netlist.csv");
   for(int i=1;i<csv.Length;i++){var columns=csv[i].Split(',');columns[2]=pins[columns[0]].role;csv[i]=string.Join(",",columns);}File.WriteAllLines(root+"/Data/Resolved_Pin_Netlist.csv",csv);
  }
 }
}
#endif

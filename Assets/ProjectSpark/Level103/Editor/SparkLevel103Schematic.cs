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
    internal static class SparkLevel103Schematic
    {
        const string Folder="Assets/ProjectSpark/Level103";
        internal static SparkLevel103Design Build()
        {
            var design=AssetDatabase.LoadAssetAtPath<SparkLevel103Design>(Folder+"/Data/SMX4_Educational_Design.asset");
            bool create=!design;
            if(create)design=ScriptableObject.CreateInstance<SparkLevel103Design>();
            string[][] Rows(string file)=>File.ReadAllLines(Folder+"/Data/"+file+".csv").Skip(1)
                .Where(line=>!string.IsNullOrWhiteSpace(line)).Select(line=>line.Split(',')).ToArray();
            float Number(string v)=>float.Parse(v,CultureInfo.InvariantCulture);
            var manifest=Rows("Asset_Manifest_62");
            design.parts=Rows("Component_BOM_44").Select(row=>new SparkLevel103Design.Part {
                reference=row[0],kind=row[1],value=row[2],xMm=Number(row[3]),zMm=Number(row[4]),
                widthMm=Number(row[5]),depthMm=Number(row[6]),
                model=manifest.Single(m=>m[1].StartsWith("L96_"+row[0]+"_",StringComparison.Ordinal) ||
                    (row[0]=="W059" && m[1]=="L96_W059_Jumper"))[1]
            }).ToArray();
            var sourceContacts=Rows("Component_Contact_Plan");
            var assigned=new Dictionary<string,string>();
            var routes=new List<SparkLevel103Design.Route>();
            var factory=new List<SparkLevel103Design.Branch>();
            void Assign(string pin,string net)
            {
                if(assigned.TryGetValue(pin,out var previous) && previous!=net)
                    throw new InvalidOperationException(pin+" assigned to both "+previous+" and "+net);
                assigned[pin]=net;
            }
            string P(string reference,int index)=>reference+".P"+index.ToString("00");
            void Route(string from,string to,string net,int channel=-1)
            {
                Assign(from,net);Assign(to,net);
                routes.Add(new SparkLevel103Design.Route {id="W"+(routes.Count+1).ToString("000"),
                    from=from,to=to,net=net,channel=channel,via=""});
            }
            void Factory(string from,string to,string net)
            {
                Assign(from,net);Assign(to,net);
                factory.Add(new SparkLevel103Design.Branch {from=from,to=to});
            }
            const string ground="J1.P02", logic="U7.P03";
            Route("J1.P01","D1.P01","RAW_5V");
            Route("D1.P02","U7.P01","PROTECTED_5V");
            Route("U7.P02",ground,"GND");
            Route(logic,"U1.P01","3V3");
            Route("U1.P02",ground,"GND");
            Route(logic,"U9.P01","3V3");
            Route("U9.P02",ground,"GND");
            Route(logic,"U6.P01","3V3");
            Route("U6.P02",ground,"GND");
            Route("U6.P03","U1.P03","VREF_2V5");
            Route("U6.P03","U8.P03","VREF_2V5");
            Route(logic,"U8.P01","3V3");
            Route("U8.P02",ground,"GND");
            Route("U1.P08","U7.P04","ADC_SCLK");
            Route("U1.P09","U7.P05","ADC_DATA");
            Route("U1.P10","U7.P06","ADC_SELECT");
            Route(logic,"J2.P01","3V3");
            Route(ground,"J2.P02","GND");
            for(int channel=0;channel<4;channel++)
            {
                string amp="U"+(channel+2),sensor="S"+(channel+1),resistor="R"+(channel+1);
                Route(logic,P(amp,1),"3V3",channel);
                Route(P(amp,2),ground,"GND",channel);
                Route(P(amp,3),P(sensor,1),"CH"+channel+"_EXC",channel);
                Route(P(sensor,2),P(resistor,1),"CH"+channel+"_SENSOR",channel);
                Route(P(resistor,2),P(amp,4),"CH"+channel+"_INPUT",channel);
                Route(P(amp,5),P("U1",channel+4),"CH"+channel+"_ADC",channel);
                if(channel==1)Route(P(amp,6),P(sensor,3),"CH1_DIFFERENTIAL_MINUS",channel);
                else if(channel==0)Route(P(amp,6),"RV1.P01","RTD_TRIM_HI",channel);
                else if(channel==2)Route(P(amp,7),"R7.P01","OPTICAL_FEEDBACK",channel);
                else Route(P(amp,6),"C11.P01","HALL_FILTER",channel);
                Route(P(sensor,channel==1?4:3),ground,"GND",channel);
            }
            Route("U1.P07","U7.P07","ADC_READY");
            Route("U7.P08","U9.P03","ACQUISITION_HEARTBEAT");
            Route("U9.P04","R8.P01","SAFE_STATUS");
            Route("R8.P02","LED1.P01","LED_ANODE");
            Route("LED1.P02",ground,"GND");
            Route("U6.P03","C5.P01","VREF_2V5");
            Route("C5.P02",ground,"GND");
            Route("RV1.P03",ground,"GND");
            int[] viaRoutes={24,32,40,48,10,11,14,15,16,21,29,37};
            for(int i=0;i<viaRoutes.Length;i++)routes[viaRoutes[i]-1].via="V"+(i+1).ToString("00");
            Factory("U8.P04","W059.P01","REFERENCE_VALID_OUT");
            Factory("W059.P02","U9.P05","TRUSTED_REFERENCE_IN");
            // W059 is the only copper that joins the supervisor to the safety input.
            routes.Add(new SparkLevel103Design.Route {id="W059",from="W059.P01",to="W059.P02",
                net="TRUSTED_REFERENCE_LINK",via=""});

            void Passive(string reference,string high,string highNet,string low,string lowNet)
            {
                Factory(P(reference,1),high,highNet);
                Factory(P(reference,2),low,lowNet);
            }
            Passive("C1","U2.P04","CH0_INPUT",ground,"GND");
            Passive("C2",logic,"3V3",ground,"GND");
            Passive("C3","U3.P04","CH1_INPUT",ground,"GND");
            Passive("C4",logic,"3V3",ground,"GND");
            Passive("C6","U6.P03","VREF_2V5",ground,"GND");
            Passive("C7","U4.P04","CH2_INPUT",ground,"GND");
            Passive("C8",logic,"3V3",ground,"GND");
            Passive("C9","U5.P04","CH3_INPUT",ground,"GND");
            Passive("C10",logic,"3V3",ground,"GND");
            Factory("C11.P02",ground,"GND");
            Factory("R7.P02","U4.P05","CH2_ADC");
            Factory("RV1.P02","R5.P01","TRIM_WIPER");
            Factory("R5.P02",ground,"GND");
            Passive("R6","U7.P09","RESET_LOGIC","U9.P06","RESET_RETURN");
            Passive("R9",logic,"3V3","U3.P07","BRIDGE_COMMON_MODE");
            Factory("U3.P08",ground,"GND");
            Passive("R10","U9.P07","SAFETY_TEST",ground,"GND");
            Passive("R11",logic,"3V3","U7.P06","ADC_SELECT");
            Passive("R12","U7.P04","ADC_SCLK",ground,"GND");
            Passive("R13","U1.P11","ANALOG_RETURN",ground,"GND");
            Passive("R14","U9.P05","TRUSTED_REFERENCE_IN",ground,"GND");
            for(int channel=0;channel<4;channel++)
                if(channel!=1)Factory(P("S"+(channel+1),4),ground,"GND");

            design.contacts=sourceContacts.Select(row=>
            {
                var part=design.parts.Single(item=>item.reference==row[0]);
                var siblings=sourceContacts.Where(item=>item[0]==row[0]).ToArray();
                int ordinal=Array.FindIndex(siblings,item=>item[1]==row[1]);
                int half=(siblings.Length+1)/2;
                float z=part.zMm+(ordinal%half-(half-1)*.5f)*Mathf.Min(1.27f,part.depthMm/Mathf.Max(1,half-1));
                float x=part.xMm+(ordinal<half?-1:1)*(part.widthMm*.5f+.55f);
                string net=assigned.TryGetValue(row[1],out var assignedNet)?assignedNet:"NC_"+row[1].Replace('.','_');
                return new SparkLevel103Design.Contact {id=row[1],reference=row[0],net=net,
                    role=net.StartsWith("NC_")?"RESERVED_NO_CONNECT":net,positionMm=new Vector3(x,2.15f,z)};
            }).ToArray();
            design.routes=routes.ToArray();
            design.factory=factory.ToArray();
            design.provenance="Educational SMX-4 design authored for Project Spark Level 103 with user authorization. "+
                "Source art is Level 96. Generic IC models have explicitly assigned game pins. "+
                "No manufacturer pin compatibility is claimed. Unused package contacts are documented NC.";
            if(design.contacts.Length!=187 || design.parts.Length!=44 || routes.Count!=59 ||
                routes.Count(route=>!string.IsNullOrEmpty(route.via))!=12)
                throw new InvalidOperationException("SMX4 design counts differ from the source kit.");
            var contacts=design.contacts.ToDictionary(item=>item.id);
            foreach(var route in routes)
            {
                if(!contacts.ContainsKey(route.from)||!contacts.ContainsKey(route.to))
                    throw new InvalidOperationException("Unknown pad on "+route.id);
                if(route.id!="W059" && contacts[route.from].net!=contacts[route.to].net)
                    throw new InvalidOperationException("Incompatible copper domains on "+route.id);
            }
            if(create)AssetDatabase.CreateAsset(design,Folder+"/Data/SMX4_Educational_Design.asset");
            EditorUtility.SetDirty(design);
            File.WriteAllLines(Folder+"/Data/Resolved_Pad_Netlist.csv",new[]{"Contact,Footprint,Net,Role,DesignRevision"}
                .Concat(design.contacts.Select(pin=>$"{pin.id},{pin.reference},{pin.net},{pin.role},{design.designRevision}")));
            File.WriteAllLines(Folder+"/Data/Resolved_Player_Routes.csv",new[]{"TaskID,SourcePad,DestinationPad,NetID,Layer,ViaID,TraceWidth_mm,Clearance_mm,ApprovalStatus"}
                .Concat(routes.Select(route=>string.Join(",",route.id,route.from,route.to,route.net,
                    string.IsNullOrEmpty(route.via)?"F.Cu":"F.Cu>B.Cu",route.via,"0.25","0.20","USER_AUTHORIZED_EDUCATIONAL_DESIGN"))));
            return design;
        }
    }
}
#endif
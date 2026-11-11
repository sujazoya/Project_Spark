#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel34DrawingAssetCreator
    {
        private static readonly Color Red = new Color(.98f,.34f,.40f);
        private static readonly Color Blue = new Color(.24f,.69f,1f);
        private static readonly Color Cyan = new Color(.24f,.87f,.80f);
        private static readonly Color Amber = new Color(.99f,.71f,.28f);

        [MenuItem("Tools/Project Spark/Level 34/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel34Builder.EnsureFolder();
            string path=SparkLevel34Builder.Folder+"/Level_34_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_34";
            drawing.levelTitle="Priority Power - Automatic Load Shedding";
            drawing.pages=new[] {
                Page("01 / Feed the shared bus","5 V → 10 Ω FEEDER → THREE BRANCHES",
                    "W01 Battery_positive → FEED_IN. W02 FEED_OUT → Pump_A. W03 FEED_OUT → AUX_IN. W04 FEED_OUT → DEMAND_IN. FEED OUT is BUS. Each load draws through the same feeder, so increasing total current makes the bus voltage fall.",
                    new[] { Dot("supply",.09f,.55f,"BATTERY +",Red), Part("feed",SparkSchematicKind.Resistor,.29f,.55f,"FEEDER 10 Ω",Amber),
                        Dot("bus",.49f,.55f,"BUS",Cyan), Dot("pump",.81f,.81f,"PUMP A",Cyan),
                        Dot("aux",.81f,.55f,"AUX IN",Cyan),Dot("demand",.81f,.29f,"DEMAND IN",Cyan) },
                    Line(Red,.09f,.245f,.55f),Line(Cyan,.335f,.81f,.55f),
                    Wire(Cyan,P(.49f,.55f),P(.49f,.81f),P(.81f,.81f)),
                    Wire(Cyan,P(.49f,.55f),P(.49f,.29f),P(.81f,.29f))),
                Page("02 / Wire the loads and returns","PUMP DIRECT • FAN AND DEMAND SWITCHED",
                    "W05 AUX_OUT → Fan_A. W06 DEMAND_OUT → R_DEMAND_IN. W07 Pump_B → Battery_negative. W08 Fan_B → Battery_negative. W09 R_DEMAND_OUT → Battery_negative. The pump stays on BUS while the cutoff opens only the fan branch.",
                    new[] { Dot("bus",.11f,.51f,"BUS",Cyan), Part("pump",SparkSchematicKind.Motor,.65f,.80f,"PUMP 47 Ω",Cyan),
                        Part("aux",SparkSchematicKind.Switch,.35f,.51f,"AUX CUTOFF",Amber), Part("fan",SparkSchematicKind.Motor,.65f,.51f,"FAN 47 Ω",Amber),
                        Part("demand",SparkSchematicKind.Switch,.35f,.23f,"DEMAND",Amber),
                        Part("bank",SparkSchematicKind.Resistor,.65f,.23f,"BANK 47 Ω",Amber), Dot("return",.92f,.51f,"BATTERY −",Blue) },
                    Wire(Cyan,P(.11f,.51f),P(.11f,.80f),P(.617f,.80f)),Line(Cyan,.11f,.305f,.51f),
                    Wire(Cyan,P(.11f,.51f),P(.11f,.23f),P(.305f,.23f)),Line(Amber,.395f,.617f,.51f),Line(Amber,.395f,.605f,.23f),
                    Wire(Blue,P(.683f,.80f),P(.92f,.80f),P(.92f,.51f)),Line(Blue,.683f,.92f,.51f),
                    Wire(Blue,P(.695f,.23f),P(.92f,.23f),P(.92f,.51f))),
                Page("03 / Observe sag and recovery","SOURCE STAYS NEAR 5 V • BUS VOLTAGE CHANGES",
                    "The source still reads near 5 V. Nominal BUS is 4.12 V with one branch, 3.51 V with two, and 3.05 V with all three. At less than 3.25 V the controller records the sag and opens the fan contact. Pump plus demand then return to about 3.51 V.",
                    new[] { Dot("one",.14f,.80f,"PUMP 4.12 V",Cyan),Dot("two",.36f,.60f,"ADD FAN 3.51 V",Cyan),
                        Dot("three",.59f,.33f,"SAG 3.05 V",Red),Dot("shed",.84f,.60f,"SHED 3.51 V",Cyan) },
                    Wire(Cyan,P(.14f,.80f),P(.36f,.60f)),Wire(Red,P(.36f,.60f),P(.59f,.33f)),Wire(Amber,P(.59f,.33f),P(.84f,.60f))),
                Page("04 / Restore deliberately","DEMAND OFF + BUS ≥ 3.90 V → RESTORE AUX",
                    "1/6 Pump alone. 2/6 CONNECT AUX and verify both loads. 3/6 APPLY DEMAND and record the sag. 4/6 Keep demand on while the pump voltage recovers and fan current drops near zero. 5/6 REMOVE DEMAND and verify the fan stays off. 6/6 RESTORE AUX and confirm both loads run again.",
                    new[] { Dot("shed",.17f,.59f,"AUX SHED",Red),Dot("remove",.40f,.59f,"DEMAND OFF",Amber),
                        Dot("recovered",.63f,.59f,"BUS ≥ 3.90 V",Cyan),Dot("restore",.85f,.59f,"RESTORE AUX",Cyan) },
                    Line(Amber,.17f,.40f,.59f),Line(Cyan,.40f,.85f,.59f))
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,SparkSchematicPart[] parts,params SparkSchematicWire[] wires) =>
            new SparkLevelDrawingPage { title=title,caption=caption,explanation=explanation,parts=parts,wires=wires };
        private static SparkSchematicPart Dot(string id,float x,float y,string label,Color color) =>
            Part(id,SparkSchematicKind.Generic,x,y,label,color);
        private static SparkSchematicPart Part(string id,SparkSchematicKind kind,float x,float y,string label,Color color) =>
            new SparkSchematicPart { id=id,kind=kind,position=P(x,y),label=label,color=color };
        private static SparkSchematicWire Line(Color color,float from,float to,float y) => Wire(color,P(from,y),P(to,y));
        private static SparkSchematicWire Wire(Color color,params Vector2[] points) => new SparkSchematicWire { color=color,points=points };
        private static Vector2 P(float x,float y) => new Vector2(x,y);
    }
}
#endif

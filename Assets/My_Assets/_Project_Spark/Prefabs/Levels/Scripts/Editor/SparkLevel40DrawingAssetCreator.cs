#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel40DrawingAssetCreator
    {
        private static readonly Color Power = new Color(.98f,.35f,.42f);
        private static readonly Color Loop = new Color(.99f,.73f,.31f);
        private static readonly Color Receiver = new Color(.24f,.87f,.80f);
        private static readonly Color Return = new Color(.28f,.69f,1f);

        [MenuItem("Tools/Project Spark/Level 40/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel40Builder.EnsureFolder();
            string path=SparkLevel40Builder.Folder+"/Level_40_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_40";
            drawing.levelTitle="The Long Run";
            drawing.pages=new[]
            {
                Page("01 / Wire the current loop","4 mA IS LIVE ZERO",
                    "W01 LOOP_POS → TX_IN. W02 TX_OUT → CABLE_IN. W03 CABLE_OUT → LINE_IN. W04 LINE_OUT → RX_IN. W05 RX_OUT → LOOP_RETURN. The empty tank sends 4 mA and the 250 Ω receiver reads 1.00 V.",
                    "50 Ω CABLE","LINE CLOSED","RX 1.00 V",false),
                Page("02 / Extend the cable at midscale","50% TANK  •  12 mA  •  3.00 V AT RX",
                    "Select 50% TANK, then fit the 350 Ω cable. Receiver current and voltage stay at 12 mA and 3.00 V. The transmitter has about 4.80 V across it after the cable change.",
                    "350 Ω CABLE","LINE CLOSED","RX 3.00 V",false),
                Page("03 / Find the voltage limit","THE TRANSMITTER NEEDS 2.0 V OF HEADROOM",
                    "At 100% with the long cable, 600 Ω of cable and receiver resistance leaves no headroom at 20 mA. With 10 V available to the load, current is about 16.67 mA. The receiver reads 4.17 V and decodes about 79%.",
                    "350 Ω CABLE","LINE CLOSED","RX 4.17 V",false),
                Page("04 / Restore full scale and test the break","20 mA FULL SCALE  •  0 mA OPEN LINE",
                    "Fit the 50 Ω cable to recover 20 mA and 5.00 V at RX. Open the line and confirm that 0 mA reports LINE OPEN. Close it again and verify the 100% reading returns.",
                    "50 Ω CABLE","LINE OPEN","RX 0.00 V",true)
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,
            string cable,string line,string receiver,bool open)
        {
            return new SparkLevelDrawingPage
            {
                title=title, caption=caption, explanation=explanation,
                parts=new[]
                {
                    Part("positive",SparkSchematicKind.Generic,.10f,.72f,"12 V +",Power),
                    Part("transmitter",SparkSchematicKind.Generic,.32f,.72f,"4–20 mA TX",Loop),
                    Part("cable",SparkSchematicKind.Resistor,.58f,.72f,cable,Loop),
                    Part("line",SparkSchematicKind.Generic,.81f,.52f,line,open ? Power : Loop),
                    Part("receiver",SparkSchematicKind.Resistor,.58f,.32f,"250 Ω  "+receiver,Receiver),
                    Part("return",SparkSchematicKind.Generic,.10f,.32f,"LOOP RETURN",Return)
                },
                wires=new[]
                {
                    Wire(Power,P(.10f,.72f),P(.275f,.72f)),
                    Wire(Loop,P(.365f,.72f),P(.535f,.72f)),
                    Wire(Loop,P(.625f,.72f),P(.765f,.72f),P(.765f,.52f)),
                    Wire(Receiver,P(.855f,.52f),P(.855f,.32f),P(.625f,.32f)),
                    Wire(Return,P(.535f,.32f),P(.10f,.32f))
                }
            };
        }

        private static SparkSchematicPart Part(string id,SparkSchematicKind kind,float x,float y,string label,Color color) =>
            new SparkSchematicPart { id=id,kind=kind,position=P(x,y),label=label,color=color };

        private static SparkSchematicWire Wire(Color color,params Vector2[] points) =>
            new SparkSchematicWire { color=color,points=points };

        private static Vector2 P(float x,float y) => new Vector2(x,y);
    }
}
#endif

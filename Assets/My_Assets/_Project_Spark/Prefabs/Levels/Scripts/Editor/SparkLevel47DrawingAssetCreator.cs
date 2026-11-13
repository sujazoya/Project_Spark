#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel47DrawingAssetCreator
    {
        private static readonly Color Ladder = new Color(.24f,.87f,.80f);
        private static readonly Color Digital = new Color(.99f,.73f,.31f);
        private static readonly Color Return = new Color(.28f,.69f,1f);
        private static readonly Color Fault = new Color(.98f,.35f,.47f);

        [MenuItem("Tools/Project Spark/Level 47/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel47Builder.EnsureFolder();
            string path=SparkLevel47Builder.Folder+"/Level_47_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_47";
            drawing.levelTitle="Hold the Tension";
            drawing.pages=new[]
            {
                Page("01 / Wire the winding loop","15 WIRES  •  TWO DRIVES  •  ANALOG SENSOR",
                    "Connect both drives and encoders. Connect W13 and W14 to sensor power. Connect W15 from Sensor Signal to Tension In last.",
                    "FEED ENCODER", "TAKE-UP ENCODER", false),
                Page("02 / Calibrate the dancer","MOTORS STOPPED  •  NEUTRAL 2.50 V",
                    "Calibrate the centered dancer. Start automatic winding and observe the sensor stay inside 2.30–2.70 V as material transfers.",
                    "DANCER", "2.50 V", false),
                Page("03 / Compensate for roll growth","SURFACE SPEED = ANGULAR SPEED × RADIUS",
                    "The feed roll shrinks while the take-up roll grows. Watch their RPM ratio change. Apply the feed brake and watch the take-up drive ease back as the dancer moves toward high tension.",
                    "FEED SHRINKS", "TAKE-UP GROWS", false),
                Page("04 / Recover from a sensor fault","W15 OPEN  •  BOTH DRIVES STOP",
                    "Remove W15 while winding. Restore it, wait for safe stationary feedback, explicitly reset the fault and start AUTO again.",
                    "W15 OPEN", "RESET / START", true)
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string a,string b,bool crossed)
        {
            return new SparkLevelDrawingPage
            {
                title=title, caption=caption, explanation=explanation,
                parts=new[]
                {
                    Part("source",SparkSchematicKind.Generic,.12f,.80f,"5 V + RETURN",Return),
                    Part("encoder",SparkSchematicKind.Generic,.14f,.48f,"DANCER SENSOR",Ladder),
                    Part("decoder",SparkSchematicKind.Generic,.82f,.48f,"TENSION CONTROL",Digital),
                    Part("phaseA",SparkSchematicKind.Generic,.47f,.64f,a,crossed ? Fault : Ladder),
                    Part("phaseB",SparkSchematicKind.Generic,.47f,.30f,b,crossed ? Fault : Return),
                    Part("index",SparkSchematicKind.Generic,.47f,.48f,"DRIVER / WEB",Digital)
                },
                wires=new[]
                {
                    Wire(Return,P(.12f,.80f),P(.82f,.80f),P(.82f,.57f)),
                    Wire(crossed ? Fault : Ladder,P(.18f,.60f),P(.33f,.60f),P(.67f,crossed ? .34f : .60f),P(.78f,crossed ? .34f : .60f)),
                    Wire(crossed ? Fault : Return,P(.18f,.34f),P(.33f,.34f),P(.67f,crossed ? .60f : .34f),P(.78f,crossed ? .60f : .34f)),
                    Wire(Digital,P(.18f,.48f),P(.78f,.48f))
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

#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel46DrawingAssetCreator
    {
        private static readonly Color Ladder = new Color(.24f,.87f,.80f);
        private static readonly Color Digital = new Color(.99f,.73f,.31f);
        private static readonly Color Return = new Color(.28f,.69f,1f);
        private static readonly Color Fault = new Color(.98f,.35f,.46f);

        [MenuItem("Tools/Project Spark/Level 46/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel46Builder.EnsureFolder();
            string path=SparkLevel46Builder.Folder+"/Level_46_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_46";
            drawing.levelTitle="Keep in Step";
            drawing.pages=new[]
            {
                Page("01 / Build the transport loop","12 WIRES  •  TWO MOTORS  •  TWO ENCODERS",
                    "Wire supply and return, both PWM signals, each pair of motor leads and the four feedback leads. Add follower A and B last for the direction test.",
                    "MASTER A/B", "FOLLOWER A/B", false),
                Page("02 / Match the master","30 RPM  •  60 RPM  •  80 COUNTS PER REVOLUTION",
                    "Jog forward and compare signed counts. In SYNC, the master tracks the preset and the follower tracks measured master speed. Both motors must hold within the target band and within 2 RPM of each other.",
                    "MASTER SPEED", "FOLLOWER SPEED", false),
                Page("03 / Recover under load","FOLLOWER BRAKE  •  WEB SLACK  •  RECOVERY",
                    "Apply the test brake at steady 60 RPM. The follower slows. The governor raises follower duty and the web returns to its safe band when both rollers match again.",
                    "FOLLOWER LOAD", "DUTY RISES", false),
                Page("04 / Repair reversed feedback","FORWARD SHAFT  •  REVERSE COUNT  •  SAFE STOP",
                    "Cross follower A and B at the controller and start the direction test. Three backward transitions with forward shaft motion stop both drives. Restore the wires, reset and synchronize at 60 RPM again.",
                    "F A → B", "F B → A", true)
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
                    Part("encoder",SparkSchematicKind.Generic,.14f,.48f,"ENCODER",Ladder),
                    Part("decoder",SparkSchematicKind.Generic,.82f,.48f,"GOVERNOR",Digital),
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

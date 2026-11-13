#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel45DrawingAssetCreator
    {
        private static readonly Color Ladder = new Color(.24f,.87f,.80f);
        private static readonly Color Digital = new Color(.99f,.73f,.31f);
        private static readonly Color Return = new Color(.28f,.69f,1f);
        private static readonly Color Fault = new Color(.98f,.35f,.45f);

        [MenuItem("Tools/Project Spark/Level 45/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel45Builder.EnsureFolder();
            string path=SparkLevel45Builder.Folder+"/Level_45_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_45";
            drawing.levelTitle="Hold the Speed";
            drawing.pages=new[]
            {
                Page("01 / Build the speed loop","5 V  •  ENCODER A/B  •  PWM  •  MOTOR",
                    "Connect controller and driver power, encoder A/B, controller PWM to driver PWM, and both motor leads. Add encoder B (W06) last so it can be removed with Undo Last.",
                    "ENCODER A/B", "PWM COMMAND", false),
                Page("02 / Measure speed","80 COUNTS PER REVOLUTION  •  0.5 SECOND SAMPLE",
                    "Start in open loop. Count valid quadrature transitions to measure RPM. Add load and compare speed at the same duty. Release the brake, then select closed loop at 30 and 60 RPM.",
                    "MEASURED RPM", "SPEED ERROR", false),
                Page("03 / Reject a load change","60 RPM  •  TEST LOAD",
                    "The brake adds torque opposing the motor. Speed falls first. The governor increases duty and brings the speed back to the target band.",
                    "LOAD TORQUE", "DUTY RISES", false),
                Page("04 / Recover safely","ENCODER B LOST  •  DRIVE DISABLED",
                    "Remove W06 while running. The driver opens and the rotor coasts down. Restore W06, explicitly reset, and start again in closed loop at 60 RPM.",
                    "FEEDBACK LOST", "RESET / RESTART", true)
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
                    Part("index",SparkSchematicKind.Generic,.47f,.48f,"MOTOR / BRAKE",Digital)
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

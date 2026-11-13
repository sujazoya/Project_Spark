#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel43DrawingAssetCreator
    {
        private static readonly Color Ladder = new Color(.24f,.87f,.80f);
        private static readonly Color Digital = new Color(.99f,.73f,.31f);
        private static readonly Color Return = new Color(.28f,.69f,1f);
        private static readonly Color Fault = new Color(.98f,.35f,.43f);

        [MenuItem("Tools/Project Spark/Level 43/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel43Builder.EnsureFolder();
            string path=SparkLevel43Builder.Folder+"/Level_43_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_43";
            drawing.levelTitle="Hold the Line";
            drawing.pages=new[]
            {
                Page("01 / Close the loop","5 V SUPPLY  •  REVERSIBLE H-BRIDGE",
                    "W01 SUPPLY_PLUS to DRIVER_PLUS. W02 SUPPLY_RET to DRIVER_RET. W03 COMMAND_OUT to COMMAND_IN. W04 POSITION_OUT to POSITION_IN. W05 OUT_A to MOTOR_A. W06 OUT_B to MOTOR_B. Add the motor wires last.",
                    "MANUAL", "POSITION", false),
                Page("02 / Compare command and position","LEFT  •  STOP  •  RIGHT",
                    "Jog right to verify that feedback rises. AUTO A commands 1.25 V or 25% travel. AUTO B commands 3.75 V or 75% travel. A command above feedback drives right. A command below feedback drives left. Within 0.15 V the driver stops.",
                    "3.75 V", "2.50 V", false),
                Page("03 / Diagnose the reversed motor","WRONG DIRECTION  •  SAFE STOP",
                    "Remove W05 and W06. Connect OUT A to MOTOR B and OUT B to MOTOR A, then press TEST FAULT. The carriage moves away and stops. Restore both leads and select AUTO B.",
                    "3.75 V", "FALLING", true),
                Page("04 / Reject a disturbance","AUTO B  •  CORRECT FROM BOTH SIDES",
                    "Nudge left. The controller drives right until the carriage settles. Then nudge right. The controller drives left until it settles again. The feedback loop holds the same position command.",
                    "3.75 V", "3.75 V", false)
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string command,string feedback,bool crossed)
        {
            return new SparkLevelDrawingPage
            {
                title=title, caption=caption, explanation=explanation,
                parts=new[]
                {
                    Part("source",SparkSchematicKind.Generic,.13f,.80f,"5 V / RET",Return),
                    Part("driver",SparkSchematicKind.Generic,.47f,.74f,"H-BRIDGE",Digital),
                    Part("motor",SparkSchematicKind.Generic,.84f,.74f,"MOTOR",crossed ? Fault : Digital),
                    Part("command",SparkSchematicKind.Generic,.13f,.37f,"COMMAND "+command,Digital),
                    Part("control",SparkSchematicKind.Generic,.47f,.37f,"COMPARE / STOP BAND",Ladder),
                    Part("sensor",SparkSchematicKind.Generic,.84f,.37f,"FEEDBACK "+feedback,Ladder)
                },
                wires=new[]
                {
                    Wire(Return,P(.13f,.80f),P(.32f,.80f),P(.47f,.74f)),
                    Wire(crossed ? Fault : Digital,P(.51f,.78f),P(.68f,.78f),P(.80f,crossed ? .70f : .78f)),
                    Wire(crossed ? Fault : Digital,P(.51f,.70f),P(.68f,.70f),P(.80f,crossed ? .78f : .70f)),
                    Wire(Digital,P(.15f,.37f),P(.43f,.37f)),
                    Wire(Ladder,P(.82f,.37f),P(.51f,.37f)),
                    Wire(Ladder,P(.47f,.42f),P(.47f,.69f)),
                    Wire(Return,P(.84f,.69f),P(.84f,.42f))
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

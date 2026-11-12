#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel35DrawingAssetCreator
    {
        private static readonly Color Positive = new Color(.98f,.34f,.40f);
        private static readonly Color Return = new Color(.24f,.69f,1f);
        private static readonly Color Left = new Color(.24f,.87f,.80f);
        private static readonly Color Right = new Color(.99f,.71f,.28f);

        [MenuItem("Tools/Project Spark/Level 35/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel35Builder.EnsureFolder();
            string path=SparkLevel35Builder.Folder+"/Level_35_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_35";
            drawing.levelTitle="Bridge Balance - Find the Hidden Fault";
            drawing.pages=new[] {
                Page("01 / Wire and capture the baseline","TWO PARALLEL DIVIDERS • 5 V • FOUR 1 kΩ RESISTORS",
                    "W01 Battery_positive → LU_IN. W02 Battery_positive → RU_IN. W03 LU_OUT → LL_IN. W04 RU_OUT → RL_IN. W05 LL_OUT → Battery_negative. W06 RL_OUT → Battery_negative. Each midpoint should read 2.50 V relative to Battery −.",
                    false,false,"LEFT 2.50 V","RIGHT 2.50 V"),
                Page("02 / Locate a single fault","DIFFERENCE = LEFT − RIGHT",
                    "A 2.2 kΩ lower resistor raises its midpoint to 3.44 V. A positive difference of +0.94 V with RIGHT at 2.50 V identifies the left lower arm. A negative difference of −0.94 V with LEFT at 2.50 V identifies the right lower arm. The drawing shows the left fault.",
                    true,false,"LEFT 3.44 V","RIGHT 2.50 V"),
                Page("03 / Detect false balance","BOTH MIDPOINTS ARE HIGH • DIFF IS STILL 0.00 V",
                    "When both lower arms are 2.2 kΩ, their midpoints match at 3.44 V. The differential meter reads 0.00 V. Compare each midpoint with the 2.50 V baseline to find the two faults.",
                    true,true,"LEFT 3.44 V","RIGHT 3.44 V"),
                Page("04 / Repair and verify","BOTH MIDPOINTS RETURN TO 2.50 V",
                    "Fit a 1 kΩ cartridge to each lower arm. Replacing one first makes a signed difference appear again. Once both are replaced, check LEFT ≈ 2.50 V, RIGHT ≈ 2.50 V, and DIFF ≈ 0.00 V. Press VERIFY REPAIR to finish.",
                    false,false,"LEFT 2.50 V","RIGHT 2.50 V")
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,
            bool leftFault,bool rightFault,string leftReading,string rightReading) =>
            new SparkLevelDrawingPage {
                title=title,caption=caption,explanation=explanation,
                parts=new[] {
                    Part("supply",SparkSchematicKind.Generic,.10f,.55f,"BATTERY +",Positive),
                    Part("lu",SparkSchematicKind.Resistor,.33f,.77f,"LU 1 kΩ",Left),
                    Part("lm",SparkSchematicKind.Generic,.51f,.77f,leftReading,Left),
                    Part("ll",SparkSchematicKind.Resistor,.70f,.77f,leftFault ? "LL 2.2 kΩ" : "LL 1 kΩ",Left),
                    Part("ru",SparkSchematicKind.Resistor,.33f,.34f,"RU 1 kΩ",Right),
                    Part("rm",SparkSchematicKind.Generic,.51f,.34f,rightReading,Right),
                    Part("rl",SparkSchematicKind.Resistor,.70f,.34f,rightFault ? "RL 2.2 kΩ" : "RL 1 kΩ",Right),
                    Part("return",SparkSchematicKind.Generic,.90f,.55f,"BATTERY −",Return)
                },
                wires=new[] {
                    Wire(Positive,P(.10f,.55f),P(.10f,.77f),P(.285f,.77f)),
                    Wire(Positive,P(.10f,.55f),P(.10f,.34f),P(.285f,.34f)),
                    Wire(Left,P(.375f,.77f),P(.655f,.77f)),
                    Wire(Right,P(.375f,.34f),P(.655f,.34f)),
                    Wire(Return,P(.745f,.77f),P(.90f,.77f),P(.90f,.55f)),
                    Wire(Return,P(.745f,.34f),P(.90f,.34f),P(.90f,.55f))
                }
            };

        private static SparkSchematicPart Part(string id,SparkSchematicKind kind,float x,float y,string label,Color color) =>
            new SparkSchematicPart { id=id,kind=kind,position=P(x,y),label=label,color=color };
        private static SparkSchematicWire Wire(Color color,params Vector2[] points) => new SparkSchematicWire { color=color,points=points };
        private static Vector2 P(float x,float y) => new Vector2(x,y);
    }
}
#endif

#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel41DrawingAssetCreator
    {
        private static readonly Color Ladder = new Color(.24f,.87f,.80f);
        private static readonly Color Digital = new Color(.99f,.73f,.31f);
        private static readonly Color Return = new Color(.28f,.69f,1f);
        private static readonly Color Fault = new Color(.98f,.35f,.42f);

        [MenuItem("Tools/Project Spark/Level 41/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel41Builder.EnsureFolder();
            string path=SparkLevel41Builder.Folder+"/Level_41_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_41";
            drawing.levelTitle="Build the Analog Output";
            drawing.pages=new[]
            {
                Page("01 / Wire the R–2R ladder","R = 10 kΩ  •  2R = 20 kΩ",
                    "W01 BIT2 → B2_R_IN. W02 BIT1 → B1_R_IN. W03 BIT0 → B0_R_IN. W04 B2_R_OUT → LINK21_IN. W05 LINK21_OUT → B1_R_OUT. W06 B1_R_OUT → LINK10_IN. W07 LINK10_OUT → B0_R_OUT. W08 B0_R_OUT → TERM_R_IN. W09 TERM_OUT → DAC_RETURN.",
                    "000", "0.000 V", false),
                Page("02 / Compare the bit weights","001 = 0.625 V  •  010 = 1.250 V  •  100 = 2.500 V",
                    "Apply 001, 010, and 100 to measure the bits individually. B2 contributes four steps, B1 contributes two, and B0 contributes one. Use those weights to find the code for 3.125 V.",
                    "100", "2.500 V", false),
                Page("03 / Find the open termination","111  •  HEALTHY 4.375 V  •  OPEN LINK ≈ 5.000 V",
                    "First apply 111 with the link closed. Then open the link while keeping 111 applied. With the path to return open and all three bits high, the output rises toward the reference. Select OPEN TERMINATION.",
                    "111", "≈ 5.000 V", true),
                Page("04 / Restore and sweep","EIGHT LEVELS  •  SEVEN STEPS OF 0.625 V",
                    "Restore the link and run the sweep from 000 through 111. The readings rise from 0.000 V to 4.375 V in seven equal steps. Full scale is one step below the 5 V reference.",
                    "111", "4.375 V", false)
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string bits,string output,bool open)
        {
            return new SparkLevelDrawingPage
            {
                title=title,caption=caption,explanation=explanation,
                parts=new[]
                {
                    Part("out",SparkSchematicKind.Generic,.10f,.78f,"DAC OUT "+output,Ladder),
                    Part("link21",SparkSchematicKind.Resistor,.34f,.78f,"10 kΩ",Ladder),
                    Part("link10",SparkSchematicKind.Resistor,.63f,.78f,"10 kΩ",Ladder),
                    Part("r2",SparkSchematicKind.Resistor,.19f,.51f,"20 kΩ",Digital),
                    Part("r1",SparkSchematicKind.Resistor,.49f,.51f,"20 kΩ",Digital),
                    Part("r0",SparkSchematicKind.Resistor,.77f,.51f,"20 kΩ",Digital),
                    Part("b2",SparkSchematicKind.Generic,.19f,.24f,"B2 = "+bits[0],Digital),
                    Part("b1",SparkSchematicKind.Generic,.49f,.24f,"B1 = "+bits[1],Digital),
                    Part("b0",SparkSchematicKind.Generic,.77f,.24f,"B0 = "+bits[2],Digital),
                    Part("term",SparkSchematicKind.Resistor,.90f,.60f,"20 kΩ",Ladder),
                    Part("return",SparkSchematicKind.Generic,.90f,.30f,open ? "LINK OPEN" : "RETURN",open ? Fault : Return)
                },
                wires=new[]
                {
                    Wire(Ladder,P(.10f,.78f),P(.295f,.78f)),
                    Wire(Ladder,P(.385f,.78f),P(.585f,.78f)),
                    Wire(Ladder,P(.675f,.78f),P(.90f,.78f),P(.90f,.65f)),
                    Wire(Ladder,P(.19f,.78f),P(.19f,.56f)),
                    Wire(Ladder,P(.49f,.78f),P(.49f,.56f)),
                    Wire(Ladder,P(.77f,.78f),P(.77f,.56f)),
                    Wire(Digital,P(.19f,.46f),P(.19f,.24f)),
                    Wire(Digital,P(.49f,.46f),P(.49f,.24f)),
                    Wire(Digital,P(.77f,.46f),P(.77f,.24f)),
                    Wire(open ? Fault : Return,P(.90f,.55f),P(.90f,open ? .46f : .30f))
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

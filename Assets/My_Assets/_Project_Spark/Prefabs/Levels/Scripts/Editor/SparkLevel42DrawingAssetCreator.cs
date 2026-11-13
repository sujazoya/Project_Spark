#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel42DrawingAssetCreator
    {
        private static readonly Color Ladder = new Color(.24f,.87f,.80f);
        private static readonly Color Digital = new Color(.99f,.73f,.31f);
        private static readonly Color Return = new Color(.28f,.69f,1f);
        private static readonly Color Fault = new Color(.98f,.35f,.42f);

        [MenuItem("Tools/Project Spark/Level 42/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel42Builder.EnsureFolder();
            string path=SparkLevel42Builder.Folder+"/Level_42_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_42";
            drawing.levelTitle="Read the Analog Signal";
            drawing.pages=new[]
            {
                Page("01 / Wire the ADC","3-BIT R–2R DAC  •  5 V REFERENCE",
                    "W01–W03: BIT2, BIT1 and BIT0 to their 20 kΩ inputs. W04–W07: B2 OUT to LINK 21 IN, LINK 21 OUT to B1 OUT, B1 OUT to LINK 10 IN, LINK 10 OUT to B0 OUT. W08–W09: B0 OUT to TERM IN, TERM OUT to REF RETURN. W10: SENSOR OUT to SENSOR +. W11: DAC TAP (B2 OUT) to DAC −.",
                    "000", "0.000 V", false),
                Page("02 / Try each bit from B2 to B0","TEST  •  COMPARE  •  KEEP OR CLEAR",
                    "The ladder converts each trial code to a voltage. Keep the trial bit when the sensor is at or above the DAC. Clear it when the sensor is below the DAC. Compare all three bits and submit the final code before loading the next sensor.",
                    "100", "2.500 V", false),
                Page("03 / Diagnose swapped inputs","TEST INPUT 3.200 V  •  TRIAL 100 = 2.500 V",
                    "Undo the two comparator leads, cross them and test 100. The comparator reports BELOW. Reconnect SENSOR OUT to SENSOR + and DAC TAP to DAC −. Now 100 reports ABOVE and 110 reports BELOW.",
                    "100", "2.500 V", true),
                Page("04 / Verify all eight bands","000–111  •  0.625 V PER STEP",
                    "Run the sweep with the correct input wiring. It tests the midpoint of each band. Code 000 covers 0–0.625 V, 001 starts at 0.625 V and 111 starts at 4.375 V. The lower edge belongs to the new code.",
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
                    Part("out",SparkSchematicKind.Generic,.10f,.73f,"DAC TAP "+output,Ladder),
                    Part("sensor",SparkSchematicKind.Generic,.20f,.95f,"SENSOR OUT",Digital),
                    Part("cmp",SparkSchematicKind.Generic,.51f,.93f,open ? "INPUTS CROSSED" : "COMPARATOR + / −",open ? Fault : Return),
                    Part("link21",SparkSchematicKind.Resistor,.34f,.78f,"10 kΩ",Ladder),
                    Part("link10",SparkSchematicKind.Resistor,.63f,.78f,"10 kΩ",Ladder),
                    Part("r2",SparkSchematicKind.Resistor,.19f,.51f,"20 kΩ",Digital),
                    Part("r1",SparkSchematicKind.Resistor,.49f,.51f,"20 kΩ",Digital),
                    Part("r0",SparkSchematicKind.Resistor,.77f,.51f,"20 kΩ",Digital),
                    Part("b2",SparkSchematicKind.Generic,.19f,.24f,"B2 = "+bits[0],Digital),
                    Part("b1",SparkSchematicKind.Generic,.49f,.24f,"B1 = "+bits[1],Digital),
                    Part("b0",SparkSchematicKind.Generic,.77f,.24f,"B0 = "+bits[2],Digital),
                    Part("term",SparkSchematicKind.Resistor,.90f,.60f,"20 kΩ",Ladder),
                    Part("return",SparkSchematicKind.Generic,.90f,.30f,"RETURN",Return)
                },
                wires=new[]
                {
                    Wire(Ladder,P(.10f,.73f),P(.12f,.78f),P(.295f,.78f)),
                    Wire(open ? Fault : Digital,P(.20f,.95f),P(.39f,open ? .88f : .95f),P(.51f,open ? .88f : .95f)),
                    Wire(open ? Fault : Ladder,P(.12f,.78f),P(.12f,.85f),P(.38f,.85f),P(.46f,open ? .95f : .88f),P(.51f,open ? .95f : .88f)),
                    Wire(Ladder,P(.385f,.78f),P(.585f,.78f)),
                    Wire(Ladder,P(.675f,.78f),P(.90f,.78f),P(.90f,.65f)),
                    Wire(Ladder,P(.19f,.78f),P(.19f,.56f)),
                    Wire(Ladder,P(.49f,.78f),P(.49f,.56f)),
                    Wire(Ladder,P(.77f,.78f),P(.77f,.56f)),
                    Wire(Digital,P(.19f,.46f),P(.19f,.24f)),
                    Wire(Digital,P(.49f,.46f),P(.49f,.24f)),
                    Wire(Digital,P(.77f,.46f),P(.77f,.24f)),
                    Wire(Return,P(.90f,.55f),P(.90f,.30f))
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

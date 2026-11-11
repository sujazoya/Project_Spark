#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel32DrawingAssetCreator
    {
        private static readonly Color Supply = new Color(.97f,.29f,.35f);
        private static readonly Color Return = new Color(.23f,.66f,1f);
        private static readonly Color Sense = new Color(.20f,.90f,.78f);
        private static readonly Color Amber = new Color(.98f,.72f,.32f);

        [MenuItem("Tools/Project Spark/Level 32/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel32Builder.EnsureFolder();
            string path=SparkLevel32Builder.Folder+"/Level_32_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_32";
            drawing.levelTitle="Optical Quality Sorter";
            drawing.pages=new[] {
                Page("01 / Sensor and motor branches","5 V SUPPLY • MAIN • TWO PARALLEL BRANCHES",
                    "MAIN powers the conveyor motor and the light-sensing branch. The sensor branch puts the LDR above the 10 kΩ resistor. The SENSE reading is taken at the junction between them, relative to supply negative."),
                Page("02 / Six bench connections","USE THE TERMINAL LABELS ON THE WORKBENCH",
                    "W01 Battery_positive → MAIN_IN. W02 MAIN_OUT → LDR_A. W03 LDR_B → R_DIV_IN. W04 R_DIV_OUT → Battery_negative. W05 MAIN_OUT → Motor_A. W06 Motor_B → Battery_negative."),
                Page("03 / Calibrate the divider","DARK ≈ 0.83 V • BORDERLINE ≈ 2.50 V • BRIGHT ≈ 4.32 V",
                    "Read each reference tile. More reflected light lowers LDR resistance and raises SENSE. The sorter uses DARK at or below 1.5 V, BRIGHT at or above 3.5 V, and REVIEW between them."),
                Page("04 / Reject glare","REQUIRE 0.24 S IN ONE BAND INSIDE THE INSPECTION WINDOW",
                    "The brief bright pulse lasts 0.12 s and cannot qualify. The sensor must read the same band for 0.24 s before the gate locks a route. With no stable result, the gate sends the part to REVIEW. Finish with all six mixed parts in the correct bins.")
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation) =>
            new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {
                    Part("source",SparkSchematicKind.Battery,.10f,.49f,"5 V",Supply),
                    Part("main",SparkSchematicKind.Switch,.32f,.75f,"MAIN",Supply),
                    Part("ldr",SparkSchematicKind.Resistor,.57f,.75f,"LDR",Sense),
                    Part("divider",SparkSchematicKind.Resistor,.82f,.75f,"10 kΩ",Amber),
                    Part("motor",SparkSchematicKind.Generic,.63f,.43f,"MOTOR",Amber),
                    Part("ground",SparkSchematicKind.Ground,.72f,.18f,"GND",Return)
                },
                wires=new[] {
                    Wire(Supply,P(.10f,.62f),P(.10f,.75f),P(.275f,.75f)),
                    Wire(Supply,P(.365f,.75f),P(.525f,.75f)),
                    Wire(Sense,P(.615f,.75f),P(.775f,.75f)),
                    Wire(Return,P(.865f,.75f),P(.92f,.75f),P(.92f,.18f),P(.10f,.18f),P(.10f,.36f)),
                    Wire(Supply,P(.43f,.75f),P(.43f,.43f),P(.585f,.43f)),
                    Wire(Return,P(.675f,.43f),P(.92f,.43f))
                }
            };

        private static Vector2 P(float x,float y) => new Vector2(x,y);
        private static SparkSchematicPart Part(string id,SparkSchematicKind kind,float x,float y,string label,Color color) =>
            new SparkSchematicPart { id=id, kind=kind, position=P(x,y), label=label, color=color };
        private static SparkSchematicWire Wire(Color color,params Vector2[] points) =>
            new SparkSchematicWire { color=color, points=points };
    }
}
#endif

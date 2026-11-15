#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel76DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 76/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel76Builder.EnsureFolder();
            string path = SparkLevel76Builder.Folder + "/Level_76_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_76";
            drawing.levelTitle = "Forge the Connections";
            drawing.pages = new[] {
                Page("01 / Wire the copper workstation","110 WIRES  •  170 SOCKETS","Route rectifier, bath, wafer, metrology and safety. Connect W076 last.","POWER","FLOW","SAFETY"),
                Page("02 / Prepare and stabilize","BATH  •  SEED  •  PREWET","Condition electrolyte and verify the supplied patterned wafer. Calibrate current and reference potential.","BATH","PREWET","CALIBRATE"),
                Page("03 / Fill and inspect","16 REGIONS  •  COPPER FILL","Plate under potential control, rinse and dry, then inspect fill, voids and isolated test structures.","PLATE","RINSE","INSPECT"),
                Page("04 / Restore electrode potential","W076  •  RECTIFIER SAFE","Disconnect W076 while plating. Reconnect, recalibrate and verify reference, then certify a fresh wafer.","DISABLE","RECALIBRATE","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.71f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.71f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.71f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.71f),new Vector2(.43f,.71f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.71f,.71f),new Vector2(.75f,.71f)}}
                }
            };
        }
    }
}
#endif

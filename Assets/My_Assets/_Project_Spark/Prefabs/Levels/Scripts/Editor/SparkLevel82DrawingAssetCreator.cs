#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel82DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 82/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel82Builder.EnsureFolder();
            string path = SparkLevel82Builder.Folder + "/Level_82_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_82";
            drawing.levelTitle = "Silence the Spectrum";
            drawing.pages = new[] {
                Page("01 / Wire the EMC chamber","134 WIRES  •  206 SOCKETS","Wire the chamber, motion instruments and protected RF safety circuit. Connect W119 last.","POWER","MEASURE","SAFETY"),
                Page("02 / Find the interference paths","CONDUCTED  •  RADIATED  •  NEAR-FIELD","Measure the baseline with live workload. Sweep all antenna heights and polarizations. Locate cable and seam hot spots.","BASELINE","LOCATE","REPAIR"),
                Page("03 / Qualify emissions and immunity","72 POSES  •  4 EXPOSURE BANDS","Repair harness, gasket and filtering. Repeat both scans, then run the protected sweep and watch EUT function.","RETEST","EXPOSE","MONITOR"),
                Page("04 / Restore field trust","W119  •  PROTECTED SHUTDOWN","Remove W119 during qualified exposure. Reconnect, recalibrate and reset. Run fresh emissions and immunity tests.","INHIBIT","VERIFY","CERTIFY"),
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

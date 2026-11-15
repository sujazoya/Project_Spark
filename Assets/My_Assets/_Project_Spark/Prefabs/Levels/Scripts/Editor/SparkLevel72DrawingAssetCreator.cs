#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel72DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 72/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel72Builder.EnsureFolder();
            string path = SparkLevel72Builder.Folder + "/Level_72_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_72";
            drawing.levelTitle = "Layer by Layer";
            drawing.pages = new[] {
                Page("01 / Wire the deposition workbench","94 WIRES  •  146 SOCKETS","Connect control power and I/O, then install W94 last at the purge feedback terminals.","POWER","PURGE","SAFETY"),
                Page("02 / Commission and cycle","HEAT  •  DOSE  •  PURGE","Load and clamp a wafer after pumpdown. Stabilize heat and pressure. Demonstrate three A–purge–B–purge cycles.","CONDITION","DOSE","VERIFY"),
                Page("03 / Inspect the film","120 CYCLES  •  12.0 nm","Run Auto Batch and measure the conformal film. Diagnose purge restriction, heater drift and the stuck A valve.","DEPOSIT","INSPECT","REPAIR"),
                Page("04 / Restore purge feedback","W94  •  DOSE INHIBIT  •  RECERTIFY","During Purge A remove W94. Reconnect, recalibrate and verify purge. Reset and certify a fresh wafer.","ISOLATE","RECONNECT","CERTIFY"),
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

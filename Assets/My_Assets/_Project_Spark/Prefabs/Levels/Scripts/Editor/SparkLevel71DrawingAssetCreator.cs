#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel71DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 71/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel71Builder.EnsureFolder();
            string path = SparkLevel71Builder.Folder + "/Level_71_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_71";
            drawing.levelTitle = "Etch the Future";
            drawing.pages = new[] {
                Page("01 / Wire the plasma workbench","90 WIRES  •  140 SOCKETS","Route control power, vacuum, gas, matching, wafer, optics and safety. Install W90 last.","POWER","PROCESS I/O","SAFETY"),
                Page("02 / Commission the reactor","VACUUM  •  GAS FLOW  •  RF","Calibrate the sensors, pump down, load and clamp a wafer, then establish flow and tune the RF network.","VACUUM","MATCH","IGNITE"),
                Page("03 / Etch and inspect","OPTICAL ENDPOINT  •  METROLOGY","Enable endpoint control, etch until the spectrum changes, inspect the wafer and recover the three disturbances.","ETCH","INSPECT","RECOVER"),
                Page("04 / Restore optical feedback","W90  •  RF ISOLATION  •  RECERTIFY","With etch active, remove W90. Restore the wire, recalibrate, reset, and certify a fresh wafer.","ISOLATE","RECONNECT","CERTIFY"),
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

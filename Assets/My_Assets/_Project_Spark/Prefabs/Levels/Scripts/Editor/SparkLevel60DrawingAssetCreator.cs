#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel60DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 60/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel60Builder.EnsureFolder();
            string path = SparkLevel60Builder.Folder + "/Level_60_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_60";
            drawing.levelTitle = "The Grid Must Survive";
            drawing.pages = new[] {
                Page("01 / Build the network","46 WIRES  •  72 SOCKETS","Wire controls, battery converter, MPPT, the three sources and protected loads.","CONTROL","SOURCES","LOADS"),
                Page("02 / Configure dispatch","RESERVE  •  START LIMIT  •  PRIORITY","Calibrate and configure the reserve, generator start threshold and shedding order before enabling AUTO.","CALIBRATE","SET LIMITS","AUTO"),
                Page("03 / Survive the emergency","CLOUD COVER  →  OUTAGE  →  SURGE","Observe solar loss and islanding. Apply the surge, then fail the generator and watch priority shedding.","CLOUD","OUTAGE","COMPOUND"),
                Page("04 / Recover and certify","GENERATOR  →  UTILITY  →  LOADS","Repair and reset. Wait for automatic synchronization of each source, restore motor and auxiliary and certify.","REPAIR","RESYNC","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.60f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.60f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.60f),new Vector2(.43f,.60f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.60f,.60f),new Vector2(.75f,.60f)}}
                }
            };
        }
    }
}
#endif

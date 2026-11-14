#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel64DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 64/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel64Builder.EnsureFolder();
            string path = SparkLevel64Builder.Folder + "/Level_64_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_64";
            drawing.levelTitle = "Hold the Synchronism";
            drawing.pages = new[] {
                Page("01 / Wire the stability lab","62 WIRES  •  96 SOCKETS","Connect both sources, the dual feeders, relay routing and PMUs. Install W62 last.","GENERATORS","DUAL FEEDERS","PROTECTION"),
                Page("02 / Synchronize","VOLTAGE  •  FREQUENCY  •  PHASE","Calibrate and wait for the relay window before closing the source breakers.","MATCH","HOLD","CLOSE"),
                Page("03 / Clear and damp","MEASURED CLEARING TIME  •  ESTIMATED CCT","Clear Feeder A, observe rotor-angle motion and use PSS to damp a load step.","FAULT","ISOLATE","DAMP"),
                Page("04 / Recover and certify","BREAKER FAILURE  •  W62 PMU LOSS","Prove upstream backup for a stuck breaker. Restore PMU B, resynchronize and repeat the selective fault.","BACKUP","REPAIR","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.64f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.64f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.64f),new Vector2(.43f,.64f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.64f,.64f),new Vector2(.75f,.64f)}}
                }
            };
        }
    }
}
#endif

#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel58DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 58/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel58Builder.EnsureFolder();
            string path = SparkLevel58Builder.Folder + "/Level_58_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_58";
            drawing.levelTitle = "Isolate the Fault";
            drawing.pages = new[] {
                Page("01 / Build the network","38 WIRES  •  60 SOCKETS","Wire the control rails, ABC distribution and relay feedback. W16 carries CTB to R2.","5 V CONTROL","ABC NETWORK","CT / TRIP"),
                Page("02 / Grade the relays","PICKUP  •  CURVE  •  DELAY","Calibrate CTs and verify pickup, time multiplier, curve and high-set settings.","NORMAL LOAD","FEEDER FIRST","MAIN BACKUP"),
                Page("03 / Prove selectivity","OVERLOAD A  •  SHORT B","The faulted breaker clears while the other feeders stay energized.","Q1 / OVERLOAD","Q2 / SHORT","Q3 / PRIORITY"),
                Page("04 / Repair and certify","Q2 FAILURE  •  W16 LOSS","Prove Q0 backup. Restore Q2, reconnect W16 and repeat a selective test.","BACKUP Q0","REPAIR CTB","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.58f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.58f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.58f),new Vector2(.43f,.58f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.58f,.58f),new Vector2(.75f,.58f)}}
                }
            };
        }
    }
}
#endif

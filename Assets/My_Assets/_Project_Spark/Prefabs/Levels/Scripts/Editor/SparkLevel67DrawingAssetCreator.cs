#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel67DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 67/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel67Builder.EnsureFolder();
            string path = SparkLevel67Builder.Folder + "/Level_67_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_67";
            drawing.levelTitle = "Master the Flow";
            drawing.pages = new[] {
                Page("01 / Wire the process plant","74 WIRES  •  116 SOCKETS","Route power, drives, transmitters, valves and safety. Install W74 last.","POWER","SIGNALS","SAFETY"),
                Page("02 / Prime and tune","SUCTION  •  LEVEL  •  FLOW","Calibrate, prime A and B, start feed and mixing, prime the transfer pump, then tune outer level and inner flow.","PRIME","MEASURE","TUNE"),
                Page("03 / Make on-spec product","70% A  /  30% B","Use real flows and blend readings to certify delivery. Verify response to demand, pressure loss and Pump B failure.","RATIO","PRODUCE","DISTURB"),
                Page("04 / Recover Supply B","W74  •  RECALIBRATE","Undo the real Flow B connection, observe automatic isolation, reconnect, recalibrate, reset and repeat recipe and demand tests.","ISOLATE","RECOVER","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.67f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.67f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.67f),new Vector2(.43f,.67f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.67f,.67f),new Vector2(.75f,.67f)}}
                }
            };
        }
    }
}
#endif

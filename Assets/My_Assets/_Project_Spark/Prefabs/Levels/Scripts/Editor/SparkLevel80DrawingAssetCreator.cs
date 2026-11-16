#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel80DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 80/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel80Builder.EnsureFolder();
            string path = SparkLevel80Builder.Folder + "/Level_80_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_80";
            drawing.levelTitle = "Command the Light";
            drawing.pages = new[] {
                Page("01 / Build the optical control network","126 WIRES  •  194 SOCKETS","Wire power, transmitter, ring heaters, receiver and safety. Connect W078 last.","POWER","CONTROL","SAFETY"),
                Page("02 / Commission and lock four carriers","SOURCE  •  FIBER  •  RINGS A–D","Test shutters, calibrate the source and receiver, dock the fiber and tune four rings. Verify spacing and crosstalk.","CALIBRATE","TUNE","VERIFY"),
                Page("03 / Transmit and qualify data","POWER  •  EYE  •  BER / FEC","Measure all four power budgets, recover four clocks and qualify Logic, Memory, I/O and Sensor traffic.","BUDGET","CLOCK","DATA"),
                Page("04 / Restore trusted receiver power","W078  •  INDEPENDENT SHUTTER","Remove W078 during live traffic. Verify source decay and shutter closure, reconnect, recalibrate, reset and run fresh traffic.","SHUTTER","RECOVER","CERTIFY"),
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

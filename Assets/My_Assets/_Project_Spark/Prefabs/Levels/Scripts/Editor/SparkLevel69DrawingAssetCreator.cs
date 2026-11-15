#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel69DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 69/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel69Builder.EnsureFolder();
            string path = SparkLevel69Builder.Folder + "/Level_69_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_69";
            drawing.levelTitle = "Keep It Cold";
            drawing.pages = new[] {
                Page("01 / Wire the refrigeration network","82 WIRES  •  128 SOCKETS","Connect the pressure and temperature sensors, drives, EEVs and protection. Install W82 last.","POWER","SENSORS","SAFETY"),
                Page("02 / Commission and cool","4°C / 2°C  •  5–8 K SUPERHEAT","Validate both feedback channels. Tune superheat and both zone loops. Check actual refrigerant states.","PRESSURE","SUPERHEAT","ZONES"),
                Page("03 / Defrost and protect","FROST  •  CONDENSER  •  COP","Isolate and defrost B. Verify condensate. Apply the door load and fan failure, then compare optimized COP.","DEFROST","PROTECT","OPTIMIZE"),
                Page("04 / Restore Evaporator A","W82  •  RECALIBRATE  •  VERIFY","Remove the actual temperature feedback wire. Reconnect, recalibrate and reset before a fresh disturbance test.","ISOLATE","RECONNECT","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.69f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.69f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.69f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.69f),new Vector2(.43f,.69f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.69f,.69f),new Vector2(.75f,.69f)}}
                }
            };
        }
    }
}
#endif

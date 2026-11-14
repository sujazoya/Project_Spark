#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel62DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 62/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel62Builder.EnsureFolder();
            string path = SparkLevel62Builder.Folder + "/Level_62_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_62";
            drawing.levelTitle = "Hold the Voltage";
            drawing.pages = new[] {
                Page("01 / Build the DVR","54 WIRES  •  84 SOCKETS","Wire control rails, the three series transformers and the voltage feedback. Connect W54 last.","SOURCE","INJECTION","PROTECTED"),
                Page("02 / Commission","CALIBRATE  •  PRECHARGE  •  PLL","Check the energy bank. Wait for the DC link and PLL, then enable the DVR.","STORAGE","PRECHARGE","PHASE"),
                Page("03 / Protect the load","0.95–1.05 PU","Test sag, swell and imbalance. Observe signed per-phase injection, then start the heavy motor.","SAG","SWELL / UNBALANCE","INRUSH"),
                Page("04 / Repair and certify","W54  •  INJECTION VOLTAGE","Remove actual injection feedback. Verify bypass, reconnect, reset, recalibrate, enable and repeat a sag.","BYPASS","REPAIR","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.62f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.62f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.62f),new Vector2(.43f,.62f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.62f,.62f),new Vector2(.75f,.62f)}}
                }
            };
        }
    }
}
#endif

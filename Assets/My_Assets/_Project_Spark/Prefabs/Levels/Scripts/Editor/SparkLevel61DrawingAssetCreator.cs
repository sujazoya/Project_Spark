#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel61DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 61/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel61Builder.EnsureFolder();
            string path = SparkLevel61Builder.Folder + "/Level_61_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_61";
            drawing.levelTitle = "Clean the Wave";
            drawing.pages = new[] {
                Page("01 / Wire the power-quality cell","50 WIRES  •  78 SOCKETS","Connect the control rails, three phases, nonlinear loads and CT feedback. Install W50 last.","5 V","ABC NETWORK","CT FEEDBACK"),
                Page("02 / Measure distortion","5TH  •  7TH  •  11TH  •  13TH","Energize, calibrate, enable loads and lock the PLL. Record the baseline source current and spectrum.","WAVEFORM","SPECTRUM","BASELINE"),
                Page("03 / Compensate and test","THD < 5%  •  DPF ≥ 0.98","Enable and tune current cancellation. Switch the capacitor bank and apply the industrial load step.","CANCEL","CORRECT","STEP LOAD"),
                Page("04 / Repair and certify","W50  •  FILTER CT FEEDBACK","Disconnect W50. Reconnect, reset, calibrate and reacquire PLL. Verify low THD and corrected power factor before certifying.","SIGNAL LOST","RECOVER","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.61f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.61f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.61f),new Vector2(.43f,.61f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.61f,.61f),new Vector2(.75f,.61f)}}
                }
            };
        }
    }
}
#endif

#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel55DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 55/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel55Builder.EnsureFolder();
            string path = SparkLevel55Builder.Folder + "/Level_55_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_55";
            drawing.levelTitle = "Float the Shaft";
            drawing.pages = new[] {
                Page("01 / Build the bearing system", "28 WIRES  •  48 SOCKETS",
                    "Wire the two magnetic drivers and four independent sensors. Install W13 last.",
                    "SUPPLY", "FEEDBACK", "COILS"),
                Page("02 / Calibrate and lift", "TOUCHDOWN  •  CLOSED LOOP",
                    "Calibrate on the resting journals. The magnetic forces must overcome gravity before the shaft clears its supports.",
                    "CALIBRATE", "LIFT", "45 RPM"),
                Page("03 / Tune and certify", "LEFT AND RIGHT DISTURBANCES  •  75 RPM",
                    "Apply the two disturbances at 45 RPM. Compare peak gap error and settling time, then certify at 75 RPM.",
                    "DISTURB", "RECOVER", "75 RPM"),
                Page("04 / Touchdown and recovery", "W13 RIGHT X  •  SAFE STOP",
                    "Observe the fault and touchdown, restore W13, reset, recalibrate, relift, and certify again.",
                    "W13", "TOUCHDOWN", "RELIFT"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.55f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.55f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.55f),new Vector2(.43f,.55f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.57f,.55f),new Vector2(.75f,.55f)}}
                }
            };
        }
    }
}
#endif

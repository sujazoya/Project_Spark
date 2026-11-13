#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel52DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 52/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel52Builder.EnsureFolder();
            string path = SparkLevel52Builder.Folder + "/Level_52_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_52";
            drawing.levelTitle = "Cancel the Shake";
            drawing.pages = new[] {
                Page("01 / Wire the balance rig", "19 WIRES  •  32 SOCKETS",
                    "Wire both drives, the vibration pickup, A/B/Z encoder and guard. W19 is the once-per-turn reference.",
                    "5 V", "DRIVES", "A / B / Z"),
                Page("02 / Measure shaft phase", "45 RPM  •  FIVE REVOLUTIONS",
                    "Capture the 1× vibration vector, move both weights to a known trial correction and remeasure.",
                    "BASELINE", "TRIAL VECTOR", "INFLUENCE"),
                Page("03 / Cancel the shake", "TWO EQUAL CORRECTION MASSES",
                    "Use the measured influence to find both target angles. Verify lower vibration at 45 and then 60 RPM.",
                    "MASS A", "MASS B", "RESIDUAL"),
                Page("04 / Recover the index", "W19 OPEN  •  CONTROLLED STOP",
                    "Remove W19 while running. Restore the wire, reset after stopping, and restart to reacquire phase and verify balance.",
                    "ENCODER Z", "SAFE STOP", "RESTART"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.52f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.52f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.52f),new Vector2(.43f,.52f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.57f,.52f),new Vector2(.75f,.52f)}}
                }
            };
        }
    }
}
#endif

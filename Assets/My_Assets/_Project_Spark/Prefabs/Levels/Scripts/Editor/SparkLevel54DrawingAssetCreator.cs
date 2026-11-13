#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel54DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 54/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel54Builder.EnsureFolder();
            string path = SparkLevel54Builder.Folder + "/Level_54_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_54";
            drawing.levelTitle = "Cross the Critical";
            drawing.pages = new[] {
                Page("01 / Build and calibrate", "22 WIRES  •  36 SOCKETS",
                    "Install W15 last. Zero both powered probes at rest, then start at 45 RPM and capture the reference.",
                    "X AND Y", "ENCODER Z", "45 RPM"),
                Page("02 / Find the critical speed", "LOW DAMPING  •  MEASURED SWEEP",
                    "Measure amplitude and phase across eight speeds. Resonance occurs near the natural speed, where the excitation and shaft motion shift phase.",
                    "45 RPM", "AMPLITUDE PEAK", "72 RPM"),
                Page("03 / Verify active damping", "COMPARE RUNS  •  ≥50% REDUCTION",
                    "Repeat the same sweep with active damping. The actuator opposes lateral velocity and reduces the measured peak.",
                    "VELOCITY", "DAMPER FORCE", "PEAK REDUCTION"),
                Page("04 / Cross and recertify", "75 RPM  •  W15 SENSOR LOSS",
                    "Cross at 75 RPM, coast down, then test W15. Restore the wire, reset, recalibrate and run the crossing again.",
                    "75 RPM", "SAFE STOP", "RECERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.54f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.54f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.54f),new Vector2(.43f,.54f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.57f,.54f),new Vector2(.75f,.54f)}}
                }
            };
        }
    }
}
#endif

#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel53DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 53/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel53Builder.EnsureFolder();
            string path = SparkLevel53Builder.Folder + "/Level_53_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_53";
            drawing.levelTitle = "The Hidden Couple";
            drawing.pages = new[] {
                Page("01 / Wire both planes", "24 WIRES  •  40 SOCKETS",
                    "Connect both bearing pickups, both actuators and A/B/Z. Install W15 last.",
                    "LEFT / RIGHT", "A / B / Z", "PLANES A / B"),
                Page("02 / Discover the couple", "45 RPM  •  TWO BEARINGS",
                    "A twisting couple can produce similar amplitudes at opposite phases. Check both bearings before accepting a correction.",
                    "LEFT PHASE", "ROTOR", "RIGHT PHASE"),
                Page("03 / Calibrate and solve", "TRIAL A  •  TRIAL B  •  COMPLEX MATRIX",
                    "Each trial measures its effect on both bearings. Solve both corrections together and verify both residuals.",
                    "TRIAL A", "MATRIX", "TRIAL B"),
                Page("04 / Certify and recover", "60 RPM  •  W15 SENSOR LOSS",
                    "Verify at speed, undo W15, reconnect it after safe stop, reset and verify both bearings again.",
                    "SENSOR LOSS", "SAFE STOP", "RECERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.53f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.53f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.53f),new Vector2(.43f,.53f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.57f,.53f),new Vector2(.75f,.53f)}}
                }
            };
        }
    }
}
#endif

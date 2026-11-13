#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel48DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 48/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel48Builder.EnsureFolder();
            string path = SparkLevel48Builder.Folder + "/Level_48_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_48";
            drawing.levelTitle = "Hit the Mark";
            drawing.pages = new[] {
                Page("01 / Wire the registration line", "13 WIRES  •  22 SOCKETS",
                    "Wire the 5 V and return branches, encoder A/B, MARK IN, driver PWM and motor, and PRINT TRIG. The two supply sockets each accept three wires.",
                    "5 V / RETURN", "CONTROL / DRIVER", "EYE / ENCODER"),
                Page("02 / Calibrate the optical eye", "TWO GENUINE MARKS  •  160 COUNTS",
                    "CALIBRATE crawls the web at 18 RPM. The eye observes two genuine marks. Their measured distance sets the mark pitch.",
                    "MARK 1", "160 COUNTS", "MARK 2"),
                Page("03 / Predict the print position", "80 COUNTS  •  ±2 MM",
                    "Verify three prints at 30 RPM, then select 60 RPM while moving. The controller uses encoder distance and allows for the powered head stroke.",
                    "OPTICAL EYE", "80 COUNTS", "PRINT HEAD"),
                Page("04 / Prove safe recovery", "GLARE  •  MISSING MARK  •  RESET",
                    "Run GLARE TEST and verify a genuine print. Cover the eye with MISSING MARK and wait for safe stop. Restore the eye, RESET FAULT, then START and verify a recovered print.",
                    "REJECT GLARE", "SAFE STOP", "RESET / START"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.49f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.50f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.49f),new Vector2(.43f,.49f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.57f,.49f),new Vector2(.75f,.49f)}}
                }
            };
        }
    }
}
#endif

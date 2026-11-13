#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel49DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 49/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel49Builder.EnsureFolder();
            string path = SparkLevel49Builder.Folder + "/Level_49_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_49";
            drawing.levelTitle = "Cut on the Mark";
            drawing.pages = new[] {
                Page("01 / Build the flying shear", "15 WIRES  •  26 SOCKETS",
                    "Connect power and return, registration feedback, carriage drive, cutter trigger, guard and home. Verify the full wire map.",
                    "5 V / RETURN", "CARRIAGE DRIVER", "GUARD / HOME"),
                Page("02 / Home and calibrate", "HOME  •  TWO MARKS  •  160 COUNTS",
                    "Seek the home sensor, then measure two real marks while the web moves slowly.",
                    "HOME SENSOR", "160 COUNTS", "OPTICAL EYE"),
                Page("03 / Match the web", "30 / 60 RPM  •  25 MM TRACKING",
                    "The carriage tracks the moving mark through the blade stroke. Check both the position error and the speed difference.",
                    "ACCELERATE", "SYNC / CUT", "RETURN"),
                Page("04 / Prove recovery", "SPEED TEST  •  GUARD TEST",
                    "Recover from a speed dip, then open the guard. Close it and explicitly reset, re-home and restart.",
                    "SPEED DIP", "GUARD TRIP", "RESET / HOME"),
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

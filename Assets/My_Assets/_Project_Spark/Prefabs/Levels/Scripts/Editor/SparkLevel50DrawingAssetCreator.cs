#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel50DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 50/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel50Builder.EnsureFolder();
            string path = SparkLevel50Builder.Folder + "/Level_50_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_50";
            drawing.levelTitle = "Zero Defect Factory";
            drawing.pages = new[] {
                Page("01 / Wire the factory", "22 WIRES  •  38 SOCKETS",
                    "Connect four branches of 5 V and return, then optical, encoder, station commands, vision, motor, bin and safety feedback.",
                    "5 V / RETURN", "FACTORY CONTROL", "FEEDBACK"),
                Page("02 / Track each product", "80 COUNTS PER STATION",
                    "Calibrate from two genuine marks. An ID moves from print to cut, inspection and sorting according to encoder distance.",
                    "MARK", "PRINT / CUT", "INSPECT"),
                Page("03 / Prove quality", "GOOD AND REJECT",
                    "Produce three good pieces at 30 RPM. At 60 RPM, run a mixed batch and confirm each product using its bin sensor.",
                    "INSPECTION", "DIVERTER", "BIN CONFIRMATION"),
                Page("04 / Recover safely", "SPEED DIP  •  FALSE MARK  •  E-STOP",
                    "Verify tracking through a speed dip and reject a false pulse. Stop the line, restore safety, reset, home, calibrate and produce four good pieces.",
                    "DISTURBANCE", "SAFE STOP", "RESTART"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.50f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.50f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.50f),new Vector2(.43f,.50f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.57f,.50f),new Vector2(.75f,.50f)}}
                }
            };
        }
    }
}
#endif

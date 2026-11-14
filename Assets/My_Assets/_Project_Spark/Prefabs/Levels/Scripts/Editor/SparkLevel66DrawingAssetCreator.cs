#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel66DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 66/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel66Builder.EnsureFolder();
            string path = SparkLevel66Builder.Folder + "/Level_66_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_66";
            drawing.levelTitle = "Save the Factory";
            drawing.pages = new[] {
                Page("01 / Wire the condition network","70 WIRES  •  110 SOCKETS","Connect the VFD, measurements, diagnostics and safety. Install W70 last.","DRIVE","MEASUREMENT","SAFETY"),
                Page("02 / Diagnose the bearing","BASELINE  •  RMS  •  FFT","Record a healthy baseline. Compare the new spectrum with bearing race, alignment and gear-mesh frequencies.","BASELINE","BPFO","DIAGNOSE"),
                Page("03 / Isolate and repair","ZERO SPEED  •  LOCKOUT","Stop, verify isolation and lock out before opening the guard. Extract, replace, align and verify torque.","ISOLATE","REPLACE","VERIFY"),
                Page("04 / Validate and recover","LOAD TEST  •  W70","Verify the repair at low and nominal speed under load. Disconnect and restore the primary vibration signal and re-certify.","TEST","RECOVER","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.66f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.66f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.66f),new Vector2(.43f,.66f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.66f,.66f),new Vector2(.75f,.66f)}}
                }
            };
        }
    }
}
#endif

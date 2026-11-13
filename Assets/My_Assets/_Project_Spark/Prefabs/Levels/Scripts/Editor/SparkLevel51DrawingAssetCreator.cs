#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel51DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 51/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel51Builder.EnsureFolder();
            string path = SparkLevel51Builder.Folder + "/Level_51_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_51";
            drawing.levelTitle = "Predict the Failure";
            drawing.pages = new[] {
                Page("01 / Wire the diagnostic rig", "19 WIRES  •  32 SOCKETS",
                    "Place the shunt in series with the motor. Run its two independent sense leads to the controller.",
                    "5 V / RETURN", "DRIVER / SHUNT", "SENSORS"),
                Page("02 / Capture a healthy reference", "45 RPM  •  STABLE SAMPLES",
                    "Capture the baseline once speed, current, temperature and vibration settle.",
                    "ENCODER", "BASELINE", "HEALTH"),
                Page("03 / Compare fault signatures", "SYNCHRONOUS VS IRREGULAR VIBRATION",
                    "An eccentric rotor produces vibration at shaft frequency. Bearing wear adds irregular vibration, higher current and heating.",
                    "ROTOR", "SIGNATURE", "BEARING"),
                Page("04 / Prevent and repair", "WARNING  •  STOP  •  VERIFY",
                    "Stop during the warning window. Open the guard, replace the bearing, correct the balance, reset and verify at 45 RPM.",
                    "TREND", "SERVICE", "CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.51f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.51f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.51f),new Vector2(.43f,.51f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.57f,.51f),new Vector2(.75f,.51f)}}
                }
            };
        }
    }
}
#endif

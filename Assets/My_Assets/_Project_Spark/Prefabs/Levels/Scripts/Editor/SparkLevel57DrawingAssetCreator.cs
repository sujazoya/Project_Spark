#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel57DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 57/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel57Builder.EnsureFolder();
            string path = SparkLevel57Builder.Folder + "/Level_57_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_57";
            drawing.levelTitle = "Synchronize the Grid";
            drawing.pages = new[] {
                Page("01 / Wire the microgrid", "34 WIRES  •  54 SOCKETS",
                    "Connect the 5 V rails, three ordered phases and the sensing and protection signals. Install W24 last.",
                    "5 V DC", "ABC BUS", "FEEDBACK"),
                Page("02 / Synchronize the sources", "±0.2 HZ  •  ±5°  •  ±5% VOLTAGE",
                    "Match frequency, phase and voltage before closing the three-pole contactor.",
                    "GRID A", "SYNC WINDOW", "INVERTER B"),
                Page("03 / Share and island", "40–60% SHARE  •  B FORMS THE ISLAND",
                    "Apply a load step. Check both source powers, then isolate A and sustain essential loads on B.",
                    "LOAD STEP", "SHARING", "ISLAND"),
                Page("04 / Repair and recertify", "W24  •  GRID VOLTAGE",
                    "After restoration, disconnect W24. Reconnect, reset, calibrate and resynchronize before final certification.",
                    "FEEDBACK LOST", "OPEN TIE", "RECERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.57f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.57f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.57f),new Vector2(.43f,.57f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.57f,.57f),new Vector2(.75f,.57f)}}
                }
            };
        }
    }
}
#endif

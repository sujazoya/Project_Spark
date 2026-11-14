#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel56DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 56/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel56Builder.EnsureFolder();
            string path = SparkLevel56Builder.Folder + "/Level_56_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_56";
            drawing.levelTitle = "Harvest the Spin";
            drawing.pages = new[] {
                Page("01 / Wire the recovery rig", "30 WIRES  •  48 SOCKETS",
                    "Connect the converter, storage protection, chopper and measurement channels. Install W16 last.",
                    "SUPPLY", "CONVERTER", "BANK"),
                Page("02 / Release kinetic energy", "90 → 20 RPM",
                    "Compare the flywheel energy change with the energy gained by the capacitor. Losses include winding heat, friction and converter heat.",
                    "FLYWHEEL", "GENERATOR", "SUPERCAP"),
                Page("03 / Optimize and reuse", "≥60% RECOVERY",
                    "Repeat with a lower current. Compare supply work for matched runs from 20 RPM to 90 RPM.",
                    "LOW CURRENT", "VERIFY", "REUSE"),
                Page("04 / Recover a feedback fault", "W16  •  CAPACITOR VOLTAGE",
                    "Disconnect W16 and confirm the storage relay opens. Reconnect, reset, calibrate and repeat regeneration.",
                    "FEEDBACK", "SAFE STOP", "RECERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.56f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.56f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.56f),new Vector2(.43f,.56f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.57f,.56f),new Vector2(.75f,.56f)}}
                }
            };
        }
    }
}
#endif

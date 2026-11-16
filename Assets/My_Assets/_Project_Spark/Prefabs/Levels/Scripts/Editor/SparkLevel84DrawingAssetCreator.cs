#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel84DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 84/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel84Builder.EnsureFolder();
            string path = SparkLevel84Builder.Folder + "/Level_84_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_84";
            drawing.levelTitle = "Defy the Tumble";
            drawing.pages = new[] {

                Page("01 / Build the ADCS","142 DIRECT WIRES","Follow the numbered routes at the equipment. Keep W097 accessible for the final test.","WIRE","POWER","TEST"),
                Page("02 / Stabilize and point","GYRO  •  STARS  •  WHEELS","Calibrate the gyro, reduce actual body rates, acquire the primary star tracker, and settle a three-axis slew.","DETUMBLE","ACQUIRE","POINT"),
                Page("03 / Unload momentum and hold two passes","TORQUERS  •  BORESIGHT","Unload excess wheel momentum using the changing field. Align optics and qualify both scheduled ground passes.","UNLOAD","ALIGN","TRACK"),
                Page("04 / Recover trusted attitude","W097  •  STALE  •  RESET","Remove W097 during a live link. Restore the wire, obtain a fresh primary fix, check coarse safety, reset and run a new two-pass mission.","INHIBIT","REACQUIRE","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.71f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.71f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.71f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.71f),new Vector2(.43f,.71f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.71f,.71f),new Vector2(.75f,.71f)}}
                }
            };
        }
    }
}
#endif

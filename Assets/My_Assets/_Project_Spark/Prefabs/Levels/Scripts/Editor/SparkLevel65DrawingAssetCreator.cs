#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel65DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 65/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel65Builder.EnsureFolder();
            string path = SparkLevel65Builder.Folder + "/Level_65_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_65";
            drawing.levelTitle = "The Factory Thinks";
            drawing.pages = new[] {
                Page("01 / Build the factory","66 WIRES  •  104 SOCKETS","Connect power, sensors, drives, vacuum, pneumatics and safety. Add W66 last.","POWER","CONTROL","SAFETY"),
                Page("02 / Prove inspection","BELT ENCODER  •  CAMERA","Commission the PLC, calibrate motion and vision and track a test part.","DETECT","INSPECT","TRACK"),
                Page("03 / Sort and optimize","A / B / C  •  REJECT","Home the robot, commission services and prove sorting. Tune belt and cycle speeds for the mixed batch.","PICK","PLACE","MEASURE"),
                Page("04 / Recover and certify","W66  •  20-PART BATCH","Disconnect actual encoder feedback. Restore the signal, recalibrate and reset. Run a fresh certification batch.","STOP","RECOVER","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.65f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.65f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.65f),new Vector2(.43f,.65f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.65f,.65f),new Vector2(.75f,.65f)}}
                }
            };
        }
    }
}
#endif

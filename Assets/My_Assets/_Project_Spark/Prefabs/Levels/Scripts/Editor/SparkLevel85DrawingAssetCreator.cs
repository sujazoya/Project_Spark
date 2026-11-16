#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel85DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 85/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel85Builder.EnsureFolder();
            string path = SparkLevel85Builder.Folder + "/Level_85_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_85";
            drawing.levelTitle = "The Last Meter";
            drawing.pages = new[] {


                Page("01 / Assemble rendezvous controls","146 EQUIPMENT WIRES","Use the direct Wire tool. Match terminal identities and keep the W123 cable accessible.","WIRE","POWER","TEST"),
                Page("02 / Fly the approach corridor","100 M  •  30 M  •  5 M","Calibrate the navigation sensors and test a finite RCS pulse. Stop at the three holds before the final docking segment.","CALIBRATE","APPROACH","HOLD"),
                Page("03 / Capture and verify the dock","CONTACT  •  6 LATCHES  •  SEAL","Wait for compliant capture and verify all six independent latch travels, the electrical coupler and the pressure seal.","CAPTURE","LATCH","VERIFY"),
                Page("04 / Repair the closing-rate channel","W123  •  FRESH RATE  •  RESET","Open W123 while closing. Watch controlled braking. Restore fresh readings, check safe, reset, return to staging and fly a new complete approach.","ABORT","RECONNECT","CERTIFY"),
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

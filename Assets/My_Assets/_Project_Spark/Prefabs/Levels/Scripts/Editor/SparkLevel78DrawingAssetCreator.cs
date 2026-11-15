#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel78DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 78/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel78Builder.EnsureFolder();
            string path = SparkLevel78Builder.Folder + "/Level_78_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_78";
            drawing.levelTitle = "Seal the Signal";
            drawing.pages = new[] {
                Page("01 / Wire the dielectric cluster","118 WIRES  •  182 SOCKETS","Connect vacuum, deposition, cure, metrology and safety. Add W083 last.","POWER","PROCESS","RELIABILITY"),
                Page("02 / Prepare and seal copper","VACUUM  •  PREP  •  CAP","Pump down, inspect and precondition copper. Calibrate flow and RF, deposit the cap and verify continuity.","PUMP DOWN","CAP","INSPECT"),
                Page("03 / Build and qualify low-k","LOW-K  •  CURE  •  SCREEN","Deposit the dielectric, open the UV shutter during cure and then verify film and coupon quality.","LOW-K","UV CURE","LEAKAGE"),
                Page("04 / Restore leakage feedback","W083  •  DISCHARGE","Disconnect W083 during coupon stress. Watch the fixture discharge. Reconnect, calibrate, verify, reset and qualify a fresh wafer.","DISCHARGE","CALIBRATE","CERTIFY"),
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

#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel74DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 74/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel74Builder.EnsureFolder();
            string path = SparkLevel74Builder.Folder + "/Level_74_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_74";
            drawing.levelTitle = "Align the Light";
            drawing.pages = new[] {
                Page("01 / Wire the lithography workstation","102 WIRES  •  158 SOCKETS","Route power, stages, alignment, optics, process and safety. Connect interferometer W102 last.","POWER","STAGES","SAFETY"),
                Page("02 / Prepare and align","SPIN COAT  •  BAKE  •  4:1","Prepare a 350 nm resist film, load the reticle, align the wafer, map focus and run a dry scan.","COAT","ALIGN","VERIFY"),
                Page("03 / Print and measure","16 FIELDS  •  CD AND OVERLAY","Expose all sixteen fields, post-bake, develop, rinse and dry. Verify metrology and diagnose the three process disturbances.","EXPOSE","DEVELOP","SCAN"),
                Page("04 / Restore stage feedback","W102  •  SHUTTER SAFE","Disconnect W102 during a scan. Reconnect, recalibrate, reference and reset before printing a fresh wafer.","BRAKE","REFERENCE","CERTIFY"),
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

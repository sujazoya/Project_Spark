#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel75DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 75/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel75Builder.EnsureFolder();
            string path = SparkLevel75Builder.Folder + "/Level_75_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_75";
            drawing.levelTitle = "Write the Silicon";
            drawing.pages = new[] {
                Page("01 / Wire the implanter","106 WIRES  •  164 SOCKETS","Route beam optics, vacuum and safety. Connect W067 last for dosimetry loss.","POWER","BEAM","SAFETY"),
                Page("02 / Qualify the beam","VACUUM  •  SPECIES  •  PROFILE","Commission vacuum and wafer handling. Qualify the analyzer, beam profile and Faraday calibration.","VACUUM","PROFILE","CALIBRATE"),
                Page("03 / Implant and activate","16 REGIONS  •  100 DOSE","Implant a masked wafer, activate, then verify depth and sheet resistance. Diagnose all three disturbances.","IMPLANT","ACTIVATE","SCAN"),
                Page("04 / Restore dosimetry","W067  •  BEAM BLANKING","Disconnect W067 during the live raster. Reconnect, zero, verify, reset and certify a fresh wafer.","BLANK","RECALIBRATE","CERTIFY"),
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

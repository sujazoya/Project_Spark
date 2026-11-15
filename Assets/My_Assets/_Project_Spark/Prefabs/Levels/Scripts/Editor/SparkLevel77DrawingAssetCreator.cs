#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel77DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 77/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel77Builder.EnsureFolder();
            string path = SparkLevel77Builder.Folder + "/Level_77_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_77";
            drawing.levelTitle = "Reveal the Circuit";
            drawing.pages = new[] {
                Page("01 / Wire the triple CMP bench","114 WIRES  •  176 SOCKETS","Route power, drives, slurry, sensors and safety. Connect W055 last for the recovery test.","POWER","OPTICS","SAFETY"),
                Page("02 / Commission and polish","BULK  •  SOFT LAND  •  BARRIER","Calibrate copper thickness and barrier optics. Inspect the loaded wafer, then polish and transfer between all three stations.","BULK","SOFT LAND","BARRIER"),
                Page("03 / Clean and isolate","16 REGIONS  •  COPPER / BARRIER","Protect, brush, megasonically clean, rinse and dry. Inspect geometry, via continuity and interline isolation.","CLEAN","GEOMETRY","ELECTRICAL"),
                Page("04 / Restore endpoint feedback","W055  •  SAFE STOP","Disconnect W055 during Stage C. Reconnect, recalibrate, verify and reset. Certify a fresh wafer.","SAFE STOP","RECALIBRATE","CERTIFY"),
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

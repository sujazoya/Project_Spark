#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel59DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 59/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel59Builder.EnsureFolder();
            string path = SparkLevel59Builder.Folder + "/Level_59_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_59";
            drawing.levelTitle = "Bring Back the Power";
            drawing.pages = new[] {
                Page("01 / Build the control network","42 WIRES  •  66 SOCKETS","Wire control power, battery paths and the ordered A/B/C connections.","5 V CONTROL","BATTERY","ABC BUS"),
                Page("02 / Energize the dead bus","BMS  •  PRECHARGE  •  50 HZ","Verify QG open. Enable BMS, precharge the DC link, then form the island and restore the essential feeders.","QG OPEN","PRECHARGE","ESSENTIALS"),
                Page("03 / Synchronize and restore","ΔF ≤ 0.2 HZ  •  ΔPHASE ≤ 5°","Start and synchronize the generator. Close QGEN within the relay window, then bring on motor and auxiliary loads.","START","MATCH","RESTORE"),
                Page("04 / Recover and certify","ISOLATE  •  SHED  •  RESTORE","Prove essential service after generator failure. Repair, reset, resynchronize and restore full load before certifying.","FAILURE","RECOVERY","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.59f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.59f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.59f),new Vector2(.43f,.59f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.59f,.59f),new Vector2(.75f,.59f)}}
                }
            };
        }
    }
}
#endif

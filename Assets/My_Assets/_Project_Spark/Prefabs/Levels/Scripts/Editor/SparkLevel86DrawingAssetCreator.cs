#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel86DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 86/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel86Builder.EnsureFolder();
            string path = SparkLevel86Builder.Folder + "/Level_86_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_86";
            drawing.levelTitle = "The Orbital Forge";
            drawing.pages = new[] {


                Page("01 / Commission the service cell","152 DIRECT WIRES  •  7 AXES","Wire the equipment, test the independent interlock, home every axis and calibrate the wrist and stereo reference.","WIRE","HOME","TEST"),
                Page("02 / Remove the failed radiator","ISOLATE  •  UNBOLT  •  CRADLE","Vent the coolant loop, release all fasteners and confirm a supported grip before moving the old radiator.","ISOLATE","GRIP","EXTRACT"),
                Page("03 / Fit and cool","INSTALL  •  TORQUE  •  TEST FLOW","Fetch the new radiator, settle the slide, torque the fasteners and test the pressurized loop and cooling trend.","ALIGN","FASTEN","COOL"),
                Page("04 / Recover the trusted wrench","W129  •  PROTECTED HOLD","Open W129 during loaded travel. Restore the wire, validate fresh contact force and reset. Complete a fresh full service run.","HOLD","RESET","QUALIFY"),

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

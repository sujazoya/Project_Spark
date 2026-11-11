#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel33DrawingAssetCreator
    {
        private static readonly Color Red = new Color(.98f,.34f,.40f);
        private static readonly Color Blue = new Color(.24f,.69f,1f);
        private static readonly Color Cyan = new Color(.24f,.87f,.80f);
        private static readonly Color Amber = new Color(.99f,.71f,.28f);

        [MenuItem("Tools/Project Spark/Level 33/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel33Builder.EnsureFolder();
            string path=SparkLevel33Builder.Folder+"/Level_33_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_33";
            drawing.levelTitle="Jam Guard - Delayed Trip and Manual Reset";
            drawing.pages=new[] {
                Page("01 / Charge the timer","JAM → 4.7 kΩ → TIMER +",
                    "W01 Battery_positive → JAM_IN. W02 JAM_OUT → R_CHARGE_IN. W03 R_CHARGE_OUT → Timer_Positive. W04 Timer_Negative → Battery_negative. JAM closes while blocked. The 470 µF capacitor rises gradually. Observe about 1.0 V after a 0.50 s blockage and about 3.0 V after 2 s.",
                    new[] { Dot("supply",.08f,.60f,"BATTERY +",Red), Part("jam",SparkSchematicKind.Switch,.26f,.60f,"JAM CONTACT",Red),
                        Part("r_charge",SparkSchematicKind.Resistor,.46f,.60f,"4.7 kΩ",Amber), Part("cap",SparkSchematicKind.Capacitor,.68f,.60f,"TIMER + / −",Cyan),
                        Dot("ground",.90f,.60f,"BATTERY −",Blue) },
                    Line(Red,.08f,.215f,.60f),Line(Red,.305f,.415f,.60f),Line(Cyan,.505f,.635f,.60f),Line(Blue,.725f,.90f,.60f)),
                Page("02 / Discharge when clear","TIMER + → 1 kΩ → CLEAR CONTACT → BATTERY −",
                    "W05 Timer_Positive → R_CLEAR_IN. W06 R_CLEAR_OUT → CLEAR_IN. W07 CLEAR_OUT → Battery_negative. Clearing opens JAM and closes CLEAR. TIMER falls through 1 kΩ. Clearing a latched jam leaves the conveyor stopped.",
                    new[] { Dot("timer",.10f,.60f,"TIMER +",Cyan), Part("r_clear",SparkSchematicKind.Resistor,.35f,.60f,"1 kΩ",Amber),
                        Part("clear",SparkSchematicKind.Switch,.61f,.60f,"CLEAR CONTACT",Cyan), Dot("ground",.88f,.60f,"BATTERY −",Blue) },
                    Line(Cyan,.10f,.305f,.60f),Line(Cyan,.395f,.565f,.60f),Line(Blue,.655f,.88f,.60f)),
                Page("03 / Wire the motor cutoff","3.0 V AT TIMER OPENS THE MOTOR CONTACT",
                    "W08 Battery_positive → RUN_IN. W09 RUN_OUT → Motor_A. W10 Motor_B → Battery_negative. The controller reads the solved capacitor voltage. At 3.0 V it opens the motor contact and holds it open. The timer has a separate supply path.",
                    new[] { Dot("supply",.09f,.60f,"BATTERY +",Red), Part("cutoff",SparkSchematicKind.Switch,.35f,.60f,"MOTOR CONTACT",Amber),
                        Part("motor",SparkSchematicKind.Motor,.61f,.60f,"MOTOR A / B",Amber),Dot("ground",.89f,.60f,"BATTERY −",Blue) },
                    Line(Red,.09f,.305f,.60f),Line(Amber,.395f,.577f,.60f),Line(Blue,.643f,.89f,.60f)),
                Page("04 / Verify trip and reset","BELOW 1.0 V AND CLEAR → MANUAL RESET",
                    "1/6 Start with TIMER below 0.25 V. 2/6 Brief blockage leaves the conveyor running. 3/6 Hold JAM until the conveyor stops. 4/6 Press RESET while blocked and observe that it is refused. 5/6 CLEAR JAM and wait below 1.0 V. The motor stays stopped. 6/6 Press RESET to restart.",
                    new[] { Dot("brief",.19f,.33f,"BRIEF ≈ 1.0 V",Cyan),Dot("trip",.49f,.72f,"TRIP AT 3.0 V",Red),
                        Dot("reset",.79f,.32f,"RESET BELOW 1.0 V",Blue) },
                    Wire(Cyan,P(.08f,.18f),P(.19f,.33f),P(.25f,.20f),P(.30f,.18f)),
                    Wire(Amber,P(.33f,.18f),P(.38f,.39f),P(.44f,.59f),P(.49f,.72f)),
                    Wire(Red,P(.49f,.72f),P(.65f,.72f)),Wire(Blue,P(.65f,.72f),P(.70f,.49f),P(.79f,.32f),P(.90f,.20f)))
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,SparkSchematicPart[] parts,params SparkSchematicWire[] wires) =>
            new SparkLevelDrawingPage { title=title,caption=caption,explanation=explanation,parts=parts,wires=wires };
        private static SparkSchematicPart Dot(string id,float x,float y,string label,Color color) =>
            Part(id,SparkSchematicKind.Generic,x,y,label,color);
        private static SparkSchematicPart Part(string id,SparkSchematicKind kind,float x,float y,string label,Color color) =>
            new SparkSchematicPart { id=id,kind=kind,position=P(x,y),label=label,color=color };
        private static SparkSchematicWire Line(Color color,float from,float to,float y) => Wire(color,P(from,y),P(to,y));
        private static SparkSchematicWire Wire(Color color,params Vector2[] points) => new SparkSchematicWire { color=color,points=points };
        private static Vector2 P(float x,float y) => new Vector2(x,y);
    }
}
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel29DrawingAssetCreator
    {
        private static readonly Color R=new Color(1,.3f,.4f), B=new Color(.2f,.65f,1), G=new Color(.3f,1,.6f), Y=new Color(1,.75f,.3f);
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Level 29 Drawing Asset")]
        public static void CreateAsset()
        {
            string path=EditorUtility.SaveFilePanelInProject("Automatic Backup Drawing","Level_29_Automatic_Backup_Drawing","asset","Create the approved Level 29 drawing.");
            if(string.IsNullOrEmpty(path)) return;
            if(AssetDatabase.LoadAssetAtPath<Object>(path)!=null) { EditorUtility.DisplayDialog("Preserve existing asset","Choose a new filename.","OK"); return; }
            var a=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            a.levelId="level_29"; a.levelTitle="Automatic Emergency Backup"; a.pages=new SparkLevelDrawingPage[4];
            for(int i=0;i<4;i++) a.pages[i]=Page(i);
            AssetDatabase.CreateAsset(a,path); EditorUtility.SetDirty(a); AssetDatabase.SaveAssets(); Selection.activeObject=a;
        }
        private static Vector2 P(float x,float y)=>new Vector2(x,y);
        private static SparkSchematicPart N(string id,float x,float y,string label,Color c)=>new SparkSchematicPart {id=id,kind=SparkSchematicKind.Generic,position=P(x,y),label=label,color=c};
        private static SparkSchematicWire W(Color c,params Vector2[] p)=>new SparkSchematicWire {color=c,points=p,energized=false};
        private static SparkLevelDrawingPage Page(int i)
        {
            // All symbol and wire geometry is normalized so terminals remain aligned on resize.
            var parts=new[]{
                N("src",.06f,.84f,"+5 V",R), N("sw",.20f,.84f,"MAIN switch",R),
                N("dm",.40f,.84f,"S01 D_MAIN A > K",Y), N("bus",.73f,.84f,"LOAD BUS",G),
                N("dc",.28f,.59f,"S02 D_CHARGE A > K",Y), N("rc",.46f,.59f,"S03 220 ohm",Y),
                N("cp",.64f,.42f,"S04 C+ 10000 uF",G), N("cn",.64f,.15f,"C-",B),
                N("db",.73f,.54f,"S06 BACKUP A ^ K",Y),
                N("rl",.88f,.74f,"S05 330 ohm",Y),N("led",.88f,.47f,"LED A v K",G),
                N("neg",.06f,.15f,"BATTERY -",B)
            };
            var wires=new[]{
                W(R,P(.06f,.84f),P(.20f,.84f),P(.40f,.84f)),
                W(R,P(.50f,.84f),P(.73f,.84f),P(.88f,.84f),P(.88f,.74f)),
                W(Y,P(.20f,.84f),P(.20f,.59f),P(.28f,.59f)),
                W(Y,P(.38f,.59f),P(.46f,.59f)),
                W(Y,P(.56f,.59f),P(.64f,.59f),P(.64f,.42f)),
                W(G,P(.64f,.42f),P(.73f,.42f),P(.73f,.54f)),
                W(G,P(.73f,.64f),P(.73f,.84f)),
                W(G,P(.88f,.64f),P(.88f,.47f)),
                W(B,P(.88f,.37f),P(.88f,.15f),P(.06f,.15f)),
                // Two horizontal diode symbols (triangle to cathode bar).
                W(Y,P(.40f,.84f),P(.43f,.84f)),W(Y,P(.43f,.86f),P(.43f,.82f),P(.47f,.84f),P(.43f,.86f)),
                W(Y,P(.47f,.865f),P(.47f,.815f)),W(Y,P(.47f,.84f),P(.50f,.84f)),
                W(Y,P(.28f,.59f),P(.31f,.59f)),W(Y,P(.31f,.61f),P(.31f,.57f),P(.35f,.59f),P(.31f,.61f)),
                W(Y,P(.35f,.615f),P(.35f,.565f)),W(Y,P(.35f,.59f),P(.38f,.59f)),
                // Charging resistor.
                W(Y,P(.46f,.59f),P(.475f,.59f),P(.485f,.61f),P(.50f,.57f),P(.515f,.61f),P(.53f,.57f),P(.545f,.59f),P(.56f,.59f)),
                // Capacitor: separate plates, no bypass line through the symbol.
                W(G,P(.64f,.42f),P(.64f,.32f)),W(G,P(.61f,.32f),P(.67f,.32f)),
                W(G,P(.61f,.30f),P(.67f,.30f)),W(B,P(.64f,.30f),P(.64f,.15f)),
                // Backup diode upward (anode below, cathode above).
                W(Y,P(.73f,.54f),P(.73f,.57f)),W(Y,P(.71f,.57f),P(.75f,.57f),P(.73f,.61f),P(.71f,.57f)),
                W(Y,P(.705f,.61f),P(.755f,.61f)),W(Y,P(.73f,.61f),P(.73f,.64f)),
                W(Y,P(.88f,.74f),P(.88f,.73f),P(.90f,.715f),P(.86f,.70f),P(.90f,.685f),P(.86f,.67f),P(.88f,.655f),P(.88f,.64f)),
                // LED downward to negative.
                W(G,P(.88f,.47f),P(.88f,.45f)),W(G,P(.86f,.45f),P(.90f,.45f),P(.88f,.40f),P(.86f,.45f)),
                W(G,P(.855f,.40f),P(.905f,.40f)),W(G,P(.88f,.40f),P(.88f,.37f))
            };
            string[] titles={"Functional Path Map","Charge / Normal Power","Automatic Transfer","Restore and Complete"};
            string[] explanations={
                "MAIN is represented by its labelled feed point; use the actual main switch in the scene. Triangles point toward cathode bars. Capacitor plates are separated. Use Level29_Exact_Wiring.csv for all 11 terminal pairs. Three diode cathodes face away from their supplying source. No SPDT or bleed resistor remains.",
                "MAIN ON. Battery -> D_MAIN -> R_LED -> LED powers the load. Separate branch: MAIN -> D_CHARGE -> 220 ohm -> capacitor positive. Capacitor negative and LED cathode return to negative. Observe at least 0.30 V capacitor rise and reach 4.10 V before switching MAIN OFF.",
                "MAIN OFF. Capacitor positive -> D_BACKUP -> R_LED -> LED supplies automatic backup. D_MAIN and D_CHARGE block reverse current toward the main supply. Hold LED current at least 2 mA for 2 accepted simulated seconds while capacitor voltage falls by at least 0.20 V. Main/source/charge path leakage must remain <=0.10 mA.",
                "Restore MAIN after BACKUP PASSED. Observe resumed charging (>0.20 mA), capacitor recovery by >=0.15 V and back to >=4.10 V. Hold normal operation 0.5 simulated seconds. Six targets are phase gates published by SparkLevel29AutomaticBackup using the real LED. No observer means no completion. Full phase sequence is required."
            };
            return new SparkLevelDrawingPage {title="LEVEL 29 | "+titles[i],caption="PAGE "+(i+1)+"/4 | AUTOMATIC EMERGENCY BACKUP",explanation=explanations[i],parts=parts,wires=wires};
        }
    }
}
#endif

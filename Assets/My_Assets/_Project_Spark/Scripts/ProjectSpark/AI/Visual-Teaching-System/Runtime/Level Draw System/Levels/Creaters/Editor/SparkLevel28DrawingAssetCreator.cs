#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>Creates four VISUAL-ONLY Level 28 teaching pages. Does not make gameplay connections.</summary>
    public static class SparkLevel28DrawingAssetCreator
    {
        private const string Menu = "Tools/Project Spark/Visual Teaching/Create Level 28 Drawing Asset";
        private static readonly Color Red = new Color(.98f,.33f,.40f,1);
        private static readonly Color Blue = new Color(.22f,.66f,1f,1);
        private static readonly Color Gold = new Color(1f,.75f,.33f,1);
        private static readonly Color Green = new Color(.35f,.95f,.52f,1);
        private static readonly Color Cyan = new Color(.32f,.86f,.94f,1);

        [MenuItem(Menu)]
        public static void CreateAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Level 28 Drawing", "Level_28_Drawing", "asset",
                "Create four-page Level 28 visual teaching asset.");
            if (string.IsNullOrEmpty(path)) return;
            var asset = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(path);
            if (asset != null)
            {
                if (!EditorUtility.DisplayDialog("Replace drawing pages?",
                    "Replace existing Level 28 visual pages in place?", "Replace", "Cancel")) return;
                Undo.RecordObject(asset, "Update Level 28 drawing");
            }
            else
            {
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                {
                    EditorUtility.DisplayDialog("Level 28", "Path already contains another asset type.", "OK");
                    return;
                }
                asset = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.levelId = "level_28";
            asset.levelTitle = "Emergency Power Backup - Charge / Backup Selector";
            asset.initialPage = 0;
            asset.pages = new SparkLevelDrawingPage[4];
            for (int i=0;i<4;i++) asset.pages[i] = BuildPage(i);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            Debug.Log("[Level 28 Drawing] Four schematic pages saved: " + path);
        }
        private static Vector2 P(float x,float y) => new Vector2(x,y);
        private static SparkSchematicKind Kind(string name)
        {
            return Enum.TryParse(name,true,out SparkSchematicKind k) &&
                Enum.IsDefined(typeof(SparkSchematicKind),k) ? k : SparkSchematicKind.Generic;
        }
        private static SparkSchematicPart Part(string id,SparkSchematicKind kind,float x,float y,string label,Color color) =>
            new SparkSchematicPart { id=id,kind=kind,position=P(x,y),label=label,color=color };
        private static SparkSchematicWire Wire(Color c,params Vector2[] pts) =>
            new SparkSchematicWire { color=c,energized=false,points=pts };
        private static SparkLevelDrawingPage BuildPage(int index)
        {
            var parts = new[] {
                Part("battery",SparkSchematicKind.Battery,.09f,.55f,"5 V DC",Red),
                Part("main",SparkSchematicKind.Switch,.21f,.62f,"MAIN",Cyan),
                Part("d_protect",Kind("Diode"),.33f,.70f,"S01 1N5819",Gold),
                Part("r_charge",SparkSchematicKind.Resistor,.46f,.70f,"S02 220 OHM",Gold),
                Part("selector",Kind("Switch"),.61f,.67f,"S06 SPDT A / COM / B",Cyan),
                Part("cap",Kind("Capacitor"),.61f,.32f,"S03 4700 uF + / -",Cyan),
                Part("r_led",SparkSchematicKind.Resistor,.80f,.77f,"S04 330 OHM",Gold),
                Part("led",Kind("LED"),.92f,.77f,"GREEN LED",Green),
                Part("r_bleed",SparkSchematicKind.Resistor,.80f,.40f,"S05 1k OHM",Gold),
                Part("gnd",SparkSchematicKind.Ground,.50f,.14f,"BATTERY NEGATIVE",Blue),
            };
            // Charging path: Battery+ -> main -> diode -> 220 -> SPDT_A.
            // COM -> capacitor+. Backup B branches to 330/LED and 1k resistor.
            // All three return lines join the battery negative net.
            var wires = new[] {
                Wire(Red,P(.13f,.60f),P(.17f,.62f)),   // 1
                Wire(Red,P(.25f,.62f),P(.29f,.70f)),   // 2
                Wire(Red,P(.37f,.70f),P(.42f,.70f)),   // 3
                Wire(Red,P(.50f,.70f),P(.555f,.70f)), // 4 -> A
                Wire(Gold,P(.61f,.58f),P(.61f,.39f)), // 5 COM -> C+
                Wire(Blue,P(.61f,.25f),P(.61f,.14f)), // 6 cap- return
                Wire(Gold,P(.67f,.70f),P(.75f,.77f)), // 7 B -> 330 in
                Wire(Gold,P(.84f,.77f),P(.88f,.77f)), // 8 330 -> LED anode
                Wire(Blue,P(.95f,.70f),P(.95f,.14f)), // 9 LED cathode -> return
                Wire(Gold,P(.67f,.70f),P(.72f,.70f),P(.72f,.40f),P(.75f,.40f)), // 10 B -> 1k
                Wire(Blue,P(.84f,.40f),P(.84f,.14f)), // 11 1k -> return
                Wire(Blue,P(.09f,.48f),P(.09f,.14f),P(.95f,.14f)), // common ground visual
            };
            string title,caption,explanation;
            switch(index)
            {
                case 0:
                    title="LEVEL 28 | Emergency Power Backup";
                    caption="PAGE 1/4 | SIX SLOTS | ELEVEN CONNECTIONS";
                    explanation="Slot_01 Schottky 1N5819; Slot_02 220 Ohm; Slot_03 4700 uF capacitor; Slot_04 330 Ohm; Slot_05 1k bleed; Slot_06 SPDT. Fixed 5V, MAIN switch, green LED. SPDT A charge, COM capacitor+, B backup loads. Wire IDs in Level28_Exact_Wiring.csv.";
                    break;
                case 1:
                    title="LEVEL 28 | Charge the Capacitor";
                    caption="PAGE 2/4 | MAIN ON | A-to-COM CLOSED";
                    explanation="5V -> MAIN -> 1N5819 anode/cathode -> 220 Ohm -> SPDT_A -> COM -> capacitor positive. Capacitor negative returns to Battery_negative. SPDT_B is open; the 330 Ohm LED load and 1k bleed are isolated. Charge to at least approximately 4.4V, model dependent.";
                    break;
                case 2:
                    title="LEVEL 28 | Emergency Backup";
                    caption="PAGE 3/4 | MAIN OFF | COM-to-B CLOSED";
                    explanation="Switch main supply OFF first. Then move SPDT to BACKUP. SPDT_COM connects charged capacitor+ to SPDT_B. B feeds 330 Ohm -> LED anode -> LED cathode -> battery negative, and 1k Ohm -> battery negative in parallel. The capacitor supplies both loads and discharges.";
                    break;
                default:
                    title="LEVEL 28 | Six Phase-Based Goals";
                    caption="PAGE 4/4 | CHARGE -> ISOLATE -> BACKUP";
                    explanation="Validate all 11 wires and six slots; capacitor voltage reaches >=4.4V in charge; holds >=4.2V briefly while isolated; backup LED turns ON when COM-B selected; LED stays ON >=0.35 s; capacitor falls by >=1.0V and LED eventually OFF. Temporal objectives require a level-specific phase evaluator, not simultaneous static targets. Exact thresholds must be tuned in Play Mode.";
                    break;
            }
            return new SparkLevelDrawingPage { title=title,caption=caption,explanation=explanation,parts=parts,wires=wires };
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Native, four-page authored drawing asset creator for Levels 10-20.
    /// Uses exactly the same public asset/page/part/wire API as
    /// SparkLevel0708DrawingAssetCreator and SparkLevel09DrawingAssetCreator.
    /// No MonoBehaviours, scene changes, or changes to electrical evaluation.
    /// </summary>
    public static class SparkLevel10To20DrawingAssetCreator
    {
        private static readonly Color Supply = new Color(.99f, .30f, .40f, 1f);
        private static readonly Color Ground = new Color(.21f, .70f, 1f, 1f);
        private static readonly Color Gold = new Color(1f, .79f, .32f, 1f);
        private static readonly Color Junction = new Color(.25f, .99f, .77f, 1f);
        private static readonly Color Cyan = new Color(.28f, .95f, .98f, 1f);
        private static readonly Color Sense = new Color(.78f, .53f, .97f, 1f);

        private static Vector2 P(float x, float y) => new Vector2(x, y);
        private static SparkSchematicPart Part(string id, SparkSchematicKind kind,
            float x, float y, string label, Color color) =>
            new SparkSchematicPart { id = id, kind = kind, position = P(x, y), label = label, color = color };
        private static SparkSchematicWire Wire(Color color, params Vector2[] points) =>
            new SparkSchematicWire { color = color, energized = false, points = points };

        private sealed class Drawing
        {
            public readonly List<SparkSchematicPart> Parts = new List<SparkSchematicPart>();
            public readonly List<SparkSchematicWire> Wires = new List<SparkSchematicWire>();
            public Drawing Add(string id, SparkSchematicKind kind, float x, float y, string label, Color color)
            { Parts.Add(Part(id,kind,x,y,label,color)); return this; }
            public Drawing Connect(Color color, params Vector2[] points)
            { Wires.Add(Wire(color, points)); return this; }
        }

        private sealed class Lesson
        {
            public int Number;
            public string Name;
            public Drawing Circuit;
            public string[] Headings;
            public string[] Captions;
            public string[] Explanations;
        }

        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawings / Levels 10-20 (All)")]
        public static void CreateAll()
        {
            string folder = EditorUtility.OpenFolderPanel("Choose folder for native drawing assets", "Assets", "");
            if (string.IsNullOrEmpty(folder)) return;
            folder = folder.Replace('\\','/');
            string root = Application.dataPath.Replace('\\','/');
            if (!folder.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("Project Spark", "Choose a folder INSIDE this project's Assets directory.", "OK");
                return;
            }
            string assetFolder = "Assets" + folder.Substring(root.Length);
            int created = 0, skipped = 0;
            for (int level = 10; level <= 20; level++)
            {
                string target = assetFolder + "/Level_" + level.ToString("00") + "_Drawing.asset";
                if (CreateOne(level, target)) created++; else skipped++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project Spark", "Created " + created + " drawing assets. Skipped " + skipped + " existing assets (never overwritten).", "OK");
        }

        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawing / Level 10")]
        public static void Create10() => Prompt(10);
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawing / Level 11")]
        public static void Create11() => Prompt(11);
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawing / Level 12")]
        public static void Create12() => Prompt(12);
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawing / Level 13")]
        public static void Create13() => Prompt(13);
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawing / Level 14")]
        public static void Create14() => Prompt(14);
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawing / Level 15")]
        public static void Create15() => Prompt(15);
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawing / Level 16")]
        public static void Create16() => Prompt(16);
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawing / Level 17")]
        public static void Create17() => Prompt(17);
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawing / Level 18")]
        public static void Create18() => Prompt(18);
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawing / Level 19")]
        public static void Create19() => Prompt(19);
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Drawing / Level 20")]
        public static void Create20() => Prompt(20);

        private static void Prompt(int level)
        {
            string id = "level_" + level.ToString("00");
            string path = EditorUtility.SaveFilePanelInProject("Create " + id + " drawing", "Level_" + level.ToString("00") + "_Drawing", "asset", "Choose a destination. Existing assets will never be overwritten.");
            if (string.IsNullOrEmpty(path)) return;
            if (CreateOne(level,path)) AssetDatabase.SaveAssets();
        }

        private static bool CreateOne(int level, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            {
                Debug.LogWarning("[Spark Level Drawing] Asset already exists; skipped: " + path);
                return false;
            }
            Lesson lesson = BuildLesson(level);
            if (lesson == null) return false;
            var asset = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            asset.levelId = "level_" + level.ToString("00");
            asset.levelTitle = lesson.Name;
            asset.initialPage = 0;
            asset.pages = new SparkLevelDrawingPage[4];
            for (int i=0; i<4; i++)
            {
                asset.pages[i] = new SparkLevelDrawingPage
                {
                    title = "LEVEL " + level + " | " + lesson.Headings[i],
                    caption = "PAGE " + (i + 1) + "/4 | " + lesson.Captions[i],
                    explanation = lesson.Explanations[i],
                    parts = lesson.Circuit.Parts.ToArray(),
                    wires = lesson.Circuit.Wires.ToArray()
                };
            }
            AssetDatabase.CreateAsset(asset,path);
            EditorUtility.SetDirty(asset);
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            Debug.Log("[Spark Level Drawing] Created native four-page asset for " + asset.levelId + " at " + path);
            return true;
        }

        private static Lesson L(int n, string name, Drawing d, string overview, string wiring,
            string theory, string solution)
        {
            return new Lesson
            {
                Number=n, Name=name, Circuit=d,
                Headings=new[]{"Circuit Overview", "Wiring Guide", "Electrical Concept", "Correct Solution"},
                Captions=new[]{"CIRCUIT TOPOLOGY", "STEP-BY-STEP CONNECTIONS", "NOMINAL ELECTRICAL RESULTS", "CHECK & VERIFY"},
                Explanations=new[]{overview,wiring,theory,solution}
            };
        }

        // Shared 5-V switch and return bus; positive rail is at y=0.82, ground y=0.15.
        private static Drawing Base()
        {
            return new Drawing()
                .Add("battery",SparkSchematicKind.Battery,.10f,.50f,"5 V DC",Supply)
                .Add("switch",SparkSchematicKind.Switch,.27f,.82f,"SW1",Gold)
                .Add("ground",SparkSchematicKind.Ground,.60f,.15f,"GND",Ground)
                .Connect(Supply,P(.10f,.64f),P(.10f,.82f),P(.21f,.82f))
                .Connect(Supply,P(.33f,.82f),P(.43f,.82f))
                .Connect(Ground,P(.84f,.15f),P(.10f,.15f),P(.10f,.36f));
        }

        // Horizontal resistor icons. Voltage and current are conceptual, not live solver data.
        private static Drawing Parallel(string[] labels, bool[] hasLed = null)
        {
            Drawing d = Base();
            float[] rows = labels.Length==2 ? new[]{.65f,.39f} : new[]{.69f,.46f,.25f};
            d.Connect(Supply,P(.43f,.82f),P(.43f,rows[rows.Length-1]));
            d.Connect(Ground,P(.84f,.15f),P(.84f,rows[0]));
            for(int i=0;i<labels.Length;i++)
            {
                float y=rows[i];
                bool led=hasLed!=null && hasLed.Length>i && hasLed[i];
                d.Add("R"+(i+1),SparkSchematicKind.Resistor,led ? .58f : .64f,y,labels[i],Gold);
                d.Connect(Supply,P(.43f,y),P(led ? .52f : .58f,y));
                if(led)
                {
                    d.Add("LED"+(i+1),SparkSchematicKind.Generic,.76f,y,i==0?"RED LED":"GREEN LED",i==0?Supply:Junction);
                    d.Connect(Supply,P(.64f,y),P(.70f,y));
                    d.Connect(Ground,P(.82f,y),P(.84f,y));
                }
                else d.Connect(Ground,P(.70f,y),P(.84f,y));
            }
            return d;
        }

        private static Drawing SeriesChain(string[] labels)
        {
            Drawing d=Base();
            float[] xs = labels.Length==2?new[]{.52f,.72f}:new[]{.49f,.65f,.81f};
            for(int i=0;i<labels.Length;i++)
                d.Add("S"+(i+1),labels[i].Contains("LED")?SparkSchematicKind.Generic:SparkSchematicKind.Resistor,
                    xs[i],.82f,labels[i],labels[i].Contains("GREEN")?Junction:Gold);
            float left=.43f;
            foreach(float x in xs) { d.Connect(Supply,P(left,.82f),P(x-.06f,.82f));left=x+.06f; }
            d.Connect(Ground,P(left,.82f),P(.84f,.82f),P(.84f,.15f));
            return d;
        }

        // Top series resistor, then independent parallel resistor/LED branches.
        private static Drawing SeriesParallel(string series, string[] branches, bool branchLeds=false)
        {
            Drawing d=Base();
            d.Add("series",SparkSchematicKind.Resistor,.52f,.82f,series,Gold)
             .Add("junction",SparkSchematicKind.Generic,.66f,.82f,"VOUT",Junction)
             .Connect(Supply,P(.43f,.82f),P(.46f,.82f))
             .Connect(Supply,P(.58f,.82f),P(.66f,.82f))
             .Connect(Junction,P(.66f,.82f),P(.66f,.39f))
             .Connect(Ground,P(.86f,.15f),P(.86f,.66f));
            float[] ys={.66f,.39f};
            for(int i=0;i<2;i++)
            {
                float y=ys[i];
                d.Add("branch"+i,SparkSchematicKind.Resistor,.74f,y,branches[i],Gold);
                d.Connect(Junction,P(.66f,y),P(.68f,y));
                if(branchLeds)
                {
                    // LED in each branch is represented as a separate symbol before the return bus.
                    d.Add("led"+i,SparkSchematicKind.Generic,.86f,y,i==0?"RED LED":"GREEN LED",i==0?Supply:Junction);
                    d.Connect(Supply,P(.80f,y),P(.81f,y));
                    d.Connect(Ground,P(.91f,y),P(.94f,y),P(.94f,.15f),P(.86f,.15f));
                }
                else d.Connect(Ground,P(.80f,y),P(.86f,y));
            }
            return d;
        }

        // Level 12: Branch A consists of R1; Branch B has R2 then R3 in series.
        private static Drawing MixedBranches()
        {
            Drawing d = Base();
            d.Add("A",SparkSchematicKind.Resistor,.56f,.64f,"R1 330 Ohm",Gold)
             .Add("B1",SparkSchematicKind.Resistor,.56f,.39f,"R2 220 Ohm",Gold)
             .Add("B2",SparkSchematicKind.Resistor,.75f,.39f,"R3 470 Ohm",Gold)
             .Add("mid",SparkSchematicKind.Generic,.65f,.39f,"MID B",Junction)
             .Add("lcd",SparkSchematicKind.Meter,.76f,.22f,"LCD across R3",Cyan)
             .Connect(Supply,P(.43f,.82f),P(.43f,.39f))
             .Connect(Supply,P(.43f,.64f),P(.50f,.64f))
             .Connect(Supply,P(.43f,.39f),P(.50f,.39f))
             .Connect(Junction,P(.62f,.39f),P(.69f,.39f))
             .Connect(Ground,P(.62f,.64f),P(.89f,.64f),P(.89f,.15f),P(.84f,.15f))
             .Connect(Ground,P(.81f,.39f),P(.89f,.39f))
             .Connect(Sense,P(.65f,.39f),P(.65f,.28f),P(.70f,.28f))
             .Connect(Sense,P(.82f,.22f),P(.89f,.22f));
            return d;
        }

        private static Drawing Divider(bool loaded)
        {
            Drawing d=Base();
            d.Add("top",SparkSchematicKind.Resistor,.53f,.82f,"R_TOP 330 Ohm",Gold)
             .Add("vout",SparkSchematicKind.Generic,.68f,.82f,"VOUT",Junction)
             .Add("bottom",SparkSchematicKind.Resistor,.75f,.52f,"R_BOTTOM 470 Ohm",Gold)
             .Add("lcd",SparkSchematicKind.Meter,.50f,.32f,"LCD A/B",Cyan)
             .Connect(Supply,P(.43f,.82f),P(.47f,.82f))
             .Connect(Supply,P(.59f,.82f),P(.68f,.82f))
             .Connect(Junction,P(.68f,.82f),P(.68f,.52f),P(.69f,.52f))
             .Connect(Ground,P(.81f,.52f),P(.84f,.52f),P(.84f,.15f))
             .Connect(Sense,P(.68f,.82f),P(.60f,.82f),P(.60f,.38f),P(.50f,.38f))
             .Connect(Sense,P(.50f,.26f),P(.50f,.15f));
            if(loaded)
            {
                d.Add("load",SparkSchematicKind.Resistor,.75f,.30f,"R_LOAD 1 kOhm",Gold)
                 .Connect(Junction,P(.68f,.52f),P(.68f,.30f),P(.69f,.30f))
                 .Connect(Ground,P(.81f,.30f),P(.84f,.30f));
            }
            return d;
        }

        // Two separate voltage divider branches between the switched rail and GND.
        // Measurement between their midpoints is high-impedance only.
        private static Drawing Bridge(string leftTop="330 Ohm", string leftBottom="470 Ohm", string rightTop="330 Ohm", string rightBottom="470 Ohm")
        {
            Drawing d=Base();
            d.Add("Ltop",SparkSchematicKind.Resistor,.56f,.70f,"L TOP " + leftTop,Gold)
             .Add("Lbottom",SparkSchematicKind.Resistor,.56f,.37f,"L BOTTOM " + leftBottom,Gold)
             .Add("Rtop",SparkSchematicKind.Resistor,.78f,.70f,"R TOP " + rightTop,Gold)
             .Add("Rbottom",SparkSchematicKind.Resistor,.78f,.37f,"R BOTTOM " + rightBottom,Gold)
             .Add("A",SparkSchematicKind.Generic,.64f,.54f,"MID A",Junction)
             .Add("B",SparkSchematicKind.Generic,.88f,.54f,"MID B",Junction)
             .Add("lcd",SparkSchematicKind.Meter,.76f,.23f,"LCD A - B",Cyan)
             .Connect(Supply,P(.43f,.82f),P(.50f,.82f),P(.50f,.70f))
             .Connect(Supply,P(.43f,.82f),P(.72f,.82f),P(.72f,.70f))
             .Connect(Junction,P(.62f,.70f),P(.64f,.70f),P(.64f,.54f))
             .Connect(Junction,P(.64f,.54f),P(.50f,.54f),P(.50f,.37f))
             .Connect(Junction,P(.84f,.70f),P(.88f,.70f),P(.88f,.54f))
             .Connect(Junction,P(.88f,.54f),P(.72f,.54f),P(.72f,.37f))
             .Connect(Ground,P(.62f,.37f),P(.64f,.37f),P(.64f,.15f))
             .Connect(Ground,P(.84f,.37f),P(.88f,.37f),P(.88f,.15f))
             .Connect(Sense,P(.64f,.54f),P(.68f,.54f),P(.68f,.29f),P(.70f,.29f))
             .Connect(Sense,P(.88f,.54f),P(.94f,.54f),P(.94f,.23f),P(.82f,.23f));
            return d;
        }

        private static Lesson BuildLesson(int n)
        {
            switch(n)
            {
                case 10: return L(n,"Bridge Midpoint Comparison", Bridge("220 Ohm","330 Ohm","100 Ohm","150 Ohm"),
                    "Two 5-V voltage dividers share the same switched positive bus and ground. LEFT: upper 220 Ohm, lower 330 Ohm. RIGHT: upper 100 Ohm, lower 150 Ohm. LCD senses MID A minus MID B; no conductive cross-link.",
                    "Battery+ -> switch; switch output -> upper LEFT and RIGHT resistors; each upper resistor -> its own midpoint; each midpoint -> corresponding lower resistor; both lowers -> Battery_negative. LCD A -> MID A; LCD B -> MID B. Do not use LCD as a return wire.",
                    "Nominal: LEFT midpoint = 5*330/(220+330)=3.00 V; RIGHT midpoint = 5*150/(100+150)=3.00 V; differential approximately 0 V. The diagram labels show the Level 10 source configuration, not the solved Level 20 resistor values.",
                    "Validate both energized branches and a near-zero differential on the LCD. Check the actual Level 10 target thresholds in Level_10.asset; drawing never awards victory.");
                case 11: return L(n,"Three Parallel Loads and Fault",Parallel(new[]{"R1 330 Ohm","R2 220 Ohm FAULT","R3 470 Ohm"}),
                    "Three resistive loads connect independently from the switched +5 V rail to GND. The middle 220-Ohm resistor is marked as faulty in the authored reference.",
                    "Battery+ -> switch -> positive distribution node. Connect EACH resistor input to that node and EACH output to common Battery_negative. Never place the three resistors in series.",
                    "In parallel, every branch sees approximately 5 V; total source current is the sum of the branch currents. Fault behavior depends on your configured resistor burn/repair rules.",
                    "Repair or replace the faulty middle branch as required by Level_11.asset; verify all intended branches conduct. Do not interpret the separate source-current audit's historical Level 11 example as authoritative for every level.");
                case 12: return L(n,"Mixed Series and Parallel Branches",MixedBranches(),
                    "Reference topology: one 330-Ohm branch in parallel with a second branch containing 220 and 470 Ohms in SERIES. The schematic is conceptual; text defines the exact series link.",
                    "SwitchOutput splits: Branch A -> R1 330 Ohm -> Battery_negative. Branch B -> R2 220 Ohm -> R3 470 Ohm -> Battery_negative. The LCD senses voltage across R3 with probe A at R2/R3 midpoint and B at ground.",
                    "Nominal with ideal 5 V: Branch A = 15.15 mA, Branch B = 5/690 = 7.25 mA, R3 drop = 3.406 V. Source current approximately 22.40 mA.",
                    "Confirm R2 and R3 are electrically in series on Branch B. Verify all Level 12 completion targets against the level definition asset.");
                case 13: return L(n,"Series Resistor and Parallel Loads",SeriesParallel("R1 220 Ohm",new[]{"R2 330 Ohm","R3 470 Ohm"}),
                    "Battery+ -> switch -> R1 220 Ohm -> VOUT node -> (R2 330 Ohm || R3 470 Ohm) -> ground. LCD voltage senses VOUT/GND.",
                    "SwitchOutput -> R1 input; R1 output -> both R2 and R3 inputs; both lower outputs -> Battery_negative; LCD A -> VOUT and LCD B -> Battery_negative.",
                    "Nominal Rparallel = 193.875 Ohm; source current ~12.081 mA; VOUT ~2.342 V; R2 ~7.098 mA; R3 ~4.983 mA.",
                    "Confirm actual conducting network and the Level 13 configured voltage/current ranges, not only the LCD reading.");
                case 14: return L(n,"Resistor-LED Branch and Resistor Branch",Parallel(new[]{"R1 + LED","R2 470 Ohm"},new[]{true,false}),
                    "The reference game scene shows a red LED in series with a resistor on the left branch and a separate 470-Ohm resistor on the right branch. Left resistor value is not confirmed by the reference.",
                    "SwitchOutput feeds both branches. LEFT: resistor -> red LED anode -> LED cathode -> Battery_negative. RIGHT: 470-Ohm resistor -> Battery_negative. Common return shared only at ground.",
                    "Both branches are in parallel across the 5-V supply. LED branch current depends on the forward drop and the series resistor; the right resistor current is nominally 5/470 = 10.64 mA.",
                    "Ensure LED polarity and series limiting resistor are correct. Read the precise resistor and target values from Level_14.asset before final course publishing.");
                case 15: return L(n,"Two Parallel LED Branches",Parallel(new[]{"R1 330 Ohm","R2 470 Ohm"},new[]{true,true}),
                    "Two independent branches: R1 330 Ohm + RED LED, R2 470 Ohm + GREEN LED. Both are fed by the same switched +5-V rail.",
                    "SwitchOutput -> R1 input and R2 input; R1 output -> red LED anode; R2 output -> green LED anode; both LED cathodes -> Battery_negative.",
                    "Each resistor limits only its own LED branch. Idealized branch currents approximately (5 - Vforward_LED)/R, not a simple 5/R because LED forward voltage matters.",
                    "Require both LEDs to conduct at sensible currents and confirm the level-specific thresholds from Level_15.asset.");
                case 16: return L(n,"Two LEDs in Series",SeriesChain(new[]{"R_LIMIT 100 Ohm","RED LED","GREEN LED"}),
                    "Single series path through a 100-Ohm current limiter, a red LED, and a green LED. Both diodes must have forward polarity.",
                    "Battery+ -> switch -> 100-Ohm resistor -> RED LED anode -> RED cathode to GREEN anode -> GREEN cathode -> Battery_negative. No bypass wires around either LED.",
                    "The same current must flow through all three series components. 5-V supply must exceed both LED forward drops; resistor drop is the remaining voltage.",
                    "Both LEDs should conduct and level-specific resistor/current conditions must pass. Use the solver values rather than an assumed fixed LED drop.");
                case 17: return L(n,"Series-Parallel LED Network",SeriesParallel("R_SERIES 100 Ohm",new[]{"R_RED 330 Ohm","R_GREEN 470 Ohm"},true),
                    "5-V source -> switch -> 100-Ohm COMMON resistor -> two parallel resistor-plus-LED branches, red and green.",
                    "SwitchOutput -> 100-Ohm resistor -> split node. Split -> R_RED 330 -> red LED anode; split -> R_GREEN 470 -> green LED anode. Both cathodes -> Battery_negative.",
                    "Project Spark previously solved the two-branch circuit near RED 5.983 mA, GREEN 4.204 mA, source 10.187 mA (nominal example, not a live reading).",
                    "Install 100 Ohm in Register_Slot_2; 330 Ohm in RED and 470 Ohm in GREEN. Confirm both LEDs conducting and all five targets from Level_17.asset.");
                case 18: return L(n,"Precision Voltage Divider",Divider(false),
                    "Two resistors in series: 330 Ohm TOP and 470 Ohm BOTTOM; VOUT is between them. LCD reads VOUT to supply negative.",
                    "Battery+ -> switch -> R_TOP 330 -> VOUT -> R_BOTTOM 470 -> Battery_negative. LCD A -> VOUT, LCD B -> negative; meter is high impedance.",
                    "Nominal VOUT = 5*(470/800)=2.9375 V. Current through both resistors = 5/800=6.25 mA. The tested Spark solver yielded about 2.937 V and 6.249 mA.",
                    "Correct binding IDs R_TOP_Component and R_BOTTOM_Component; level completes at five targets when the runtime workstation bridge resolves both components and terminals.");
                case 19: return L(n,"The Loading Effect",Divider(true),
                    "Start from Level 18's divider and add a 1-kOhm LOAD in parallel with R_BOTTOM (470 Ohm). VOUT decreases under load.",
                    "Battery+ -> switch -> R_TOP 330 -> VOUT; from VOUT -> R_BOTTOM 470 -> negative AND VOUT -> R_LOAD 1 kOhm -> negative; LCD A -> VOUT, B -> negative.",
                    "Solved Project Spark example: VOUT ~2.460 V; TOP and source ~7.694 mA; BOTTOM ~5.234 mA; LOAD ~2.460 mA. TOP current = BOTTOM + LOAD currents.",
                    "Ensure THREE unique slot IDs and runtime bindings: R_TOP, R_BOTTOM, R_LOAD; all five targets must pass. Do not assign the LOAD resistor the BOTTOM workstation SlotId.");
                case 20: return L(n,"Wheatstone Bridge Balance",Bridge(),
                    "Two 330/470-Ohm voltage dividers run in parallel across 5 V. Measure the difference between MID A and MID B with high-impedance LCD leads.",
                    "SwitchOutput feeds L_TOP 330 and R_TOP 330. L_TOP -> MID A -> L_BOTTOM 470 -> negative. R_TOP -> MID B -> R_BOTTOM 470 -> negative. LCD A -> MID A, LCD B -> MID B. Do not hard-wire the midpoints together.",
                    "At balance: MID A ~2.9375 V, MID B ~2.9375 V, difference ~0 V, each branch ~6.25 mA and source ~12.50 mA. Start R_TOP at 220 Ohm for the repair challenge, replace with 330 Ohm.",
                    "Use four unique slots and correct runtime declarations R_LTOP, R_LBOTTOM, R_RTOP, R_RBOTTOM. Check voltage difference 0-0.05 V, midpoint voltage, two branch currents, source current.");
                default: return null;
            }
        }
    }
}
#endif

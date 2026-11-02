#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>Creates editable Level 1-6 authored diagram assets; does not change gameplay.</summary>
    public static class SparkLevel0106DrawingAssetCreator
    {
        private static readonly Color Hot = new Color(.98f,.32f,.36f,1f);
        private static readonly Color Cold = new Color(.22f,.69f,1f,1f);
        private static readonly Color Gold = new Color(1f,.79f,.30f,1f);
        private static readonly Color Cyan = new Color(.23f,.98f,.82f,1f);
        private static Vector2 P(float x,float y) => new Vector2(x,y);
        private static SparkSchematicPart Part(string id,SparkSchematicKind kind,float x,float y,string label,Color color)
            => new SparkSchematicPart { id=id,kind=kind,position=P(x,y),label=label,color=color };
        private static SparkSchematicWire Wire(Color color,params Vector2[] points)
            => new SparkSchematicWire { color=color,energized=false,points=points };

        [MenuItem("Tools/Project Spark/Visual Teaching/Level 01-06/Create All Six Drawing Assets")]
        public static void CreateAll()
        {
            string folder = EditorUtility.OpenFolderPanel("Choose Unity Assets folder for six drawings",Application.dataPath,"");
            if (string.IsNullOrEmpty(folder)) return;
            string assetsRoot = Application.dataPath.Replace('\\','/');
            folder = folder.Replace('\\','/');
            if (folder != assetsRoot && !folder.StartsWith(assetsRoot + "/",StringComparison.Ordinal))
            { EditorUtility.DisplayDialog("Invalid folder","Choose a folder inside this project's Assets folder.","OK"); return; }
            string relative = "Assets" + folder.Substring(assetsRoot.Length);
            int created = 0;
            for(int level=1;level<=6;level++) if(Create(level,relative)) created++;
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Level Drawing Assets",$"Created {created} new asset(s). Existing files were not overwritten. Add them to SparkDrawCurrentLevel.Drawings.","OK");
        }

        [MenuItem("Tools/Project Spark/Visual Teaching/Level 01-06/Create Level 01")]
        public static void Create1() => CreateSingle(1);
        [MenuItem("Tools/Project Spark/Visual Teaching/Level 01-06/Create Level 02")]
        public static void Create2() => CreateSingle(2);
        [MenuItem("Tools/Project Spark/Visual Teaching/Level 01-06/Create Level 03")]
        public static void Create3() => CreateSingle(3);
        [MenuItem("Tools/Project Spark/Visual Teaching/Level 01-06/Create Level 04")]
        public static void Create4() => CreateSingle(4);
        [MenuItem("Tools/Project Spark/Visual Teaching/Level 01-06/Create Level 05")]
        public static void Create5() => CreateSingle(5);
        [MenuItem("Tools/Project Spark/Visual Teaching/Level 01-06/Create Level 06")]
        public static void Create6() => CreateSingle(6);
        private static void CreateSingle(int level)
        {
            string path = EditorUtility.SaveFilePanelInProject("Create level diagram",$"Level_{level:00}_Drawing","asset","Select where to save the drawing.");
            if(string.IsNullOrEmpty(path)) return;
            if(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path)!=null)
            { EditorUtility.DisplayDialog("Asset exists","Choose a new path; existing drawings are not overwritten.","OK"); return; }
            CreateAt(level,path);
            AssetDatabase.SaveAssets();
        }
        private static bool Create(int level,string folder)
        {
            string path=$"{folder}/Level_{level:00}_Drawing.asset";
            if(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path)!=null)
            { Debug.LogWarning("[Level Draw] Skipped existing: "+path); return false; }
            CreateAt(level,path); return true;
        }
        private static void CreateAt(int level,string path)
        {
            var a=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            a.levelId=$"level_{level:00}";
            a.levelTitle=Titles[level-1]; a.initialPage=0;
            a.pages=new SparkLevelDrawingPage[4];
            for(int page=0;page<4;page++) a.pages[page]=Build(level,page);
            AssetDatabase.CreateAsset(a,path); EditorUtility.SetDirty(a);
            Selection.activeObject=a;
            Debug.Log("[Level Draw] Created "+path);
        }
        private static readonly string[] Titles = {
            "Level 1 - Basic Circuit (Starter)","Level 2 - Resistor and LED (Starter)",
            "Level 3 - The Current Limiter (Starter)","Level 4 - The Power Challenge (Starter)",
            "Level 5 - Parallel Paths (Starter)","Level 6 - Circuit Fault Detective"
        };

        private static SparkLevelDrawingPage Build(int level,int page)
        {
            SparkSchematicPart[] parts;
            SparkSchematicWire[] wires;
            switch(level)
            {
                case 1: BuildSeries(new[] { Part("led",SparkSchematicKind.Generic,.64f,.76f,"LED",Cyan) },out parts,out wires); break;
                case 2: BuildSeries(new[] { Part("r",SparkSchematicKind.Resistor,.49f,.76f,"R (fixed)",Gold), Part("led",SparkSchematicKind.Generic,.72f,.76f,"LED",Cyan) },out parts,out wires); break;
                case 3: BuildSeries(new[] { Part("r",SparkSchematicKind.Resistor,.49f,.76f,"R (shelf)",Gold), Part("led",SparkSchematicKind.Generic,.72f,.76f,"LED",Cyan) },out parts,out wires); break;
                case 4: BuildSeries(new[] { Part("r",SparkSchematicKind.Resistor,.49f,.76f,"Resistor",Gold), Part("led",SparkSchematicKind.Generic,.72f,.76f,"Load",Cyan) },out parts,out wires); break;
                case 5: BuildParallel(false,out parts,out wires); break;
                default: BuildParallel(true,out parts,out wires); break;
            }
            string name=Titles[level-1];
            string title=page==0?"Overview":page==1?"Connection Guide":page==2?"Measurements":"Solution & Checks";
            string explanation=Explain(level,page);
            return new SparkLevelDrawingPage { title=$"LEVEL {level:00} | {title}",caption=$"PAGE {page+1}/4 | {name}",explanation=explanation,parts=parts,wires=wires };
        }

        // All coordinates are intentionally spread to accommodate existing label renderer.
        private static void BuildSeries(SparkSchematicPart[] devices,out SparkSchematicPart[] parts,out SparkSchematicWire[] wires)
        {
            var list=new System.Collections.Generic.List<SparkSchematicPart> {
                Part("battery",SparkSchematicKind.Battery,.11f,.51f,"5 V",Hot),
                Part("switch",SparkSchematicKind.Switch,.29f,.76f,"SWITCH",Gold)
            };
            list.AddRange(devices);
            list.Add(Part("ground",SparkSchematicKind.Ground,.47f,.17f,"GND",Cold));
            parts=list.ToArray();
            var ws=new System.Collections.Generic.List<SparkSchematicWire> {
                Wire(Hot,P(.11f,.65f),P(.11f,.76f),P(.23f,.76f)),
                Wire(Cold,P(.88f,.76f),P(.88f,.17f),P(.11f,.17f),P(.11f,.37f))
            };
            var nodes=new System.Collections.Generic.List<float> { .35f };
            foreach(var d in devices) nodes.Add(d.position.x);
            float last=.35f;
            foreach(var d in devices) { ws.Add(Wire(Hot,P(last,.76f),P(d.position.x-.06f,.76f))); last=d.position.x+.06f; }
            ws.Add(Wire(Hot,P(last,.76f),P(.88f,.76f)));
            wires=ws.ToArray();
        }
        private static void BuildParallel(bool faults,out SparkSchematicPart[] parts,out SparkSchematicWire[] wires)
        {
            parts=new[] {
                Part("battery",SparkSchematicKind.Battery,.11f,.50f,"5 V",Hot),
                Part("switch",SparkSchematicKind.Switch,.27f,.81f,"SWITCH",Gold),
                Part("bus",SparkSchematicKind.Generic,.40f,.81f,"",Cyan),
                Part("r1",SparkSchematicKind.Resistor,.55f,.65f,"R1 330 Ohm",Gold),
                Part("r2",SparkSchematicKind.Resistor,.55f,.38f,faults?"R2 100 Ohm":"R2 (branch)",Gold),
                Part("led1",SparkSchematicKind.Generic,.70f,.65f,"LED 1",Cyan),
                Part("led2",SparkSchematicKind.Generic,.70f,.38f,"LED 2",Cyan),
                Part("ground",SparkSchematicKind.Ground,.79f,.17f,"GND",Cold)
            };
            var w=new System.Collections.Generic.List<SparkSchematicWire> {
                Wire(Hot,P(.11f,.64f),P(.11f,.81f),P(.21f,.81f)),
                Wire(Hot,P(.33f,.81f),P(.40f,.81f),P(.40f,.38f)),
                Wire(Hot,P(.40f,.65f),P(.49f,.65f)),
                Wire(Hot,P(.40f,.38f),P(.49f,.38f)),
                Wire(Hot,P(.61f,.65f),P(.64f,.65f)),
                Wire(Hot,P(.61f,.38f),P(.64f,.38f)),
                Wire(Cold,P(.76f,.65f),P(.79f,.65f),P(.79f,.17f),P(.11f,.17f),P(.11f,.36f))
            };
            if(!faults) w.Add(Wire(Cold,P(.76f,.38f),P(.79f,.38f)));
            // Fault detective page intentionally omits lower branch return.
            wires=w.ToArray();
        }
        private static string Explain(int level,int page)
        {
            string prefix=level<6?"STARTER EXAMPLE: Confirm against the actual Level definition/layout; exact terminal IDs, parts and objectives are not supplied. ":"AUTHOR LEVEL 6 DESIGN: circuit illustrates initial faults, not live wiring. ";
            string[] notes;
            switch(level)
            {
                case 1: notes=new[]{"Introduction to closed DC circuits: battery, switch and load.","Illustrative: Battery_positive -> switch input -> load -> Battery_negative. Use verified scene terminal IDs.","With switch ON, inspect supply voltage and load current from the solver.","Ensure an actual closed return, supported component solver definition and the level's configured objectives."}; break;
                case 2: notes=new[]{"Fixed resistor and LED in a series circuit.","Illustrative: switch output -> fixed resistor -> LED anode; LED cathode -> Battery_negative.","At 5 V, a 150 Ohm resistor and nominal 2 V LED would carry about 20 mA (illustrative only).","Confirm LED polarity, resistor current, no short and level's real objective targets."}; break;
                case 3: notes=new[]{"Current Limiter: experiment with shelf resistor choices.","Illustrative: switch output -> shelf resistor -> LED anode -> LED cathode -> Battery_negative.","Compare solver current when resistor values change; do not treat shelf items as connected until placed.","Check workbench placement, solver discovery, safe current and actual Level 3 objectives."}; break;
                case 4: notes=new[]{"Power Challenge: observe power dissipated by a resistor/load.","Illustrative: battery -> switch -> resistor -> load -> ground. Adapt to actual Level 4 topology.","Read verified voltage, current and power; resistor power depends on V multiplied by I.","Use actual resistor power rating and SparkResistorBurn thresholds; don't infer burning solely from resistance."}; break;
                case 5: notes=new[]{"Parallel Paths: two LED/resistor branches share source nodes.","Illustrative: SwitchOutput to BOTH branch resistors; each goes to its own LED; both cathodes return to Battery_negative.","Each branch has a source-voltage reference; sum branch currents for total when there are no other loads.","Check each branch independently, returns, LED polarity, objectives and source current."}; break;
                default: notes=new[]{"Circuit Fault Detective: R1 330 Ohm/LED1 working branch; R2 100 Ohm/LED2 fault branch with missing return.","Fault-start: SwitchOutput -> R1 -> LED1 -> ground. SwitchOutput -> R2 100 Ohm -> LED2 -> OPEN RETURN. Repair missing return then replace R2.","At 5 V and nominal 2 V LED: R1 about 9.1 mA, R2 at 100 Ohm about 30 mA after return repair; 220 Ohm about 13.6 mA.","To pass: both LEDs on, branch currents 6-15 mA, source current 12-30 mA, installed resistors healthy. 470 Ohm can also meet branch target."}; break;
            }
            return prefix + notes[Mathf.Clamp(page,0,3)] + " Author drawing only — NOT verified live circuit topology or measured results.";
        }
    }
}
#endif

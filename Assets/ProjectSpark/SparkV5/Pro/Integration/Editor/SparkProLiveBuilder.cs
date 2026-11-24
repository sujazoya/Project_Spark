using System;
using System.Collections.Generic;
using System.IO;
using ProjectSpark.AI;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    internal static class SparkProLiveBuilder
    {
        private const string Menu = "Project Spark/Spark V5 Pro/";
        private const string Root = "Assets/ProjectSpark/SparkV5/Pro";
        private const string Resources = Root + "/Resources";
        private const string PanelPath = Resources + "/SparkProLivePanel.prefab";
        private const string LibraryPath = Resources + "/SparkProTeachingLibrary.asset";
        internal const string CharacterPath = Root + "/Generated/SparkV5Pro.prefab";

        private const string Game = "Assets/My_Assets/_Project_Spark/";
        private static readonly (string Id, string Path, string Description)[] Models =
        {
            ("resistor", Game + "Prefabs/Electronic Components/Registers/Resistor_1k Ω.prefab",
                "A game resistor model. A resistor limits current and produces a voltage drop when current flows."),
            ("capacitor", Game + "Prefabs/Electronic Components/Capacitor/Capacitor_1000uF_16V.prefab",
                "A polarized capacitor model. Identify its marked leads before connecting it in a practice circuit."),
            ("led", Game + "Models/Level Items/LED.fbx",
                "An LED model. An LED emits light under suitable forward current."),
            ("diode", Game + "Prefabs/Levels/Level_96_Layout/Level96/Prefabs/Modules/L96_D1_Diode.prefab",
                "A game diode model. A diode primarily conducts in its forward direction."),
            ("transistor", Game + "Models/Level Items/Transistor.fbx",
                "An illustrative transistor package. Use the specific part documentation to identify each terminal."),
            ("mosfet", Game + "Prefabs/Levels/Level_97_Layout/Level97/Models_Rebuilt/09_L97_Q1_MOSFET.fbx",
                "An authored board MOSFET package. A MOSFET uses gate voltage to control a channel."),
            ("switch", Game + "Models/Level Items/ToggleSwitch.fbx",
                "A toggle switch model. The control opens or closes a connection; use the actual circuit to verify its state."),
            ("multimeter", Game + "Models/Work Shop/Multimeter/MutliMeter.obj",
                "The workshop meter model. Connect in parallel for voltage, and use the specified input jack for the selected mode."),
            ("power supply", Game + "Prefabs/Levels/Level_95_Layout/Level95/Models/15_L95_Protected5VBenchSupply.fbx",
                "A training bench supply model. Read the actual supply settings and measurement before drawing conclusions."),
            ("integrated circuit", Game + "Prefabs/Levels/Level_120_Layout/RAX28/Models_OBJ/313_L120_GateDriverIsolationReference.obj",
                "An authored Level 120 gate driver reference. Pin assignments depend on the part and its circuit documentation."),
            ("pcb", Game + "Prefabs/Levels/Level_120_Layout/RAX28/Models_OBJ/265_L120_PCB_Main.obj",
                "The Level 120 board substrate. A PCB mechanically supports parts and routes connections through conductive layers."),
            ("motherboard", Game + "Prefabs/Levels/Level_120_Layout/L120_CompleteMotherboard_VISUAL_ONLY.prefab",
                "An illustrative RAX-28 model from The Axis Forge. Evaluate the live board through the game solver and objectives."),
            ("rdx-21 board", Game + "Prefabs/Levels/Level_113_Layout/RDX21/Models_OBJ/167_L113_PCB_Main.obj",
                "Level 113 board substrate. Compare its two protected input regions with supervised W233 authorization. Use the mission diagram for terminal evidence."),
            ("rdx-21 heartbeat controller", Game + "Prefabs/Levels/Level_113_Layout/RDX21/Models_OBJ/015_L113_U15.obj",
                "The U15 package from Level 113. Study the orientation mark, then trace the separate U15-to-U6 authorization path in the training schematic."),
            ("axis forge dc link", Game + "Prefabs/Levels/Level_120_Layout/RAX28/Models_OBJ/306_L120_SharedDCLink_CapBank.obj",
                "Axis Forge capacitor-bank reference. Inspect the assembly and relate stored-energy capacity to the training precharge and controlled-discharge checks."),
            ("axis forge encoder reference", Game + "Prefabs/Levels/Level_120_Layout/RAX28/Models_OBJ/308_L120_Encoder_ReferencePlane.obj",
                "Authored encoder-reference plane from The Axis Forge. Compare it with the mission's encoder plausibility and reference-domain checks."),
            ("axis forge sto guard zone", Game + "Prefabs/Levels/Level_120_Layout/RAX28/Models_OBJ/307_L120_DualSTO_GuardZone.obj",
                "Teaching reference for the two torque-off channels in The Axis Forge. The model illustrates location; the game evaluator verifies the safety trial.")
        };

        [MenuItem(Menu + "Build Live Communication Panel")]
        internal static void BuildLivePanel()
        {
            EnsureResources();
            var existing = AssetDatabase.LoadMainAssetAtPath(PanelPath);
            if (existing && (!(existing is GameObject own) || !own.GetComponent<SparkProLivePanel>()))
                throw new InvalidOperationException("The live panel path contains an unrelated asset.");
            if (existing && ((GameObject)existing).GetComponent<SparkProLivePanel>().PrefabVersion >= SparkProLivePanel.CurrentPrefabVersion)
            {
                ValidatePanel((GameObject)existing);
                return;
            }

            var panel = SparkProLivePanel.Build();
            try
            {
                var saved = PrefabUtility.SaveAsPrefabAsset(panel.gameObject, PanelPath);
                ValidatePanel(saved);
            }
            finally
            {
                Object.DestroyImmediate(panel.gameObject);
            }
        }

        [MenuItem(Menu + "Register Holographic Teaching Models")]
        internal static void RegisterTeachingModels()
        {
            EnsureResources();
            var library = AssetDatabase.LoadAssetAtPath<SparkProTeachingLibrary>(LibraryPath);
            if (!library)
            {
                if (AssetDatabase.LoadMainAssetAtPath(LibraryPath))
                    throw new InvalidOperationException("The teaching library path contains an unrelated asset.");
                library = ScriptableObject.CreateInstance<SparkProTeachingLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            var models = new List<SparkProHologram.TeachingModel>(library.models ?? Array.Empty<SparkProHologram.TeachingModel>());
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var existing in models)
                if (existing != null && existing.model) seen.Add(existing.id);
            int added = 0;
            foreach (var choice in Models)
            {
                if (seen.Contains(choice.Id)) continue;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(choice.Path);
                if (!model || !HasDrawableGeometry(model))
                {
                    Debug.LogWarning($"[Spark Pro] Missing drawable source for {choice.Id}: {choice.Path}");
                    continue;
                }
                models.Add(new SparkProHologram.TeachingModel { id = choice.Id, model = model, description = choice.Description });
                seen.Add(choice.Id);
                added++;
            }
            if (added > 0)
            {
                Undo.RecordObject(library, "Register Spark teaching models");
                library.models = models.ToArray();
                EditorUtility.SetDirty(library);
                AssetDatabase.SaveAssetIfDirty(library);
            }
            Debug.Log($"[Spark Pro] Teaching library has {seen.Count} entries. Added {added}.");
        }

        private static bool HasDrawableGeometry(GameObject model)
        {
            foreach (var filter in model.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh && filter.GetComponent<MeshRenderer>())
                    return true;
            return false;
        }

        [MenuItem(Menu + "Validate All AI References")]
        internal static void ValidateAll()
        {
            ValidateCharacter();
            ValidatePanel(AssetDatabase.LoadAssetAtPath<GameObject>(PanelPath));
            var executives = Object.FindObjectsByType<SparkProExecutive>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (executives.Length != 1)
                throw new InvalidOperationException($"Expected one scene executive, found {executives.Length}.");
            var state = new SerializedObject(executives[0]);
            foreach (string field in new[] { "assistant", "world", "voice", "tts", "playbackSource", "character", "microphone", "academy", "hologram", "catalog" })
                if (!state.FindProperty(field).objectReferenceValue)
                    throw new InvalidOperationException("Scene executive is missing " + field);
            Debug.Log("[Spark Pro] Character prefab, Live panel and scene dependencies validated.");
        }

        [MenuItem(Menu + "Audit Complete AI Integration")]
        internal static void AuditComplete()
        {
            SparkProKnowledgeCompiler.CompileCatalog(false);
            ValidateAll();
        }

        [MenuItem(Menu + "Auto Configure Complete AI")]
        internal static void ConfigureComplete()
        {
            var executives = Object.FindObjectsByType<SparkProCharacter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (executives.Length != 1)
                throw new InvalidOperationException($"Expected one scene character, found {executives.Length}. Select and bind the intended character instead.");
            BuildLivePanel();
            SparkProKnowledgeCompiler.CompileCatalog(false);
            RegisterTeachingModels();
            var previous = Selection.activeObject;
            try
            {
                Selection.activeObject = executives[0].gameObject;
                SparkProIntegrationSetup.Bind();
            }
            finally { Selection.activeObject = previous; }
            ValidateAll();
        }

        internal static void ValidateCharacter()
        {
            var character = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPath);
            if (!character || !character.GetComponent<SparkProCharacter>() ||
                !character.TryGetComponent<LODGroup>(out var group) || group.GetLODs().Length != 4)
                throw new InvalidOperationException("The existing V5 Pro prefab or its four LODs are missing.");
            foreach (var lod in group.GetLODs())
                if (lod.renderers.Length == 0)
                    throw new InvalidOperationException("V5 Pro contains a LOD without renderers.");
            for (int i = 0; i < 4; i++)
                if (!AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/ProjectSpark/SparkV5/Models/SparkV5_LOD{i}.sparkmesh"))
                    throw new InvalidOperationException($"Native Spark mesh LOD {i} did not import.");
        }

        private static void ValidatePanel(GameObject root)
        {
            if (!root || !root.TryGetComponent(out SparkProLivePanel panel))
                throw new InvalidOperationException("Build the Spark Live communication panel first.");
            var references = new SerializedObject(panel);
            foreach (string field in new[] { "shell", "messages", "scroll", "input", "talk", "send", "close", "handsFree", "sensitivity" })
                if (!references.FindProperty(field).objectReferenceValue)
                    throw new InvalidOperationException("Live prefab is missing " + field);
        }

        private static void EnsureResources()
        {
            if (!AssetDatabase.IsValidFolder(Resources))
                AssetDatabase.CreateFolder(Root, "Resources");
        }
    }
}


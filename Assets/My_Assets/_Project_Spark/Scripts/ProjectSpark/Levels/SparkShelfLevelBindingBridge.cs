using System;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Routes the occupant of a named level workstation shelf slot into the
    /// existing SparkLevelSceneBindings runtime override API.
    /// Install once on a persistent active GameObject (not the level prefab).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkShelfLevelBindingBridge : MonoBehaviour
    {
        [Header("Existing Systems")]
        [SerializeField] private SparkObjectShelf shelf;
        [SerializeField] private LevelGamePlayManager gameplay;
        [SerializeField] private SparkLevelLayoutLoader layoutLoader;

        [Header("Level / Workstation")]
        [SerializeField] private string levelId = "level_11";
        [SerializeField] private string workstationSlotId = "R2_Workstation";

        [Header("Stable Scene Binding IDs (must already be declared)")]
        [SerializeField] private string componentBindingId = "R2_Component";
        [SerializeField] private string inputTerminalBindingId = "R2_Input";
        [SerializeField] private string outputTerminalBindingId = "R2_Output";

        [Header("Terminal Transform Names on Installed Resistor")]
        [SerializeField] private string inputTerminalName = "Register_Input";
        [SerializeField] private string outputTerminalName = "Register_Output";
        [SerializeField] private bool logTransitions = true;

        private SparkLevelSceneBindings lastBindings;
        private SparkShelfSlot lastSlot;
        private SparkElectricalComponent lastComponent;
        private SparkTerminal lastInput;
        private SparkTerminal lastOutput;
        private bool hooked;

        private void Awake() => Resolve();
        private void OnEnable() { Resolve(); Subscribe(); Sync(); }
        private void Start() { Resolve(); Subscribe(); Sync(); }
        private void OnDisable() { Unsubscribe(); ClearOld(); }

        private void Resolve()
        {
            if (shelf == null) shelf = FindFirstObjectByType<SparkObjectShelf>();
            if (gameplay == null) gameplay = FindFirstObjectByType<LevelGamePlayManager>();
            if (layoutLoader == null) layoutLoader = FindFirstObjectByType<SparkLevelLayoutLoader>();
        }

        private void Subscribe()
        {
            if (hooked || shelf == null || gameplay == null) return;
            shelf.ItemPlaced += OnItemPlaced;
            gameplay.LevelStarted += OnLevelStarted;
            hooked = true;
        }

        private void Unsubscribe()
        {
            if (!hooked) return;
            if (shelf != null) shelf.ItemPlaced -= OnItemPlaced;
            if (gameplay != null) gameplay.LevelStarted -= OnLevelStarted;
            hooked = false;
        }

        private void OnLevelStarted(SparkLevelDefinition _) => Sync();
        private void OnItemPlaced(SparkShelfItem _, SparkShelfSlot __) => Sync();

        [ContextMenu("Project Spark / Sync Workstation Binding")]
        public void Sync()
        {
            Resolve();
            if (shelf == null || gameplay == null || layoutLoader == null) return;

            var level = gameplay.ActiveLevel;
            var layout = layoutLoader.ActiveLayout;
            if (level == null || layout == null || layout.LevelDefinition != level ||
                !string.Equals(level.LevelId, levelId, StringComparison.OrdinalIgnoreCase))
            {
                ClearOld();
                return;
            }

            SparkLevelSceneBindings bindings = layout.SceneBindings;
            if (bindings == null) return;

            SparkShelfSlot slot = null;
            foreach (SparkShelfSlot candidate in shelf.RegisteredSlots)
            {
                if (candidate != null && candidate.Kind == SparkShelfSlotKind.Workstation &&
                    string.Equals(candidate.SlotId, workstationSlotId, StringComparison.Ordinal) &&
                    candidate.transform.IsChildOf(layout.transform))
                { slot = candidate; break; }
            }
            if (slot == null)
            {
                if (logTransitions) Debug.LogWarning("[Shelf Binding] Workstation slot not found: " + workstationSlotId, this);
                ClearOld();
                return;
            }

            SparkShelfItem occupant = slot.Occupant;
            SparkElectricalComponent component = occupant != null
                ? occupant.GetComponentInChildren<SparkElectricalComponent>(true) : null;
            SparkTerminal input = FindTerminal(occupant, inputTerminalName);
            SparkTerminal output = FindTerminal(occupant, outputTerminalName);

            if (bindings == lastBindings && slot == lastSlot && component == lastComponent &&
                input == lastInput && output == lastOutput) return;

            ClearOld();
            lastBindings = bindings;
            lastSlot = slot;
            lastComponent = component;
            lastInput = input;
            lastOutput = output;

            // An empty slot must mask the serialized fallback object.
            // RegisterRuntimeComponent does not accept null; therefore an empty
            // slot is masked by registering the last component then unregistering it.
            // If the slot starts empty and no prior component is known, the
            // serialized fallback remains and should be handled via an explicit
            // ClearRuntimeComponentBinding API shown in the README.
            if (component != null)
            {
                if (!bindings.RegisterRuntimeComponent(componentBindingId, component))
                    Debug.LogError("[Shelf Binding] Declare component binding ID: " + componentBindingId, this);
            }
            else
            {
                bindings.ClearRuntimeComponentBinding(componentBindingId);
            }
            bindings.SetRuntimeTerminal(inputTerminalBindingId, input);
            bindings.SetRuntimeTerminal(outputTerminalBindingId, output);
            gameplay.QueueEvaluation();
            if (logTransitions)
                Debug.Log("[Shelf Binding] " + workstationSlotId + " => " +
                    (component != null ? component.name : "EMPTY") + ", input=" +
                    (input != null ? input.name : "NULL") + ", output=" +
                    (output != null ? output.name : "NULL"), this);
        }

        private void ClearOld()
        {
            if (lastBindings != null)
            {
                // Intentionally mask the old serialized component on removal.
                lastBindings.ClearRuntimeComponentBinding(componentBindingId);
                lastBindings.SetRuntimeTerminal(inputTerminalBindingId, null);
                lastBindings.SetRuntimeTerminal(outputTerminalBindingId, null);
            }
            lastBindings = null;
            lastSlot = null;
            lastComponent = null;
            lastInput = null;
            lastOutput = null;
        }

        private static SparkTerminal FindTerminal(SparkShelfItem item, string name)
        {
            if (item == null || string.IsNullOrWhiteSpace(name)) return null;
            foreach (SparkTerminal t in item.GetComponentsInChildren<SparkTerminal>(true))
                if (t != null && string.Equals(t.name, name, StringComparison.Ordinal)) return t;
            return null;
        }
    }
}

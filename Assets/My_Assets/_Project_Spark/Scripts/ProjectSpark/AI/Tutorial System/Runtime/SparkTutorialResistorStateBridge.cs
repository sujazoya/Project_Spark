using System;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;
using ProjectSpark.AI;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Phase 6: truthful, state-driven resistor instruction.
    /// A visible resistor is not treated as placed unless the workstation owns it.
    /// An energized resistor is not inferred from wires or light alone: the solver's
    /// electrical state must report meaningful current.
    /// Replace the old SparkTutorialResistorStateBridge with this file.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkTutorialResistorStateBridge : MonoBehaviour
    {
        public enum ElectricalStage
        {
            NoWorkstation,
            Empty,
            PlacedUnwired,
            PartiallyWired,
            WiredNoCurrent,
            Conducting,
            Burned
        }

        [Header("References")]
        [SerializeField] private SparkTutorialDirector director;
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkAIVoiceController voice;

        [Header("Runtime workstation")]
        [SerializeField] private string resistorSlotId = "Register_Slot";
        [SerializeField] private SparkShelfSlot explicitWorkstationSlot;

        [Header("Teaching")]
        [SerializeField] private bool completePlacementStepFromState = true;
        [SerializeField] private bool updateTutorialHint = true;
        [SerializeField] private bool narrateStageChanges = false;
        [SerializeField] private bool requireClosedReturnForConducting = true;
        [SerializeField, Min(0.05f)] private float refreshInterval = 0.25f;
        [SerializeField, Min(0f)] private float meaningfulCurrentAmps = 0.0001f;
        [SerializeField, Min(0f)] private float speechCooldownSeconds = 8f;
        [SerializeField] private bool diagnostics = false;

        public ElectricalStage CurrentStage { get; private set; } = ElectricalStage.NoWorkstation;
        public SparkResistor CurrentResistor { get; private set; }
        public string CurrentExplanation { get; private set; } = string.Empty;
        public event Action<ElectricalStage, string> ExplanationChanged;

        private SparkShelfSlot cachedSlot;
        private SparkResistor lastResistor;
        private float nextRefresh;
        private float lastSpeechTime = -999f;
        private string lastLevelId;
        private bool hasSnapshot;

        private void Awake() => ResolveReferences();

        private void OnEnable()
        {
            ResolveReferences();
            if (levelManager != null)
            {
                levelManager.LevelStarted += OnLevelChanged;
                levelManager.LevelReset += OnLevelChanged;
                levelManager.LevelEvaluationChanged += OnEvaluation;
                levelManager.LevelCompleted += OnLevelCompleted;
            }
            if (director != null)
                director.StepChanged += OnStepChanged;
            nextRefresh = 0f;
        }

        private void OnDisable()
        {
            if (levelManager != null)
            {
                levelManager.LevelStarted -= OnLevelChanged;
                levelManager.LevelReset -= OnLevelChanged;
                levelManager.LevelEvaluationChanged -= OnEvaluation;
                levelManager.LevelCompleted -= OnLevelCompleted;
            }
            if (director != null)
                director.StepChanged -= OnStepChanged;
        }

        private void ResolveReferences()
        {
            if (director == null) director = FindFirstObjectByType<SparkTutorialDirector>();
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelGamePlayManager>();
            if (voice == null) voice = FindFirstObjectByType<SparkAIVoiceController>();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + refreshInterval;
            Refresh();
        }

        private void OnLevelChanged(SparkLevelDefinition level)
        {
            cachedSlot = null;
            lastResistor = null;
            hasSnapshot = false;
            lastLevelId = level != null ? level.LevelId : null;
            nextRefresh = 0f;
        }

        private void OnEvaluation(SparkLevelDefinition _) => Refresh();
        private void OnStepChanged(SparkTutorialDefinition _, int __) => Refresh();
        private void OnLevelCompleted(SparkLevelDefinition _) => Refresh();

        private SparkShelfSlot FindWorkstation()
        {
            if (explicitWorkstationSlot != null &&
                explicitWorkstationSlot.isActiveAndEnabled &&
                explicitWorkstationSlot.gameObject.activeInHierarchy)
                return explicitWorkstationSlot;

            if (cachedSlot != null && cachedSlot.isActiveAndEnabled &&
                cachedSlot.gameObject.activeInHierarchy)
                return cachedSlot;

            SparkShelfSlot[] slots = FindObjectsByType<SparkShelfSlot>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (SparkShelfSlot slot in slots)
            {
                if (slot != null && slot.Kind == SparkShelfSlotKind.Workstation &&
                    string.Equals(slot.SlotId, resistorSlotId, StringComparison.OrdinalIgnoreCase))
                {
                    cachedSlot = slot;
                    return slot;
                }
            }
            return null;
        }

        public void Refresh()
        {
            ResolveReferences();
            if (levelManager == null || !levelManager.IsPlaying) return;
            SparkLevelDefinition active = levelManager.ActiveLevel;
            if (active == null) return;
            if (!string.Equals(lastLevelId, active.LevelId, StringComparison.Ordinal))
                OnLevelChanged(active);

            SparkShelfSlot slot = FindWorkstation();
            SparkResistor resistor = null;
            SparkShelfItem occupant = slot != null ? slot.Occupant : null;
            if (occupant != null)
            {
                resistor = occupant.GetComponent<SparkResistor>();
                if (resistor == null) resistor = occupant.GetComponentInChildren<SparkResistor>();
            }

            CurrentResistor = resistor;
            if (resistor != lastResistor)
            {
                lastResistor = resistor;
                hasSnapshot = false;
            }

            ElectricalStage stage;
            string explanation;
            if (slot == null)
            {
                stage = ElectricalStage.NoWorkstation;
                explanation = "The active level's resistor workstation is not available yet.";
            }
            else if (resistor == null)
            {
                stage = ElectricalStage.Empty;
                explanation = occupant == null
                    ? "Choose a resistor from the shelf and place it in the workstation."
                    : "This workstation contains a different type of component.";
            }
            else
            {
                // The authoritative occupancy test is now satisfied, even if the
                // placement event arrived before the tutorial entered this step.
                if (completePlacementStepFromState && director != null &&
                    director.IsRunning && director.CurrentStep != null &&
                    string.Equals(director.CurrentStep.id, "workstation", StringComparison.OrdinalIgnoreCase))
                {
                    director.Signal("shelf.item_placed");
                }

                var circuit = levelManager.CircuitSystem;
                int connectionsA = circuit != null && resistor.TerminalA != null
                    ? circuit.GetConnectionCount(resistor.TerminalA) : 0;
                int connectionsB = circuit != null && resistor.TerminalB != null
                    ? circuit.GetConnectionCount(resistor.TerminalB) : 0;

                float volts = IsFinite(resistor.Voltage) ? resistor.Voltage : 0f;
                float amps = IsFinite(resistor.Current) ? resistor.Current : 0f;
                float watts = IsFinite(resistor.Power) ? resistor.Power : 0f;
                float ohms = resistor.ResistanceOhms;

                SparkResistorBurn burn = resistor.GetComponent<SparkResistorBurn>();
                if (burn != null && burn.IsFailed)
                {
                    stage = ElectricalStage.Burned;
                    explanation = $"The {ohms:0.##} ohm resistor has burned out. Disconnect power and replace it before continuing.";
                }
                else if (connectionsA == 0 && connectionsB == 0)
                {
                    stage = ElectricalStage.PlacedUnwired;
                    explanation = $"{ohms:0.##} ohm resistor installed. Neither terminal has a wire. Connect both terminals into the circuit.";
                }
                else if (connectionsA == 0 || connectionsB == 0)
                {
                    stage = ElectricalStage.PartiallyWired;
                    explanation = $"{ohms:0.##} ohm resistor installed. Only one side is wired. Complete the path through its other terminal.";
                }
                else if (Mathf.Abs(amps) >= meaningfulCurrentAmps &&
                    (!requireClosedReturnForConducting || levelManager.ClosedReturn))
                {
                    stage = ElectricalStage.Conducting;
                    explanation = $"{ohms:0.##} ohm resistor conducting: {volts:0.###} volts, {Mathf.Abs(amps):0.####} amps, {Mathf.Max(0f, watts):0.####} watts. The resistor limits current and dissipates electrical energy as heat.";
                }
                else
                {
                    stage = ElectricalStage.WiredNoCurrent;
                    explanation = $"{ohms:0.##} ohm resistor has wires on both terminals, but the solver reports {Mathf.Abs(amps):0.####} amps. Check the source, switch, and return path. Two connected wires alone do not guarantee a closed circuit.";
                }
            }

            bool stageChanged = !hasSnapshot || stage != CurrentStage;
            bool changed = stageChanged || explanation != CurrentExplanation;
            CurrentStage = stage;
            CurrentExplanation = explanation;
            hasSnapshot = true;
            if (!changed) return;

            if (updateTutorialHint && director != null)
                director.SetLiveElectricalNote(explanation);

            ExplanationChanged?.Invoke(stage, explanation);
            if (diagnostics) Debug.Log("[ResistorTeacher] " + stage + " | " + explanation, this);

            // Speak only on meaningful stage transitions, not every numerical update.
            if (narrateStageChanges && stageChanged && voice != null &&
                (stage == ElectricalStage.Conducting || stage == ElectricalStage.Burned ||
                 stage == ElectricalStage.PartiallyWired || stage == ElectricalStage.PlacedUnwired) &&
                Time.unscaledTime - lastSpeechTime >= speechCooldownSeconds)
            {
                lastSpeechTime = Time.unscaledTime;
                voice.SpeakText(explanation);
            }
        }

        private static bool IsFinite(float n) => !float.IsNaN(n) && !float.IsInfinity(n);

        [ContextMenu("Diagnostics / Refresh Live Resistor Explanation")]
        private void DebugRefresh() => Refresh();
    }
}

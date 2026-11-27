using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using ProjectSpark.Measurement;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    internal sealed class NexusTarget
    {
        public Component Component { get; }
        public string Label { get; }
        public string NativeId { get; }
        public NexusTarget(Component component, string label, string id)
        {
            Component = component; Label = label; NativeId = id;
        }
    }

    internal sealed class NexusSnapshot
    {
        public int Revision { get; }
        public int Epoch { get; }
        public SparkLevelDefinition Level { get; }
        public Transform Layout { get; }
        public SparkLevelSceneBindings Bindings { get; }
        public IReadOnlyList<NexusTarget> Targets { get; }
        public Component Selected { get; }
        public int SolveRevision { get; }
        public int Topology { get; }
        public string Objectives { get; }
        public string Fault { get; }
        public string Telemetry { get; }

        public NexusSnapshot(int revision, int epoch, LevelGamePlayManager manager, Transform layout,
            IReadOnlyList<NexusTarget> targets, Component selected)
        {
            Revision = revision; Epoch = epoch; Layout = layout; Targets = targets; Selected = selected;
            Level = layout && manager ? manager.ActiveLevel : null;
            Bindings = layout && manager ? manager.SceneBindings : null;
            var solver = Level ? manager.ElectricalSolver : null;
            Topology = Level && manager.CircuitSystem ? manager.CircuitSystem.TopologyVersion : -1;
            bool current = solver && !solver.IsDirty && !solver.IsSolving && solver.LatestSolveResult != null;
            SolveRevision = current ? solver.CompletedSolveRevision : -1;
            Objectives = !Level ? "Load a campaign level"
                : manager.HasCurrentElectricalEvaluation
                    ? $"{manager.SatisfiedTargetCount}/{manager.ActiveTargetCount} objectives"
                    : "Objectives awaiting evaluation";
            Fault = !Level ? "" : manager.IsFailed ? manager.LastFailureReason
                : manager.HasCurrentElectricalEvaluation && (manager.SourceShorted || manager.TargetShorted || manager.Overloaded)
                    ? manager.LastEvaluationMessage : "";
            Telemetry = ReadTelemetry(selected, current, solver, Bindings);
        }

        private static string ReadTelemetry(Component selected, bool current, SparkElectricalSolver solver,
            SparkLevelSceneBindings bindings)
        {
            if (!selected) return "Select a component or terminal to inspect live data.";
            var meter = selected.GetComponentInParent<SparkMultimeter>();
            if (meter)
            {
                var reading = meter.CurrentReading;
                if (!meter.IsPowered) return "Multimeter off";
                string mode = $"{reading.Mode} / {reading.Range}";
                if (!current || !reading.Valid) return mode + "  ·  Reading unavailable";
                if (reading.OverRange) return mode + "  ·  Over range";
                if (reading.Mode == SparkMultimeterMode.Continuity)
                    return mode + (reading.Continuity ? "  ·  Closed path" : "  ·  Open path");
                return $"{mode}  ·  {reading.DisplayValue:G5} {reading.Unit}" + (reading.Stable ? "" : "  ·  Settling");
            }

            var terminal = selected.GetComponent<SparkTerminal>();
            if (terminal && current && solver.LatestSolveResult.TryGetTerminalVoltage(terminal, out float volts) && Finite(volts))
            {
                string label = bindings && bindings.TryGetTerminalId(terminal, out var id) ? id : "Selected terminal";
                return $"{label}  ·  {volts:G5} V (node reference)  ·  {terminal.SolvedPolarity}";
            }

            var electrical = selected.GetComponentInParent<SparkElectricalComponent>();
            if (!electrical || !electrical.ElectricalEnabled) return "Electrical data unavailable for this target.";
            string device = electrical switch
            {
                SparkResistor resistor => $"Rated {resistor.ResistanceOhms:G5} Ω",
                SparkPowerSupply supply => $"{supply.State}  ·  Setpoint {supply.OutputVoltage:G5} V  ·  Limit {supply.CurrentLimit:G5} A",
                SparkSwitch control => control.IsClosed ? "Switch closed" : "Switch open",
                SparkLED led => current ? led.IsOverCurrent ? "LED overcurrent" : led.IsReverseVoltageExceeded
                    ? "LED reverse limit exceeded" : led.IsOn ? "LED conducting" : "LED off"
                    : "LED state awaiting solve",
                _ => electrical.GetType().Name
            };
            var state = electrical.ElectricalState;
            if (!current || !solver.ElectricalComponents.Contains(electrical) || state.Conduction == SparkConductionState.Unknown
                || !Finite(state.Voltage) || !Finite(state.Current) || !Finite(state.Power))
                return device + "  ·  Solved values unavailable";
            return $"{device}  ·  Δ {state.Voltage:G5} V  ·  {state.Current:G5} A  ·  {state.Power:G5} W";
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    internal sealed class SparkNexusContext : IDisposable
    {
        private readonly SparkProExecutive executive;
        private readonly List<SparkMultimeter> meters = new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private SparkSelectionController selection;
        private SparkLevelSceneBindings bindings;
        private SparkEngineeringControlHub hub;
        private SparkEngineeringDeviceOperator devices;
        private Transform layout;
        private Component focus;
        private bool rebuildInventory = true;
        private bool dirty = true;
        private int revision;
        private int epoch;
        private int planStamp = -1;
        private int solveStamp = int.MinValue;
        private float nextFallback;
        private IReadOnlyList<NexusTarget> targets = Array.Empty<NexusTarget>();

        public NexusSnapshot Snapshot { get; private set; }
        public event Action Changed;

        public SparkNexusContext(SparkProExecutive executive) => this.executive = executive;

        public void Tick()
        {
            if (!executive) return;
            // A bounded reference check covers additive sessions rebinding the executive.
            if (Time.unscaledTime >= nextFallback || Snapshot == null)
            {
                nextFallback = Time.unscaledTime + 0.25f;
                if (executive.ActiveLevelManager != manager || executive.ActiveSelectionController != selection)
                    Rebind();
                if (planStamp != (manager && manager.ActiveLevel ? manager.ActiveLevel.GetInstanceID() : 0))
                    LevelChanged(manager ? manager.ActiveLevel : null);
                int stamp = solver && !solver.IsDirty && !solver.IsSolving ? solver.CompletedSolveRevision : -1;
                if (stamp != solveStamp) { solveStamp = stamp; dirty = true; }
            }

            if (!dirty) return;
            dirty = false;
            var active = loader ? loader.ActiveLayout : null;
            var current = active ? active.transform : null;
            if (current != layout)
            {
                layout = current; focus = null; epoch++; rebuildInventory = true;
                if (bindings) bindings.RuntimeBindingsChanged -= InventoryChanged;
                bindings = active ? active.SceneBindings : null;
                if (bindings) bindings.RuntimeBindingsChanged += InventoryChanged;
            }
            if (rebuildInventory) RebuildInventory();
            Component selected = focus ? focus : selection ? selection.SelectedObject : null;
            if (!layout || (selected && !selected.transform.IsChildOf(layout))) selected = null;
            Snapshot = new NexusSnapshot(++revision, epoch, manager, layout, targets, selected);
            Changed?.Invoke();
        }

        public void Focus(Component target)
        {
            if (focus == target) return;
            focus = target; dirty = true;
        }

        private void LevelChanged(SparkLevelDefinition definition)
        {
            planStamp = definition ? definition.GetInstanceID() : 0;
            epoch++; focus = null; InventoryChanged();
        }

        private void LevelFailed(SparkLevelDefinition definition, string reason) => Mark();
        private void LayoutLoaded(SparkLevelLayout _) => InventoryChanged();
        private void LayoutUnloaded() { epoch++; focus = null; InventoryChanged(); }
        private void Selected(SparkSelectable previous, SparkSelectable next) { focus = null; Mark(); }
        private void Reading(SparkMultimeterReading _) => Mark();
        private void Mark() => dirty = true;
        private void Evaluated(SparkLevelDefinition _) => Mark();
        private void InventoryChanged() { rebuildInventory = true; Mark(); }

        private void RebuildInventory()
        {
            rebuildInventory = false;
            foreach (var meter in meters) if (meter) meter.ReadingChanged -= Reading;
            meters.Clear();
            var found = new List<NexusTarget>();
            if (layout)
            {
                foreach (var item in layout.GetComponentsInChildren<SparkElectronicObject>(false))
                    found.Add(new NexusTarget(item, item.name + " · " + Kind(item), null));
                if (bindings)
                    foreach (var terminal in layout.GetComponentsInChildren<SparkTerminal>(false))
                        if (bindings.TryGetTerminalId(terminal, out string id))
                            found.Add(new NexusTarget(terminal, id + " · Terminal", id));
                foreach (var port in layout.GetComponentsInChildren<SparkEngineeringDevicePort>(false))
                    if (!string.IsNullOrWhiteSpace(port.DeviceId))
                        found.Add(new NexusTarget(port, port.DeviceId + " · " + port.Kind, port.DeviceId));
                foreach (var meter in layout.GetComponentsInChildren<SparkMultimeter>(false))
                {
                    found.Add(new NexusTarget(meter, meter.name + " · Multimeter", null));
                    meters.Add(meter);
                    meter.ReadingChanged += Reading;
                }
            }
            targets = Array.AsReadOnly(found.OrderBy(item => item.Label, StringComparer.OrdinalIgnoreCase).ToArray());
        }

        internal static string Kind(Component item) => item switch
        {
            SparkLED => "LED", SparkPowerSupply => "Power supply", SparkSwitch => "Switch",
            SparkResistor => "Resistor", SparkMultimeter => "Multimeter", SparkTerminal => "Terminal",
            SparkEngineeringDevicePort port => port.Kind.ToString(), _ => item.GetType().Name
        };

        private void Rebind()
        {
            Dispose();
            manager = executive.ActiveLevelManager;
            selection = executive.ActiveSelectionController;
            loader = manager ? manager.LayoutLoader : null;
            circuit = manager ? manager.CircuitSystem : null;
            solver = manager ? manager.ElectricalSolver : null;
            hub = executive.GetComponent<SparkEngineeringControlHub>();
            devices = executive.GetComponent<SparkEngineeringDeviceOperator>();
            if (manager)
            {
                manager.LevelStarted += LevelChanged;
                manager.LevelReset += LevelChanged;
                manager.LevelEvaluationChanged += Evaluated;
                manager.LevelCompleted += Evaluated;
                manager.LevelFailed += LevelFailed;
            }
            if (selection) selection.SelectionChanged += Selected;
            if (loader) { loader.LayoutLoaded += LayoutLoaded; loader.LayoutUnloaded += LayoutUnloaded; }
            if (circuit) circuit.TopologyChanged += Mark;
            if (solver) { solver.SolveCompleted += Mark; solver.SolveFailed += Mark; }
            if (hub) hub.StateChanged += Mark;
            if (devices) devices.StatusChanged += Mark;
            epoch++;
            InventoryChanged();
        }

        public void Dispose()
        {
            if (manager)
            {
                manager.LevelStarted -= LevelChanged;
                manager.LevelReset -= LevelChanged;
                manager.LevelEvaluationChanged -= Evaluated;
                manager.LevelCompleted -= Evaluated;
                manager.LevelFailed -= LevelFailed;
            }
            if (selection) selection.SelectionChanged -= Selected;
            if (loader) { loader.LayoutLoaded -= LayoutLoaded; loader.LayoutUnloaded -= LayoutUnloaded; }
            if (circuit) circuit.TopologyChanged -= Mark;
            if (solver) { solver.SolveCompleted -= Mark; solver.SolveFailed -= Mark; }
            if (bindings) bindings.RuntimeBindingsChanged -= InventoryChanged;
            if (hub) hub.StateChanged -= Mark;
            if (devices) devices.StatusChanged -= Mark;
            foreach (var meter in meters) if (meter) meter.ReadingChanged -= Reading;
            meters.Clear();
            bindings = null; layout = null; focus = null; manager = null; selection = null;
        }
    }
}

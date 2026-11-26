using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Real native-wire adapter for Project Spark's existing PLAYER SignalWireBuilder.
    /// There is NO LineRenderer, second cable prefab, or direct TryCreateConnection call here.
    /// SignalWireBuilder owns the same SignalPathMesh and SparkCircuitSystem registration
    /// that player input uses. SparkNativeWireDirector controls the staged motion.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAINativeSignalWirePort : SparkAINativeWirePort
    {
        [Header("Existing PLAYER wire tool (optional scene reference)")]
        [SerializeField] private SignalWireBuilder playerBuilder;
        [SerializeField] private SparkProExecutive executive;
        [SerializeField] private bool findActiveBuilderAutomatically = true;
        [Header("Native cable routing")]
        [SerializeField] private bool driveRealFreehandRoute = true;
        [SerializeField] private bool verbose;

        private readonly List<SparkCircuitConnection> connections = new List<SparkCircuitConnection>(8);
        private SignalWireBuilder engagedBuilder;
        private SparkTerminal source;
        private SparkTerminal destination;
        private SparkCircuitSystem authoritativeCircuit;
        private SignalPath realCablePath;
        private WireDrawingMode priorMode;
        private TerminalCableRouting priorRoute;
        private TerminalWireInputStyle priorInput;
        private bool priorExternalInput;
        private bool configurationCaptured;
        private bool active;
        private bool sourceAttached;
        private ulong lastAIConnectionId;
        private SparkCircuitSystem lastCircuit;
        private SignalWireBuilder lastBuilder;
        private string lastError = "Not initialized";

        public string LastError => lastError;
        public SignalWireBuilder ResolvedPlayerBuilder => engagedBuilder != null ? engagedBuilder : playerBuilder;
        public bool IsWiring => active;
        public ulong LastAIConnectionId => lastAIConnectionId;
        public override bool IsConfigured
        {
            get
            {
                SignalWireBuilder candidate;
                string error;
                return TryFindPlayerBuilder(out candidate, out error);
            }
        }

        private void Awake()
        {
            if (!executive) executive = GetComponent<SparkProExecutive>();
        }

        private void OnDisable()
        {
            if (active) Abort();
        }

        private bool TryFindPlayerBuilder(out SignalWireBuilder resolved, out string error)
        {
            resolved = null;
            error = null;
            SparkCircuitSystem expected = executive && executive.ActiveLevelManager
                ? executive.ActiveLevelManager.CircuitSystem : null;

            if (playerBuilder && playerBuilder.isActiveAndEnabled &&
                playerBuilder.gameObject.activeInHierarchy)
            {
                if (expected == null || playerBuilder.Circuit == null ||
                    playerBuilder.Circuit == expected)
                {
                    resolved = playerBuilder;
                    return true;
                }
            }

            if (!findActiveBuilderAutomatically)
            {
                error = "Assign the active player's SignalWireBuilder in the Inspector.";
                return false;
            }

            SignalWireBuilder[] all = FindObjectsByType<SignalWireBuilder>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            SignalWireBuilder only = null;
            int count = 0;
            for (int i = 0; i < all.Length; i++)
            {
                SignalWireBuilder candidate = all[i];
                if (!candidate || !candidate.isActiveAndEnabled || !candidate.gameObject.activeInHierarchy)
                    continue;
                if (expected != null && candidate.Circuit == expected)
                {
                    resolved = candidate;
                    return true;
                }
                if (expected != null && candidate.Circuit != null && candidate.Circuit != expected)
                    continue;
                count++;
                only = candidate;
            }
            if (count == 1)
            {
                resolved = only;
                return true;
            }
            error = count == 0
                ? "No active player SignalWireBuilder was found in this laboratory."
                : "Multiple player wire builders are active; assign the correct builder explicitly.";
            return false;
        }

        public override bool Begin(SparkTerminal a, SparkTerminal b, out string error)
        {
            error = null;
            if (active)
            {
                error = "Spark already holds a native wire.";
                return false;
            }
            if (!a || !b || a == b)
            {
                error = "Two different valid terminals are required.";
                return false;
            }
            if (!a.gameObject.activeInHierarchy || !b.gameObject.activeInHierarchy)
            {
                error = "One or both terminals are inactive.";
                return false;
            }
            if (!TryFindPlayerBuilder(out SignalWireBuilder candidate, out error))
            {
                lastError = error;
                return false;
            }
            if (candidate.IsDrawing || SignalWireBuilder.AnyWireDrawing)
            {
                error = "The player is already drawing a wire. Finish or cancel it first.";
                return false;
            }
            SparkCircuitSystem circuit = executive && executive.ActiveLevelManager
                ? executive.ActiveLevelManager.CircuitSystem : candidate.Circuit;
            if (!circuit)
            {
                error = "No authoritative circuit system is available for this level.";
                return false;
            }
            if (candidate.Circuit != null && candidate.Circuit != circuit)
            {
                error = "Player wire builder belongs to a different circuit.";
                return false;
            }
            if (circuit.HasConnectionBetween(a, b))
            {
                error = "These terminals are already connected.";
                return false;
            }
            if (a.Owner && b.Owner && a.Owner == b.Owner)
            {
                error = "Refusing a direct short across the same component.";
                return false;
            }
            if (!a.CanConnectTo(b, SparkConnectionKind.Wire,
                SparkConnectionDirection.Bidirectional, out error))
            {
                lastError = error;
                return false;
            }
            engagedBuilder = candidate;
            authoritativeCircuit = circuit;
            source = a;
            destination = b;
            priorMode = candidate.DrawingMode;
            priorRoute = candidate.CableRouting;
            priorInput = candidate.TerminalInputStyle;
            priorExternalInput = candidate.ExternalInputControl;
            configurationCaptured = true;
            active = true;
            sourceAttached = false;
            // Configure the SAME builder the player uses. No electrical change yet.
            candidate.SetWireDrawingMode(WireDrawingMode.TerminalToTerminal);
            if (driveRealFreehandRoute)
            {
                candidate.SetTerminalCableRouting(TerminalCableRouting.FreehandDrag);
                candidate.SetTerminalInputStyle(TerminalWireInputStyle.DragAndRelease);
            }
            candidate.SetExternalInputControl(true);
            if (verbose) Debug.Log("[Spark Native v10] Player wire builder reserved: " + candidate.name, this);
            return true;
        }

        public override bool AttachSource(SparkTerminal a, out string error)
        {
            error = null;
            if (!active || !engagedBuilder || a != source || sourceAttached)
            {
                error = "Invalid native wire source stage.";
                return false;
            }
            if (engagedBuilder.IsDrawing)
            {
                error = "Player wire builder was taken by another action.";
                return false;
            }
            if (!engagedBuilder.BeginWire(a, DeterminePolarity(a)))
            {
                error = "The real player's SignalWireBuilder refused to begin the cable. Check SignalPath_Manager and terminal capacity.";
                return false;
            }
            if (engagedBuilder.Circuit != authoritativeCircuit)
            {
                engagedBuilder.CancelWire();
                error = "The wire builder resolved a different circuit after starting; cable cancelled.";
                return false;
            }
            realCablePath = engagedBuilder.CurrentPath;
            if (!realCablePath)
            {
                engagedBuilder.CancelWire();
                error = "Player wire builder started but did not create a real SignalPath cable.";
                sourceAttached = false;
                return false;
            }
            sourceAttached = true;
            if (verbose) Debug.Log("[Spark Native v10] Real player cable started: " + a.name, this);
            return true;
        }

        public override bool MoveLooseEnd(Vector3 worldPosition, out string error)
        {
            error = null;
            if (!active || !engagedBuilder || !sourceAttached || !engagedBuilder.IsDrawing)
            {
                error = "No active player cable to move.";
                return false;
            }
            if (!IsFinite(worldPosition))
            {
                error = "Invalid wire endpoint coordinates.";
                return false;
            }
            // This updates the SAME SignalPathMesh that mouse dragging uses.
            engagedBuilder.UpdateWire(worldPosition);
            return true;
        }

        public override bool AttachDestination(SparkTerminal b, out string error)
        {
            error = null;
            if (!active || !sourceAttached || !engagedBuilder || b != destination ||
                !engagedBuilder.IsDrawing || !source || !authoritativeCircuit)
            {
                error = "Native destination is invalid or no cable is being drawn.";
                return false;
            }
            SignalWireBuilder builder = engagedBuilder;
            SparkCircuitSystem circuit = authoritativeCircuit;
            SparkTerminal a = source;
            // Finishes the real player cable mesh AND commits its electrical link.
            bool connected = builder.TryFinishTerminalWire(b);
            if (!connected)
            {
                error = "The player's Wire Tool rejected the destination. The preview was discarded.";
                RestoreBuilderSettings();
                ClearSession();
                return false;
            }
            circuit.GetConnections(a, connections);
            ulong createdId = 0;
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].Connects(a, b))
                {
                    createdId = connections[i].Id;
                    break;
                }
            }
            connections.Clear();
            if (createdId == 0 || !circuit.HasConnection(createdId))
            {
                error = "The real cable finished but the authoritative circuit could not verify its connection.";
                RestoreBuilderSettings();
                ClearSession();
                return false;
            }
            if (!realCablePath || !realCablePath.gameObject.activeInHierarchy)
            {
                // Never accept a graph-only result from the supposedly native player tool.
                builder.RemoveConnectedWire(createdId);
                error = "Electrical connection exists, but the native SignalPath cable is missing; rolled back.";
                RestoreBuilderSettings();
                ClearSession();
                return false;
            }
            lastAIConnectionId = createdId;
            lastCircuit = circuit;
            lastBuilder = builder;
            if (verbose) Debug.Log("[Spark Native v10] Real player wire complete, connection " + createdId, this);
            RestoreBuilderSettings();
            ClearSession();
            return true;
        }

        public override void Abort()
        {
            if (engagedBuilder && sourceAttached && engagedBuilder.IsDrawing)
                engagedBuilder.CancelWire(); // removes only the in-progress player cable.
            RestoreBuilderSettings();
            ClearSession();
        }

        public override bool UndoLastCompletedWire(out string error)
        {
            error = null;
            if (active)
            {
                error = "Cancel the active wire before undoing.";
                return false;
            }
            if (lastAIConnectionId == 0 || !lastCircuit || !lastBuilder)
            {
                error = "No confirmed Spark-created native wire is available to undo.";
                return false;
            }
            if (!lastCircuit.HasConnection(lastAIConnectionId))
            {
                error = "The last Spark-created wire has already been removed.";
                lastAIConnectionId = 0;
                return false;
            }
            // Removes both real cable mesh and registered topology via player's manager.
            bool removed = lastBuilder.RemoveConnectedWire(lastAIConnectionId);
            if (!removed)
            {
                SparkWireRegistry[] registries = FindObjectsByType<SparkWireRegistry>(
                    FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < registries.Length && !removed; i++)
                {
                    if (!registries[i] || !registries[i].PathManager ||
                        registries[i].PathManager.Circuit != lastCircuit) continue;
                    removed = registries[i].RemoveWire(lastAIConnectionId);
                }
            }
            if (!removed || lastCircuit.HasConnection(lastAIConnectionId))
            {
                error = "Player wire manager did not confirm removal of the exact Spark-created cable.";
                return false;
            }
            lastAIConnectionId = 0;
            lastBuilder = null;
            lastCircuit = null;
            return true;
        }

        private void RestoreBuilderSettings()
        {
            if (!configurationCaptured || !engagedBuilder) return;
            // Complete or cancel the temporary path before restoring player settings.
            if (engagedBuilder.IsDrawing) engagedBuilder.CancelWire();
            engagedBuilder.SetTerminalInputStyle(priorInput);
            engagedBuilder.SetTerminalCableRouting(priorRoute);
            engagedBuilder.SetWireDrawingMode(priorMode);
            engagedBuilder.SetExternalInputControl(priorExternalInput);
            configurationCaptured = false;
        }

        private void ClearSession()
        {
            active = false;
            sourceAttached = false;
            source = null;
            destination = null;
            authoritativeCircuit = null;
            realCablePath = null;
            engagedBuilder = null;
            configurationCaptured = false;
        }

        private static bool IsFinite(Vector3 p)
        {
            return !float.IsNaN(p.x) && !float.IsNaN(p.y) && !float.IsNaN(p.z) &&
                !float.IsInfinity(p.x) && !float.IsInfinity(p.y) && !float.IsInfinity(p.z);
        }

        private static WirePolarity DeterminePolarity(SparkTerminal terminal)
        {
            SparkElectricalComponent component = terminal.GetComponentInParent<SparkElectricalComponent>();
            if (component is SparkPowerSupply)
            {
                SparkTerminal[] terminals = component.GetComponentsInChildren<SparkTerminal>(true);
                if (terminals.Length > 0 && terminals[0] == terminal) return WirePolarity.Positive;
                if (terminals.Length > 1 && terminals[1] == terminal) return WirePolarity.Negative;
            }
            if (terminal.Polarity == SparkTerminalPolarity.Positive) return WirePolarity.Positive;
            if (terminal.Polarity == SparkTerminalPolarity.Negative) return WirePolarity.Negative;
            return WirePolarity.Positive;
        }
    }
}

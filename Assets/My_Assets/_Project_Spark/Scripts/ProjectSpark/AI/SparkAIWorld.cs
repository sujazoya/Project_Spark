using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Read-only observation layer for Spark AI.
    ///
    /// SparkAIWorld does not own Project Spark state and does not perform
    /// electrical simulation.
    ///
    /// Authoritative systems:
    ///     SparkCircuitSystem
    ///     SparkElectricalSolver
    ///     SparkTerminal
    ///     LevelGamePlayManager
    ///     SparkElectronicObject
    ///     SparkSelectionController
    ///
    /// SparkAIWorld converts those systems into a coherent AI snapshot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIWorld : MonoBehaviour
    {
        // =====================================================================
        // AUTHORITATIVE PROJECT SPARK SYSTEMS
        // =====================================================================

        [Header("Authoritative Systems")]

        [SerializeField]
        private SparkCircuitSystem circuitSystem;

        [SerializeField]
        private SparkElectricalSolver electricalSolver;

        [SerializeField]
        private LevelGamePlayManager levelGamePlayManager;

        [SerializeField]
        private SparkSelectionController selectionController;

        [Header("Player Activity")]
[SerializeField]
private SparkAIPlayerActivity playerActivity;

public SparkAIPlayerActivity PlayerActivity => playerActivity;

        // =====================================================================
        // WORLD OBSERVATION
        // =====================================================================

        [Header("Electronic Object Observation")]

        [Tooltip(
            "Optional root containing electronic objects visible to Spark AI.")]
        [SerializeField]
        private Transform electronicObjectRoot;

        [Tooltip(
            "Explicit electronic objects that Spark AI must observe. " +
            "Useful when objects are not under one common root.")]
        [SerializeField]
        private SparkElectronicObject[] explicitElectronicObjects =
            Array.Empty<SparkElectronicObject>();

        // =====================================================================
        // RUNTIME CACHE
        // =====================================================================

        private readonly List<SparkElectronicObject>
            electronicObjects =
                new List<SparkElectronicObject>(64);

        private readonly List<SparkTerminal>
            terminals =
                new List<SparkTerminal>(128);

        private readonly List<SparkCircuitConnection>
            connectionBuffer =
                new List<SparkCircuitConnection>(128);

        private readonly List<SparkAIConnectionSnapshot>
            connectionSnapshots =
                new List<SparkAIConnectionSnapshot>(128);

        private readonly List<SparkAIConnectionSnapshot>
            wireSnapshots =
                new List<SparkAIConnectionSnapshot>(64);

        private readonly List<SparkAIElectronicObjectSnapshot>
            objectSnapshots =
                new List<SparkAIElectronicObjectSnapshot>(64);

        private readonly List<SparkAITerminalSnapshot>
            terminalSnapshots =
                new List<SparkAITerminalSnapshot>(128);

        private bool initialized;
        private bool subscribed;
        private bool electricalObservationPending;

        private int observedTopologyVersion = -1;

        private SparkAIWorldSnapshot latestSnapshot;

        // =====================================================================
        // EVENTS
        // =====================================================================

        /// <summary>
        /// Raised when Spark AI becomes aware of a meaningful world change.
        /// </summary>
        public event EventHandler<SparkAIWorldChangedEventArgs>
            WorldChanged;

        // =====================================================================
        // PUBLIC STATE
        // =====================================================================

        /// <summary>
        /// Gets the latest coherent world snapshot.
        /// </summary>
        public SparkAIWorldSnapshot LatestSnapshot =>
            latestSnapshot;

        /// <summary>
        /// Gets whether the world observer has been initialized.
        /// </summary>
        public bool IsInitialized =>
            initialized;

        /// <summary>
        /// Gets the authoritative circuit system.
        /// </summary>
        public SparkCircuitSystem CircuitSystem =>
            circuitSystem;

        /// <summary>
        /// Gets the authoritative electrical solver.
        /// </summary>
        public SparkElectricalSolver ElectricalSolver =>
            electricalSolver;

        /// <summary>
        /// Gets the authoritative level manager.
        /// </summary>
        public LevelGamePlayManager LevelGamePlayManager =>
            levelGamePlayManager;

        // =====================================================================
        // UNITY
        // =====================================================================

        private void Awake()
        {
            ResolveRequiredReferences();

            RebuildObservationCache();

            Subscribe();

            CaptureSnapshotInternal();

            initialized = true;

            RaiseWorldChanged(
                new SparkAIEvent(
                    SparkAIEventType.WorldInitialized,
                    Time.time,
                    message: "Spark AI world observation initialized."));
        }

        private void OnEnable()
        {
            ResolveRequiredReferences();

            if (!initialized)
            {
                return;
            }

            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        // =====================================================================
        // PUBLIC SNAPSHOT API
        // =====================================================================

        /// <summary>
        /// Captures and returns one coherent authoritative world snapshot.
        ///
        /// This method performs no simulation and changes no Project Spark
        /// state.
        /// </summary>
        public SparkAIWorldSnapshot CaptureSnapshot()
        {
            if (!initialized)
            {
                ResolveRequiredReferences();
                RebuildObservationCache();
                Subscribe();

                initialized = true;
            }

            CaptureSnapshotInternal();

            return latestSnapshot;
        }

        /// <summary>
        /// Rebuilds the object and terminal observation cache.
        ///
        /// This is intended for scene/world structure changes, not per-frame
        /// execution.
        /// </summary>
      public void RebuildObservationCache()
{
    UnsubscribeFromTerminals();
    UnsubscribeFromElectricalComponents();

    electronicObjects.Clear();
    terminals.Clear();

    AddExplicitElectronicObjects();

    if (electronicObjectRoot != null)
    {
        SparkElectronicObject[] discoveredObjects =
            electronicObjectRoot
                .GetComponentsInChildren<SparkElectronicObject>(true);

        AddElectronicObjects(discoveredObjects);
    }

    // First collect terminals from known objects.
    AddTerminalsFromElectronicObjects();

    // This may discover additional connected owners.
    AddConnectedTerminals();

    // Collect terminals from any owners discovered by connections.
    AddTerminalsFromElectronicObjects();

    // Subscribe only after the cache is complete.
    SubscribeToTerminals();
    SubscribeToElectricalComponents();

    observedTopologyVersion =
        circuitSystem != null
            ? circuitSystem.TopologyVersion
            : -1;
}

        // =====================================================================
        // REFERENCES
        // =====================================================================

        private void ResolveRequiredReferences()
        {
            // Deliberately no FindObjectOfType / FindFirstObjectByType.
            //
            // Spark AI should be explicitly wired to the authoritative
            // Project Spark systems.
        }

        // =====================================================================
        // CACHE BUILD
        // =====================================================================

        private void AddExplicitElectronicObjects()
        {
            if (explicitElectronicObjects == null)
            {
                return;
            }

            for (int i = 0;
                 i < explicitElectronicObjects.Length;
                 i++)
            {
                AddElectronicObject(
                    explicitElectronicObjects[i]);
            }
        }

        private void AddElectronicObjects(
            SparkElectronicObject[] objects)
        {
            if (objects == null)
            {
                return;
            }

            for (int i = 0;
                 i < objects.Length;
                 i++)
            {
                AddElectronicObject(
                    objects[i]);
            }
        }

        private void AddElectronicObject(
            SparkElectronicObject electronicObject)
        {
            if (electronicObject == null)
            {
                return;
            }

            if (electronicObjects.Contains(
                    electronicObject))
            {
                return;
            }

            electronicObjects.Add(
                electronicObject);
        }

        private void AddTerminalsFromElectronicObjects()
        {
            for (int i = 0;
                 i < electronicObjects.Count;
                 i++)
            {
                SparkElectronicObject electronicObject =
                    electronicObjects[i];

                if (electronicObject == null)
                {
                    continue;
                }

                SparkTerminal[] childTerminals =
                    electronicObject
                        .GetComponentsInChildren<SparkTerminal>(
                            true);

                if (childTerminals == null)
                {
                    continue;
                }

                for (int t = 0;
                     t < childTerminals.Length;
                     t++)
                {
                    AddTerminal(
                        childTerminals[t]);
                }
            }
        }

        private void AddConnectedTerminals()
        {
            if (circuitSystem == null)
            {
                return;
            }

            connectionBuffer.Clear();

            circuitSystem.CopyConnections(
                connectionBuffer);

            for (int i = 0;
                 i < connectionBuffer.Count;
                 i++)
            {
                SparkCircuitConnection connection =
                    connectionBuffer[i];

                if (!connection.IsValid)
                {
                    continue;
                }

                AddTerminal(
                    connection.A);

                AddTerminal(
                    connection.B);

                AddElectronicObject(
                    connection.A != null
                        ? connection.A.Owner
                        : null);

                AddElectronicObject(
                    connection.B != null
                        ? connection.B.Owner
                        : null);
            }
        }

        private void AddTerminal(
            SparkTerminal terminal)
        {
            if (terminal == null)
            {
                return;
            }

            if (terminals.Contains(
                    terminal))
            {
                return;
            }

            terminals.Add(
                terminal);
        }

        // =====================================================================
        // EVENTS
        // =====================================================================

        private void Subscribe()
        {
            if (subscribed)
            {
                return;
            }

            if (circuitSystem != null)
            {
                circuitSystem.TopologyChanged +=
                    HandleTopologyChanged;

                circuitSystem.ConnectionCreated +=
                    HandleConnectionCreated;

                circuitSystem.ConnectionRemoved +=
                    HandleConnectionRemoved;
            }

            if (levelGamePlayManager != null)
            {
                levelGamePlayManager.LevelStarted +=
                    HandleLevelStarted;

                levelGamePlayManager.LevelCompleted +=
                    HandleLevelCompleted;

                levelGamePlayManager.LevelFailed +=
                    HandleLevelFailed;

                levelGamePlayManager.LevelEvaluationChanged +=
                    HandleLevelEvaluationChanged;
            }

            if (selectionController != null)
            {
                selectionController.SelectionChanged -=
                    HandleSelectionChanged;

                selectionController.SelectionChanged +=
                    HandleSelectionChanged;
            }

            SubscribeToTerminals();
            
            if (electricalSolver != null)
            {
                electricalSolver.SolveCompleted -=
                    HandleSolverSolveCompleted;

                electricalSolver.SolveCompleted +=
                    HandleSolverSolveCompleted;

                electricalSolver.SolveFailed -=
                    HandleSolverSolveFailed;

                electricalSolver.SolveFailed +=
                    HandleSolverSolveFailed;
            }


            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (circuitSystem != null)
            {
                circuitSystem.TopologyChanged -=
                    HandleTopologyChanged;

                circuitSystem.ConnectionCreated -=
                    HandleConnectionCreated;

                circuitSystem.ConnectionRemoved -=
                    HandleConnectionRemoved;
            }

            if (levelGamePlayManager != null)
            {
                levelGamePlayManager.LevelStarted -=
                    HandleLevelStarted;

                levelGamePlayManager.LevelCompleted -=
                    HandleLevelCompleted;

                levelGamePlayManager.LevelFailed -=
                    HandleLevelFailed;

                levelGamePlayManager.LevelEvaluationChanged -=
                    HandleLevelEvaluationChanged;
            }

            if (selectionController != null)
            {
                selectionController.SelectionChanged -=
                    HandleSelectionChanged;
            }

            
if (electricalSolver != null)
{
    electricalSolver.SolveCompleted -=
        HandleSolverSolveCompleted;

    electricalSolver.SolveFailed -=
        HandleSolverSolveFailed;
}


            UnsubscribeFromTerminals();

            subscribed = false;
        }

        private void SubscribeToTerminals()
        {
            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                SparkTerminal terminal =
                    terminals[i];

                if (terminal == null)
                {
                    continue;
                }

                terminal.ElectricalStateChanged -=
                    HandleTerminalElectricalStateChanged;

                terminal.ElectricalStateChanged +=
                    HandleTerminalElectricalStateChanged;

                terminal.ConnectionStateChanged -=
                    HandleTerminalConnectionStateChanged;

                terminal.ConnectionStateChanged +=
                    HandleTerminalConnectionStateChanged;
            }
        }

        private void UnsubscribeFromTerminals()
        {
            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                SparkTerminal terminal =
                    terminals[i];

                if (terminal == null)
                {
                    continue;
                }

                terminal.ElectricalStateChanged -=
                    HandleTerminalElectricalStateChanged;

                terminal.ConnectionStateChanged -=
                    HandleTerminalConnectionStateChanged;
            }
        }

        
private void SubscribeToElectricalComponents()
{
    for (int i = 0; i < electronicObjects.Count; i++)
    {
        SparkElectricalComponent component =
            electronicObjects[i] as SparkElectricalComponent;

        if (component == null)
            continue;

        component.ElectricalStateChanged -=
            HandleComponentElectricalStateChanged;

        component.ElectricalStateChanged +=
            HandleComponentElectricalStateChanged;
    }
}

private void UnsubscribeFromElectricalComponents()
{
    for (int i = 0; i < electronicObjects.Count; i++)
    {
        SparkElectricalComponent component =
            electronicObjects[i] as SparkElectricalComponent;

        if (component == null)
            continue;

        component.ElectricalStateChanged -=
            HandleComponentElectricalStateChanged;
    }
}
private void HandleTerminalElectricalStateChanged(
    SparkTerminal terminal)
{
    if (terminal == null)
        return;

    if (electricalSolver != null &&
        electricalSolver.IsSolving)
    {
        electricalObservationPending = true;
        return;
    }

    RaiseWorldChanged(
        new SparkAIEvent(
            SparkAIEventType.ElectricalStateChanged,
            Time.time,
            terminal: terminal,
            message:
                "An observed terminal electrical state changed."));
}

private void HandleComponentElectricalStateChanged(
    SparkElectricalState state)
{
    if (electricalSolver != null &&
        electricalSolver.IsSolving)
    {
        electricalObservationPending = true;
        return;
    }

    RaiseWorldChanged(
        new SparkAIEvent(
            SparkAIEventType.ElectricalStateChanged,
            Time.time,
            message:
                "An observed component electrical state changed."));
}

private void HandleSolverSolveCompleted()
{
    electricalObservationPending = false;

    RaiseWorldChanged(
        new SparkAIEvent(
            SparkAIEventType.ElectricalStateChanged,
            Time.time,
            message:
                "Electrical solve completed; publishing the final observation."));
}

private void HandleSolverSolveFailed()
{
    electricalObservationPending = false;

    RaiseWorldChanged(
        new SparkAIEvent(
            SparkAIEventType.ElectricalStateChanged,
            Time.time,
            message:
                "Electrical solve failed; publishing the resulting observation."));
}



        // =====================================================================
        // CIRCUIT EVENTS
        // =====================================================================

       
        // =====================================================================
        // CIRCUIT EVENTS
        // =====================================================================

        private void HandleTopologyChanged()
        {
            RebuildObservationCache();

            PublishWorldChangeWhenSafe(
                new SparkAIEvent(
                    SparkAIEventType.CircuitChanged,
                    Time.time,
                    message:
                        "Authoritative circuit topology changed."));
        }

        private void HandleConnectionCreated(
            SparkCircuitConnection connection)
        {
            if (!connection.IsValid)
            {
                return;
            }

            AddTerminal(connection.A);
            AddTerminal(connection.B);

            AddElectronicObject(
                connection.A != null
                    ? connection.A.Owner
                    : null);

            AddElectronicObject(
                connection.B != null
                    ? connection.B.Owner
                    : null);

            // Include newly discovered owners and their terminals.
            AddTerminalsFromElectronicObjects();

            SubscribeToTerminals();
            SubscribeToElectricalComponents();

            PublishWorldChangeWhenSafe(
                new SparkAIEvent(
                    SparkAIEventType.TerminalConnected,
                    Time.time,
                    connection,
                    terminal: connection.A,
                    message:
                        "A Project Spark circuit connection was created."));
        }

        private void HandleConnectionRemoved(
            SparkCircuitConnection connection)
        {
            PublishWorldChangeWhenSafe(
                new SparkAIEvent(
                    SparkAIEventType.TerminalDisconnected,
                    Time.time,
                    connection,
                    terminal: connection.A,
                    message:
                        "A Project Spark circuit connection was removed."));
        }





private void PublishWorldChangeWhenSafe(
    SparkAIEvent @event)
{
    if (electricalSolver != null &&
        electricalSolver.IsSolving)
    {
        electricalObservationPending = true;
        return;
    }

    RaiseWorldChanged(@event);
}
        // =====================================================================
        // ELECTRICAL EVENTS
        // =====================================================================    


     
        private void HandleTerminalConnectionStateChanged(
            SparkTerminal terminal)
        {
            if (terminal == null)
            {
                return;
            }

            PublishWorldChangeWhenSafe(
                new SparkAIEvent(
                    SparkAIEventType.CircuitChanged,
                    Time.time,
                    terminal: terminal,
                    message:
                        "An observed terminal connection state changed."));
        }

        // =====================================================================
        // LEVEL EVENTS
        // =====================================================================

        private void HandleLevelStarted(
            SparkLevelDefinition level)
        {
            PublishWorldChangeWhenSafe(
                new SparkAIEvent(
                    SparkAIEventType.LevelStarted,
                    Time.time,
                    level: level,
                    message:
                        "Project Spark level started."));
        }

        private void HandleLevelCompleted(
            SparkLevelDefinition level)
        {
            PublishWorldChangeWhenSafe(
                new SparkAIEvent(
                    SparkAIEventType.LevelCompleted,
                    Time.time,
                    level: level,
                    message:
                        "Project Spark level completed."));
        }

        private void HandleLevelFailed(
            SparkLevelDefinition level,
            string reason)
        {
            PublishWorldChangeWhenSafe(
                new SparkAIEvent(
                    SparkAIEventType.LevelFailed,
                    Time.time,
                    level: level,
                    message: reason));
        }

        private void HandleLevelEvaluationChanged(
            SparkLevelDefinition level)
        {
            PublishWorldChangeWhenSafe(
                new SparkAIEvent(
                    SparkAIEventType.LevelEvaluationChanged,
                    Time.time,
                    level: level,
                    message:
                        "Level evaluation changed."));
        }

        // =====================================================================
        // SELECTION
        // =====================================================================

        private void HandleSelectionChanged(
            SparkSelectable previousSelection,
            SparkSelectable newSelection)
        {
            SparkAIEventType eventType =
                newSelection != null
                    ? SparkAIEventType.ObjectSelected
                    : SparkAIEventType.ObjectDeselected;

            PublishWorldChangeWhenSafe(
                new SparkAIEvent(
                    eventType,
                    Time.time,
                    electronicObject: null,
                    message:
                        newSelection != null
                            ? $"Player selected: {newSelection.name}"
                            : "Player cleared the selection."));
        }

        // =====================================================================
        // SNAPSHOT
        // =====================================================================

        private void CaptureSnapshotInternal()
        {
            BuildObjectSnapshots();
            BuildTerminalSnapshots();
            BuildConnectionSnapshots();

            SparkAILevelSnapshot levelSnapshot =
                BuildLevelSnapshot();

            SparkAIPlayerSnapshot playerSnapshot =
                BuildPlayerSnapshot();

            string situation =
                BuildCurrentSituation(
                    levelSnapshot);

            latestSnapshot =
                new SparkAIWorldSnapshot(
                    Time.time,
                    levelSnapshot,
                    playerSnapshot,
                    objectSnapshots.ToArray(),
                    terminalSnapshots.ToArray(),
                    connectionSnapshots.ToArray(),
                    wireSnapshots.ToArray(),
                    circuitSystem != null
                        ? circuitSystem.TopologyVersion
                        : -1,
                    circuitSystem != null,
                    electricalSolver != null,
                    situation);
        }

       private void BuildObjectSnapshots()
{
    objectSnapshots.Clear();

    for (int i = 0;
         i < electronicObjects.Count;
         i++)
    {
        SparkElectronicObject objectReference =
            electronicObjects[i];

        if (objectReference == null)
        {
            continue;
        }

        SparkElectricalComponent electrical =
            objectReference as SparkElectricalComponent;

        bool isElectrical =
            electrical != null;

        float voltage = 0f;
        float current = 0f;
        float power = 0f;

        string conduction = string.Empty;

        bool electricalEnabled = false;

        if (electrical != null)
        {
            electricalEnabled =
                electrical.ElectricalEnabled;

            SparkElectricalState state =
                electrical.ElectricalState;

            voltage = state.Voltage;
            current = state.Current;
            power = state.Power;

            conduction =
                state.Conduction.ToString();
        }

        // ============================================================
        // LED TERMINAL REFERENCES
        // ============================================================

        int anodeTerminalId = 0;
        int cathodeTerminalId = 0;

        SparkLED led =
            objectReference as SparkLED;

        if (led != null)
        {
            if (led.AnodeTerminal != null)
            {
                anodeTerminalId =
                    led.AnodeTerminal.GetInstanceID();
            }

            if (led.CathodeTerminal != null)
            {
                cathodeTerminalId =
                    led.CathodeTerminal.GetInstanceID();
            }
        }

        // ============================================================
        // CREATE OBJECT SNAPSHOT
        // ============================================================

        objectSnapshots.Add(
            new SparkAIElectronicObjectSnapshot(
                objectReference.GetInstanceID(),
                objectReference.name,
                objectReference.isActiveAndEnabled,
                objectReference.InteractionsEnabled,
                objectReference.OperationalState.ToString(),
                isElectrical,
                electricalEnabled,
                voltage,
                current,
                power,
                conduction,
                anodeTerminalId,
                cathodeTerminalId));
    }
}


        private void BuildTerminalSnapshots()
        {
            terminalSnapshots.Clear();

            for (int i = 0;
                 i < terminals.Count;
                 i++)
            {
                SparkTerminal terminal =
                    terminals[i];

                if (terminal == null)
                {
                    continue;
                }

                SparkElectronicObject owner =
                    terminal.Owner;

                terminalSnapshots.Add(
                    new SparkAITerminalSnapshot(
                        terminal.GetInstanceID(),
                        owner != null
                            ? owner.GetInstanceID()
                            : 0,
                        terminal.name,
                        owner != null
                            ? owner.name
                            : string.Empty,
                        terminal.Kind.ToString(),
                        terminal.Polarity.ToString(),
                        terminal.EffectivePolarity.ToString(),
                        terminal.Voltage,
                        terminal.Current,
                        terminal.ElectricalState.Power,
                        terminal.IsPowered,
                        terminal.HasCurrent,
                        terminal.IsElectricalEnabled,
                        terminal.ConnectionCount,
                        terminal.MaxConnections,
                        terminal.AtCapacity));
            }
        }

        private void BuildConnectionSnapshots()
        {
            connectionSnapshots.Clear();
            wireSnapshots.Clear();

            if (circuitSystem == null)
            {
                return;
            }

            connectionBuffer.Clear();

            circuitSystem.CopyConnections(
                connectionBuffer);


               /* Debug.Log(
    "[SPARK AI CONNECTION SOURCE] " +
    "CopiedConnections=" + connectionBuffer.Count +
    " | CircuitSystem=" + circuitSystem.name);*/

            for (int i = 0;
                 i < connectionBuffer.Count;
                 i++)
            {
                SparkCircuitConnection connection =
                    connectionBuffer[i];

                if (!connection.IsValid)
                {
                    continue;
                }

                SparkTerminal a =
                    connection.A;

                SparkTerminal b =
                    connection.B;

                if (a == null || b == null)
                {
                    continue;
                }

                SparkAIConnectionSnapshot snapshot =
                    new SparkAIConnectionSnapshot(
                        connection.Id,
                        a.GetInstanceID(),
                        b.GetInstanceID(),
                        a.name,
                        b.name,
                        connection.Kind.ToString(),
                        connection.Direction.ToString(),
                        connection.IsValid);

                connectionSnapshots.Add(
                    snapshot);

                   /* Debug.Log(
    "[SPARK AI CAPTURED CONNECTION] " +
    "ID=" + snapshot.Id +
    " | A=" + snapshot.SourceTerminalName +
    " (" + snapshot.SourceTerminalInstanceId + ")" +
    " | B=" + snapshot.TargetTerminalName +
    " (" + snapshot.TargetTerminalInstanceId + ")" +
    " | Kind=" + snapshot.Kind +
    " | Valid=" + snapshot.Valid);*/

                if (connection.Kind ==
                    SparkConnectionKind.Wire)
                {
                    wireSnapshots.Add(
                        snapshot);
                }
            }
        }

        // =====================================================================
        // LEVEL SNAPSHOT
        // =====================================================================

       private SparkAILevelSnapshot BuildLevelSnapshot()
{
    if (levelGamePlayManager == null)
    {
        return new SparkAILevelSnapshot(
            false,
            -1,
            string.Empty,
            string.Empty,
            string.Empty,
            "Unavailable",
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            0,
            0f,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            0f,
            0f,
            0f,
            string.Empty);
    }

    SparkLevelDefinition level =
        levelGamePlayManager.ActiveLevel;

    SparkLevelValidationResult validation =
        levelGamePlayManager.ValidationResult;

    int totalTargets =
        levelGamePlayManager.ActiveTargetCount;

    int satisfiedTargets =
        levelGamePlayManager.SatisfiedTargetCount;

    float progress =
        totalTargets > 0
            ? Mathf.Clamp01(
                (float)satisfiedTargets /
                totalTargets)
            : 0f;

    return new SparkAILevelSnapshot(
        level != null,
        levelGamePlayManager.ActiveLevelIndex,

        level != null
            ? level.LevelId
            : string.Empty,

        level != null
            ? level.DisplayName
            : string.Empty,

        level != null
            ? level.Description
            : string.Empty,

        levelGamePlayManager.RuntimeState.ToString(),

        validation.state.ToString(),

        levelGamePlayManager.LastEvaluationMessage,

        levelGamePlayManager.LastFailureReason,

        satisfiedTargets,
        totalTargets,
        progress,

        levelGamePlayManager.IsCompleted,
        levelGamePlayManager.IsFailed,

        levelGamePlayManager.HasValidPowerSource,
        levelGamePlayManager.ClosedReturn,
        levelGamePlayManager.SourceShorted,
        levelGamePlayManager.TargetShorted,
        levelGamePlayManager.WrongConnection,

        // LevelGamePlayManager currently does not expose
        // an IsOverloaded property.
        false,

        levelGamePlayManager.TargetVoltage,
        levelGamePlayManager.TargetCurrent,
        levelGamePlayManager.TargetPower,

        levelGamePlayManager.ActiveSourceName);
}

        // =====================================================================
        // PLAYER SNAPSHOT
        // =====================================================================

      private SparkAIPlayerSnapshot BuildPlayerSnapshot()
{
    bool hasSelection = false;
    string selectedObjectName = string.Empty;
    int selectedObjectInstanceId = 0;
    string activeTool = string.Empty;

    if (selectionController != null)
    {
        SparkSelectable selected =
            selectionController.SelectedObject;

        hasSelection = selected != null;

        if (selected != null)
        {
            selectedObjectName = selected.name;
            selectedObjectInstanceId =
                selected.GetInstanceID();
        }
    }

    SparkAIPlayerActivitySnapshot activity =
        playerActivity != null
            ? playerActivity.CurrentActivity
            : SparkAIPlayerActivitySnapshot.CreateIdle(Time.time);

    return new SparkAIPlayerSnapshot(
        hasSelection,
        selectedObjectName,
        selectedObjectInstanceId,
        activeTool,
        activity);
}

        // =====================================================================
        // SITUATION
        // =====================================================================

        private string BuildCurrentSituation(
            SparkAILevelSnapshot level)
        {
            if (!level.HasLevel)
            {
                return "No active Project Spark level is available.";
            }

            if (level.Failed)
            {
                if (!string.IsNullOrWhiteSpace(
                        level.FailureReason))
                {
                    return level.FailureReason;
                }

                return "The active level is currently failed.";
            }

            if (level.Completed)
            {
                return "The active level is complete.";
            }

            if (level.WrongConnection)
            {
                return "The current circuit contains an incorrect connection.";
            }

            if (level.SourceShorted)
            {
                return "The power source is currently shorted.";
            }

            if (level.TargetShorted)
            {
                return "The level target is currently shorted.";
            }

            if (level.Overloaded)
            {
                return "The current circuit is overloaded.";
            }

            if (!level.HasValidPowerSource)
            {
                return "No valid active power source is currently available.";
            }

            if (level.TotalTargets > 0 &&
                level.SatisfiedTargets <
                level.TotalTargets)
            {
                return
                    $"The level is in progress. " +
                    $"{level.SatisfiedTargets} of " +
                    $"{level.TotalTargets} targets are satisfied.";
            }

            return "The level is active and awaiting player progress.";
        }

        // =====================================================================
        // WORLD EVENT
        // =====================================================================

        private void RaiseWorldChanged(
            SparkAIEvent @event)
        {
            /*
             * The snapshot is refreshed when a meaningful event occurs.
             *
             * Electrical terminal events are therefore represented by the
             * latest authoritative terminal values, not copied values from
             * the event itself.
             */

            CaptureSnapshotInternal();

            WorldChanged?.Invoke(
                this,
                new SparkAIWorldChangedEventArgs(
                    @event));
        }
    }
}
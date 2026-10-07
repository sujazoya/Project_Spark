using System;
using System.Collections.Generic;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Stable snapshot of the authoritative Project Spark world.
    ///
    /// A snapshot represents one coherent observation of the world.
    /// It is suitable for deterministic reasoning and future language-model
    /// context generation.
    /// </summary>
    public sealed class SparkAIWorldSnapshot
    {
        /// <summary>
        /// Time at which the snapshot was captured.
        /// </summary>
        public float CapturedAt { get; }

        /// <summary>
        /// Active level state.
        /// </summary>
        public SparkAILevelSnapshot Level { get; }

        /// <summary>
        /// Current player observation.
        /// </summary>
        public SparkAIPlayerSnapshot Player { get; }

        /// <summary>
        /// Observed electronic objects.
        /// </summary>
        public IReadOnlyList<SparkAIElectronicObjectSnapshot>
            ElectronicObjects { get; }

        /// <summary>
        /// Observed terminals.
        /// </summary>
        public IReadOnlyList<SparkAITerminalSnapshot>
            Terminals { get; }

        /// <summary>
        /// Current circuit connections.
        /// </summary>
        public IReadOnlyList<SparkAIConnectionSnapshot>
            Connections { get; }

        /// <summary>
        /// Current wire connections.
        ///
        /// A wire is represented from the authoritative circuit connection
        /// when its connection kind is Wire.
        /// </summary>
        public IReadOnlyList<SparkAIConnectionSnapshot>
            Wires { get; }

        /// <summary>
        /// Current circuit topology version.
        /// </summary>
        public int TopologyVersion { get; }

        /// <summary>
        /// Whether the circuit system is available.
        /// </summary>
        public bool CircuitAvailable { get; }

        /// <summary>
        /// Whether the electrical solver is available.
        /// </summary>
        public bool ElectricalSolverAvailable { get; }

        /// <summary>
        /// Human-readable current situation.
        /// </summary>
        public string CurrentSituation { get; }

        /// <summary>
        /// Creates a complete world snapshot.
        /// </summary>
        public SparkAIWorldSnapshot(
            float capturedAt,
            SparkAILevelSnapshot level,
            SparkAIPlayerSnapshot player,
            SparkAIElectronicObjectSnapshot[] electronicObjects,
            SparkAITerminalSnapshot[] terminals,
            SparkAIConnectionSnapshot[] connections,
            SparkAIConnectionSnapshot[] wires,
            int topologyVersion,
            bool circuitAvailable,
            bool electricalSolverAvailable,
            string currentSituation)
        {
            CapturedAt = capturedAt;
            Level = level;
            Player = player;

            ElectronicObjects =
                CopyArray(electronicObjects);

            Terminals =
                CopyArray(terminals);

            Connections =
                CopyArray(connections);

            Wires =
                CopyArray(wires);

            TopologyVersion = topologyVersion;
            CircuitAvailable = circuitAvailable;
            ElectricalSolverAvailable =
                electricalSolverAvailable;

            CurrentSituation =
                currentSituation ?? string.Empty;
        }

        private static T[] CopyArray<T>(
            T[] source)
        {
            if (source == null ||
                source.Length == 0)
            {
                return Array.Empty<T>();
            }

            T[] copy =
                new T[source.Length];

            Array.Copy(
                source,
                copy,
                source.Length);

            return copy;
        }
    }

    /// <summary>
    /// Snapshot of the active Project Spark level.
    /// </summary>
    public readonly struct SparkAILevelSnapshot
    {
        public bool HasLevel { get; }

        public int LevelIndex { get; }

        public string LevelId { get; }

        public string LevelName { get; }

        public string Description { get; }

        public string RuntimeState { get; }

        public string ValidationState { get; }

        public string EvaluationMessage { get; }

        public string FailureReason { get; }

        public int SatisfiedTargets { get; }

        public int TotalTargets { get; }

        public float Progress01 { get; }

        public bool Completed { get; }

        public bool Failed { get; }

        public bool HasValidPowerSource { get; }

        public bool ClosedReturn { get; }

        public bool SourceShorted { get; }

        public bool TargetShorted { get; }

        public bool WrongConnection { get; }

        public bool Overloaded { get; }

        public float TargetVoltage { get; }

        public float TargetCurrent { get; }

        public float TargetPower { get; }

        public string ActiveSourceName { get; }

        public SparkAILevelSnapshot(
            bool hasLevel,
            int levelIndex,
            string levelId,
            string levelName,
            string description,
            string runtimeState,
            string validationState,
            string evaluationMessage,
            string failureReason,
            int satisfiedTargets,
            int totalTargets,
            float progress01,
            bool completed,
            bool failed,
            bool hasValidPowerSource,
            bool closedReturn,
            bool sourceShorted,
            bool targetShorted,
            bool wrongConnection,
            bool overloaded,
            float targetVoltage,
            float targetCurrent,
            float targetPower,
            string activeSourceName)
        {
            HasLevel = hasLevel;
            LevelIndex = levelIndex;
            LevelId = levelId ?? string.Empty;
            LevelName = levelName ?? string.Empty;
            Description = description ?? string.Empty;
            RuntimeState = runtimeState ?? string.Empty;
            ValidationState = validationState ?? string.Empty;
            EvaluationMessage =
                evaluationMessage ?? string.Empty;
            FailureReason =
                failureReason ?? string.Empty;

            SatisfiedTargets = satisfiedTargets;
            TotalTargets = totalTargets;
            Progress01 = progress01;

            Completed = completed;
            Failed = failed;

            HasValidPowerSource =
                hasValidPowerSource;

            ClosedReturn =
                closedReturn;

            SourceShorted =
                sourceShorted;

            TargetShorted =
                targetShorted;

            WrongConnection =
                wrongConnection;

            Overloaded =
                overloaded;

            TargetVoltage = targetVoltage;
            TargetCurrent = targetCurrent;
            TargetPower = targetPower;

            ActiveSourceName =
                activeSourceName ?? string.Empty;
        }
    }

    /// <summary>
    /// Snapshot of player-facing interaction state.
    /// </summary>
    [Serializable]
public readonly struct SparkAIPlayerSnapshot
{
    public bool HasSelection { get; }
    public string SelectedObjectName { get; }
    public int SelectedObjectInstanceId { get; }
    public string ActiveTool { get; }

    public SparkAIPlayerActivitySnapshot LatestActivity { get; }

    public SparkAIPlayerSnapshot(
        bool hasSelection,
        string selectedObjectName,
        int selectedObjectInstanceId,
        string activeTool,
        SparkAIPlayerActivitySnapshot latestActivity)
    {
        HasSelection = hasSelection;
        SelectedObjectName = selectedObjectName ?? string.Empty;
        SelectedObjectInstanceId = selectedObjectInstanceId;
        ActiveTool = activeTool ?? string.Empty;
        LatestActivity = latestActivity;
    }
}

    /// <summary>
    /// Snapshot of an electronic Project Spark object.
    /// </summary>
    public readonly struct SparkAIElectronicObjectSnapshot
    {
        public int InstanceId { get; }

        public string Name { get; }

        public bool Active { get; }

        public bool InteractionsEnabled { get; }

        public string OperationalState { get; }

        public bool IsElectricalComponent { get; }

        public bool ElectricalEnabled { get; }

        public float Voltage { get; }

        public float Current { get; }

        public float Power { get; }

        public string ConductionState { get; }

        public SparkAIElectronicObjectSnapshot(
            int instanceId,
            string name,
            bool active,
            bool interactionsEnabled,
            string operationalState,
            bool isElectricalComponent,
            bool electricalEnabled,
            float voltage,
            float current,
            float power,
            string conductionState)
        {
            InstanceId = instanceId;
            Name = name ?? string.Empty;
            Active = active;
            InteractionsEnabled =
                interactionsEnabled;

            OperationalState =
                operationalState ?? string.Empty;

            IsElectricalComponent =
                isElectricalComponent;

            ElectricalEnabled =
                electricalEnabled;

            Voltage = voltage;
            Current = current;
            Power = power;

            ConductionState =
                conductionState ?? string.Empty;
        }
    }

    

    /// <summary>
    /// Snapshot of one Spark terminal.
    /// </summary>
    public readonly struct SparkAITerminalSnapshot
    {
        public int InstanceId { get; }

        public int OwnerInstanceId { get; }

        public string Name { get; }

        public string OwnerName { get; }

        public string Kind { get; }

        public string Polarity { get; }

        public string EffectivePolarity { get; }

        public float Voltage { get; }

        public float Current { get; }

        public float Power { get; }

        public bool Powered { get; }

        public bool HasCurrent { get; }

        public bool ElectricalEnabled { get; }

        public int ConnectionCount { get; }

        public int MaxConnections { get; }

        public bool AtCapacity { get; }

        public SparkAITerminalSnapshot(
            int instanceId,
            int ownerInstanceId,
            string name,
            string ownerName,
            string kind,
            string polarity,
            string effectivePolarity,
            float voltage,
            float current,
            float power,
            bool powered,
            bool hasCurrent,
            bool electricalEnabled,
            int connectionCount,
            int maxConnections,
            bool atCapacity)
        {
            InstanceId = instanceId;
            OwnerInstanceId = ownerInstanceId;
            Name = name ?? string.Empty;
            OwnerName = ownerName ?? string.Empty;
            Kind = kind ?? string.Empty;
            Polarity = polarity ?? string.Empty;
            EffectivePolarity =
                effectivePolarity ?? string.Empty;

            Voltage = voltage;
            Current = current;
            Power = power;

            Powered = powered;
            HasCurrent = hasCurrent;
            ElectricalEnabled =
                electricalEnabled;

            ConnectionCount =
                connectionCount;

            MaxConnections =
                maxConnections;

            AtCapacity =
                atCapacity;
        }
    }

    /// <summary>
    /// Snapshot of an authoritative Spark circuit connection.
    /// </summary>
    public readonly struct SparkAIConnectionSnapshot
    {
        public ulong Id { get; }

        public int SourceTerminalInstanceId { get; }

        public int TargetTerminalInstanceId { get; }

        public string SourceTerminalName { get; }

        public string TargetTerminalName { get; }

        public string Kind { get; }

        public string Direction { get; }

        public bool Valid { get; }

        public SparkAIConnectionSnapshot(
            ulong id,
            int sourceTerminalInstanceId,
            int targetTerminalInstanceId,
            string sourceTerminalName,
            string targetTerminalName,
            string kind,
            string direction,
            bool valid)
        {
            Id = id;

            SourceTerminalInstanceId =
                sourceTerminalInstanceId;

            TargetTerminalInstanceId =
                targetTerminalInstanceId;

            SourceTerminalName =
                sourceTerminalName ?? string.Empty;

            TargetTerminalName =
                targetTerminalName ?? string.Empty;

            Kind =
                kind ?? string.Empty;

            Direction =
                direction ?? string.Empty;

            Valid = valid;
        }
    }
}
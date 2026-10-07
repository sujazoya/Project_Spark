using System;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Events emitted by the Spark AI observation layer.
    ///
    /// These events are notifications only.
    /// They never directly modify authoritative Project Spark systems.
    /// </summary>
    public enum SparkAIEventType
    {
        WorldInitialized = 0,

        LevelStarted = 1,
        LevelCompleted = 2,
        LevelFailed = 3,
        LevelEvaluationChanged = 4,

        ObjectSelected = 5,
        ObjectDeselected = 6,

        TerminalConnected = 7,
        TerminalDisconnected = 8,

        CircuitChanged = 9,
        ElectricalStateChanged = 10,
        DeviceStateChanged = 11,

        WorldStructureChanged = 12
    }

    /// <summary>
    /// Immutable description of an event observed by Spark AI.
    ///
    /// SparkCircuitConnection is a value type in Project Spark,
    /// therefore it cannot use null as a default parameter.
    /// </summary>
    public readonly struct SparkAIEvent
    {
        public SparkAIEventType Type { get; }

        public float Time { get; }

        public SparkElectronicObject ElectronicObject { get; }

        public SparkTerminal Terminal { get; }

        public SparkCircuitConnection Connection { get; }

        public bool HasConnection { get; }

        public SparkLevelDefinition Level { get; }

        public string Message { get; }

        public SparkAIEvent(
            SparkAIEventType type,
            float time,
            SparkElectronicObject electronicObject = null,
            SparkTerminal terminal = null,
            SparkLevelDefinition level = null,
            string message = null)
        {
            Type = type;
            Time = time;

            ElectronicObject = electronicObject;
            Terminal = terminal;

            Connection = default;
            HasConnection = false;

            Level = level;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// Constructor for events associated with a circuit connection.
        /// </summary>
        public SparkAIEvent(
            SparkAIEventType type,
            float time,
            SparkCircuitConnection connection,
            SparkTerminal terminal = null,
            SparkElectronicObject electronicObject = null,
            SparkLevelDefinition level = null,
            string message = null)
        {
            Type = type;
            Time = time;

            ElectronicObject = electronicObject;
            Terminal = terminal;

            Connection = connection;
            HasConnection = true;

            Level = level;
            Message = message ?? string.Empty;
        }
    }

    /// <summary>
    /// Arguments raised when the authoritative AI world observation changes.
    /// </summary>
    public sealed class SparkAIWorldChangedEventArgs : EventArgs
    {
        public SparkAIEvent Event { get; }

        public SparkAIWorldChangedEventArgs(
            SparkAIEvent @event)
        {
            Event = @event;
        }
    }

    /// <summary>
    /// Arguments raised when Spark AI memory receives
    /// a new observation.
    /// </summary>
    public sealed class SparkAIMemoryChangedEventArgs : EventArgs
    {
        public SparkAIMemoryEntry Entry { get; }

        public SparkAIMemoryChangedEventArgs(
            SparkAIMemoryEntry entry)
        {
            Entry = entry;
        }
    }
}
using System;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Observes meaningful player activity for Spark AI.
    ///
    /// This component does not perform gameplay actions.
    /// It only records the latest known player activity so the
    /// reasoning/director layers can understand player intent.
    ///
    /// Authoritative gameplay systems remain unchanged.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIPlayerActivity : MonoBehaviour
    {
        [Header("Authoritative Systems")]
        [SerializeField] private SparkSelectionController selectionController;
        [SerializeField] private SparkCircuitSystem circuitSystem;

        [Header("Observation")]
        [SerializeField]
        private bool observeSelection = true;

        [SerializeField]
        private bool observeCircuitChanges = true;

        private SparkAIPlayerActivitySnapshot currentActivity;

        private bool initialized;

        public SparkAIPlayerActivitySnapshot CurrentActivity =>
            currentActivity;

        public bool IsInitialized => initialized;

        public event EventHandler<SparkAIPlayerActivityChangedEventArgs>
            ActivityChanged;

        private void Awake()
        {
            ResolveReferences();

            Subscribe();

            initialized = true;

            currentActivity =
                SparkAIPlayerActivitySnapshot.CreateIdle(Time.time);
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void ResolveReferences()
        {
            // Explicit references only.
            //
            // No FindObjectOfType.
            // No FindFirstObjectByType.
        }

        private void Subscribe()
        {
            if (selectionController != null && observeSelection)
            {
                selectionController.SelectionChanged -=
                    HandleSelectionChanged;

                selectionController.SelectionChanged +=
                    HandleSelectionChanged;
            }

            if (circuitSystem != null && observeCircuitChanges)
            {
                circuitSystem.ConnectionCreated -=
                    HandleConnectionCreated;

                circuitSystem.ConnectionCreated +=
                    HandleConnectionCreated;

                circuitSystem.ConnectionRemoved -=
                    HandleConnectionRemoved;

                circuitSystem.ConnectionRemoved +=
                    HandleConnectionRemoved;
            }
        }

        private void Unsubscribe()
        {
            if (selectionController != null)
            {
                selectionController.SelectionChanged -=
                    HandleSelectionChanged;
            }

            if (circuitSystem != null)
            {
                circuitSystem.ConnectionCreated -=
                    HandleConnectionCreated;

                circuitSystem.ConnectionRemoved -=
                    HandleConnectionRemoved;
            }
        }

        /// <summary>
        /// Manually records an observed player activity.
        ///
        /// This is useful for future tools such as:
        /// - multimeter
        /// - oscilloscope
        /// - wire placement
        /// - component placement
        /// - switch interaction
        /// </summary>
        public void RecordActivity(
            SparkAIPlayerActivityType type,
            string description,
            int primaryObjectInstanceId = 0,
            int secondaryObjectInstanceId = 0)
        {
            SetActivity(
                new SparkAIPlayerActivitySnapshot(
                    type,
                    description,
                    primaryObjectInstanceId,
                    secondaryObjectInstanceId,
                    Time.time));
        }

        public void RecordToolActivity(
            string toolName,
            string description)
        {
            SetActivity(
                new SparkAIPlayerActivitySnapshot(
                    SparkAIPlayerActivityType.ToolUsed,
                    description,
                    0,
                    0,
                    Time.time,
                    toolName));
        }

        public void RecordMeasurement(
            string measurementType,
            string description,
            int measuredObjectInstanceId = 0)
        {
            SetActivity(
                new SparkAIPlayerActivitySnapshot(
                    SparkAIPlayerActivityType.MeasurementTaken,
                    description,
                    measuredObjectInstanceId,
                    0,
                    Time.time,
                    measurementType));
        }

        private void HandleSelectionChanged(
            SparkSelectable previousSelection,
            SparkSelectable newSelection)
        {
            if (newSelection != null)
            {
                SetActivity(
                    new SparkAIPlayerActivitySnapshot(
                        SparkAIPlayerActivityType.ObjectSelected,
                        $"Player selected {newSelection.name}.",
                        newSelection.GetInstanceID(),
                        0,
                        Time.time));
            }
            else
            {
                SetActivity(
                    new SparkAIPlayerActivitySnapshot(
                        SparkAIPlayerActivityType.ObjectDeselected,
                        "Player cleared the current selection.",
                        0,
                        0,
                        Time.time));
            }
        }

        private void HandleConnectionCreated(
            SparkCircuitConnection connection)
        {
            if (!connection.IsValid)
                return;

            int objectA =
                connection.A != null
                    ? connection.A.GetInstanceID()
                    : 0;

            int objectB =
                connection.B != null
                    ? connection.B.GetInstanceID()
                    : 0;

            SetActivity(
                new SparkAIPlayerActivitySnapshot(
                    SparkAIPlayerActivityType.ConnectionCreated,
                    "Player created a circuit connection.",
                    objectA,
                    objectB,
                    Time.time));
        }

        private void HandleConnectionRemoved(
            SparkCircuitConnection connection)
        {
            int objectA =
                connection.A != null
                    ? connection.A.GetInstanceID()
                    : 0;

            int objectB =
                connection.B != null
                    ? connection.B.GetInstanceID()
                    : 0;

            SetActivity(
                new SparkAIPlayerActivitySnapshot(
                    SparkAIPlayerActivityType.ConnectionRemoved,
                    "Player removed a circuit connection.",
                    objectA,
                    objectB,
                    Time.time));
        }

        private void SetActivity(
            SparkAIPlayerActivitySnapshot activity)
        {
            currentActivity = activity;

            ActivityChanged?.Invoke(
                this,
                new SparkAIPlayerActivityChangedEventArgs(
                    activity));
        }
    }

    /// <summary>
    /// Type of meaningful player activity observed by Spark AI.
    /// </summary>
    public enum SparkAIPlayerActivityType
    {
        None = 0,

        ObjectSelected = 1,
        ObjectDeselected = 2,

        ConnectionCreated = 3,
        ConnectionRemoved = 4,

        ToolUsed = 5,
        MeasurementTaken = 6,

        ComponentInteracted = 7,
        SwitchChanged = 8,

        ComponentPlaced = 9,
        ComponentMoved = 10,

        ComponentRotated = 11
    }

    /// <summary>
    /// Immutable snapshot of the player's latest meaningful activity.
    /// </summary>
    public readonly struct SparkAIPlayerActivitySnapshot
    {
        public SparkAIPlayerActivityType Type { get; }

        public string Description { get; }

        public int PrimaryObjectInstanceId { get; }

        public int SecondaryObjectInstanceId { get; }

        public float Time { get; }

        public string Context { get; }

        public SparkAIPlayerActivitySnapshot(
            SparkAIPlayerActivityType type,
            string description,
            int primaryObjectInstanceId,
            int secondaryObjectInstanceId,
            float time,
            string context = null)
        {
            Type = type;
            Description = description ?? string.Empty;

            PrimaryObjectInstanceId =
                primaryObjectInstanceId;

            SecondaryObjectInstanceId =
                secondaryObjectInstanceId;

            Time = time;

            Context = context ?? string.Empty;
        }

        public static SparkAIPlayerActivitySnapshot CreateIdle(
            float time)
        {
            return new SparkAIPlayerActivitySnapshot(
                SparkAIPlayerActivityType.None,
                "No recent player activity.",
                0,
                0,
                time);
        }
    }

    /// <summary>
    /// Event arguments for player activity changes.
    /// </summary>
    public sealed class SparkAIPlayerActivityChangedEventArgs :
        EventArgs
    {
        public SparkAIPlayerActivitySnapshot Activity { get; }

        public SparkAIPlayerActivityChangedEventArgs(
            SparkAIPlayerActivitySnapshot activity)
        {
            Activity = activity;
        }
    }
}
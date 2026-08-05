namespace ProjectSpark.Core.Events
{
    public readonly struct GameStartedEvent : IEvent
    {
    }

    public readonly struct GamePausedEvent : IEvent
    {
    }

    public readonly struct GameResumedEvent : IEvent
    {
    }

    public readonly struct LevelLoadedEvent : IEvent
    {
        public readonly int Level;

        public LevelLoadedEvent(int level)
        {
            Level = level;
        }
    }

    public readonly struct LevelCompletedEvent : IEvent
    {
        public readonly int Level;

        public LevelCompletedEvent(int level)
        {
            Level = level;
        }
    }

    public readonly struct ObjectiveCompletedEvent : IEvent
    {
        public readonly string ObjectiveId;

        public ObjectiveCompletedEvent(string objectiveId)
        {
            ObjectiveId = objectiveId;
        }
    }

    public readonly struct SimulationStartedEvent : IEvent
    {
    }

    public readonly struct SimulationFinishedEvent : IEvent
    {
    }

    public readonly struct ComponentSelectedEvent : IEvent
    {
        public readonly string ComponentId;

        public ComponentSelectedEvent(string componentId)
        {
            ComponentId = componentId;
        }
    }

    public readonly struct WireConnectedEvent : IEvent
    {
        public readonly int WireId;

        public WireConnectedEvent(int wireId)
        {
            WireId = wireId;
        }
    }
}

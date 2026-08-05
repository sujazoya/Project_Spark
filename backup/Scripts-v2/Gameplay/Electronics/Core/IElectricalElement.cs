namespace ProjectSpark.Gameplay.Electronics
{
    /// <summary>
    /// Any object that participates in the electrical simulation.
    /// </summary>
    public interface IElectricalElement
    {
        void ResetState();

        void Simulate();
    }
}

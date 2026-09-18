namespace ProjectSpark.Gameplay
{
    public interface ISparkConductiveDevice
    {
        bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to);
    }
}
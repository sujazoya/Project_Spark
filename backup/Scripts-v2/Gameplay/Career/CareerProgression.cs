namespace ProjectSpark.Gameplay.Career
{
    public sealed class CareerProgression
    {
        public int Level
        {
            get;
            private set;
        } = 1;

        public int Experience
        {
            get;
            private set;
        }

        public void AddXP(
            int amount)
        {
            Experience += amount;

            while (Experience >= Level * 500)
            {
                Experience -= Level * 500;
                Level++;
            }
        }
    }
}

namespace ProjectSpark.Gameplay.Workshop
{
    public sealed class WorkshopLevel
    {
        public int Level { get; private set; } = 1;

        public int XP { get; private set; }

        public void AddXP(int amount)
        {
            XP += amount;

            while(XP >= Level * 1000)
            {
                XP -= Level * 1000;

                Level++;
            }
        }
    }
}

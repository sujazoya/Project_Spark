namespace ProjectSpark.Gameplay.Workshop
{
    public sealed class WorkshopState
    {
        public Wallet Wallet =
            new();

        public WorkshopLevel Level =
            new();

        public UpgradeManager Upgrades =
            new();

        public SkillTree Skills =
            new();
    }
}

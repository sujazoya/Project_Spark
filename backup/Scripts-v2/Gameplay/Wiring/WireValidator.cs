namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireValidator
    {
        public bool CanConnect(
            WirePin a,
            WirePin b)
        {
            if (a == null || b == null)
                return false;

            if (a == b)
                return false;

            return true;
        }
    }
}

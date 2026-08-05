namespace ProjectSpark.Gameplay.Placement
{
    public sealed class PlacementValidator
    {
        public bool Validate(
            PlacementSession session)
        {
            return session.State != PlacementState.Invalid;
        }
    }
}

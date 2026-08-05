namespace ProjectSpark.Gameplay.Placement
{
    public sealed class RotationValidator
    {
        public bool CanRotate(
            float angle)
        {
            return angle % 90f == 0f;
        }
    }
}

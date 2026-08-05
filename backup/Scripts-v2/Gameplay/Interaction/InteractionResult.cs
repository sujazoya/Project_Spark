namespace ProjectSpark.Gameplay.Interaction
{
    public readonly struct InteractionResult
    {
        public readonly bool Success;

        public readonly string Message;

        public InteractionResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static InteractionResult Successful()
            => new(true, string.Empty);

        public static InteractionResult Failed(string message)
            => new(false, message);
    }
}

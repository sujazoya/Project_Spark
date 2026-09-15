namespace ProjectSpark.Gameplay
{
    public interface ISparkInteractable
    {
        bool CanInteract(in SparkInteractionContext context, out string reason);
        SparkResult BeginInteraction(in SparkInteractionContext context);
        SparkResult UpdateInteraction(in SparkInteractionContext context);
        SparkResult EndInteraction(in SparkInteractionContext context);
        SparkResult CancelInteraction(in SparkInteractionContext context);
    }

    public interface ISparkMovable
    {
        bool CanMove(in SparkInteractionContext context, out string reason);
        SparkResult BeginMove(in SparkInteractionContext context);
        SparkResult UpdateMove(in SparkInteractionContext context);
        SparkResult EndMove(in SparkInteractionContext context);
        SparkResult CancelMove(in SparkInteractionContext context);
    }

    public interface ISparkRotatable
    {
        bool CanRotate(in SparkInteractionContext context, out string reason);
        SparkResult BeginRotate(in SparkInteractionContext context);
        SparkResult UpdateRotate(in SparkInteractionContext context);
        SparkResult EndRotate(in SparkInteractionContext context);
        SparkResult CancelRotate(in SparkInteractionContext context);
    }
}

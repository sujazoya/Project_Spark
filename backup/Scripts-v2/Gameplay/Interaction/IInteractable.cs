namespace ProjectSpark.Gameplay.Interaction
{
    public interface IInteractable
    {
        bool CanInteract { get; }

        void HoverEnter();

        void HoverExit();

        void Select();

        void Deselect();

        void Interact();
    }
}

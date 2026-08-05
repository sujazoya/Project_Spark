namespace ProjectSpark.Gameplay.Interaction
{
    public sealed class HoverController
    {
        private IInteractable _current;

        public void Set(IInteractable target)
        {
            if (_current == target)
                return;

            _current?.HoverExit();

            _current = target;

            _current?.HoverEnter();
        }

        public void Clear()
        {
            _current?.HoverExit();
            _current = null;
        }
    }
}

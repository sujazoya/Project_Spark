using UnityEngine;

namespace ProjectSpark.Gameplay.Interaction
{
    public sealed class HoverManager : MonoBehaviour
    {
        public IInteractable Current { get; private set; }

        public void SetHover(IInteractable target)
        {
            if (Current == target)
                return;

            Current?.HoverExit();

            Current = target;

            Current?.HoverEnter();
        }
    }
}

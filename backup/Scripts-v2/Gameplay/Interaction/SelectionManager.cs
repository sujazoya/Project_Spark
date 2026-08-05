using UnityEngine;

namespace ProjectSpark.Gameplay.Interaction
{
    public sealed class SelectionManager : MonoBehaviour
    {
        public IInteractable Selected { get; private set; }

        public void Select(IInteractable target)
        {
            if (Selected == target)
                return;

            Selected?.Deselect();

            Selected = target;

            Selected?.Select();
        }

        public void ClearSelection()
        {
            Selected?.Deselect();

            Selected = null;
        }
    }
}

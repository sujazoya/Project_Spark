// ============================================================================
// ScrewInteractable.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class ScrewInteractable : Interactable
    {
        [SerializeField]
        Transform screw;

        [SerializeField]
        float rotateSpeed = 1080;

        [SerializeField]
        float liftDistance = .025f;

        bool removed;

        public override void Interact()
        {
            if (removed)
                return;

            if (!ToolManager.Instance.IsSelected(
                ToolType.Screwdriver))
                return;

            removed = true;

            screw.Rotate(
                Vector3.forward,
                rotateSpeed);

            screw.position +=
                Vector3.up * liftDistance;
        }
    }
}
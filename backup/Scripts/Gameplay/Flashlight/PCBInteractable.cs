// ============================================================================
// PCBInteractable.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class PCBInteractable : Interactable
    {
        [SerializeField]
        InspectionCamera inspection;

        public override void Interact()
        {
            inspection.Focus(transform);
        }
    }
}
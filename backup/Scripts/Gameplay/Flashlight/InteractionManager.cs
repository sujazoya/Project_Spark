// ============================================================================
// InteractionManager.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class InteractionManager : MonoBehaviour
    {
        [SerializeField]
        UnityEngine.Camera gameplayCamera;

        [SerializeField]
        LayerMask interactionMask;

        void Update()
        {
            if (!UnityEngine.Input.GetMouseButtonDown(0))
                return;

            Ray ray =
                gameplayCamera.ScreenPointToRay(
                   UnityEngine.Input.mousePosition);

            if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                100,
                interactionMask))
                return;

            Interactable interactable =
                hit.collider.GetComponentInParent<Interactable>();

            if (interactable == null)
                return;

            if (!interactable.CanInteract())
                return;

            interactable.Interact();
        }
    }
}
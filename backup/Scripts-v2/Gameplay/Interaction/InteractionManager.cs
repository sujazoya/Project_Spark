using UnityEngine;

namespace ProjectSpark.Gameplay.Interaction
{
    public sealed class InteractionManager : MonoBehaviour
    {
        [SerializeField]
       private UnityEngine.Camera Camera;

        [SerializeField]
        private LayerMask interactionMask;

        private readonly InteractionRaycaster raycaster =
            new();

        private void Update()
        {
            Vector2 mouse =
               UnityEngine.Input.mousePosition;

            if (!raycaster.TryRaycast(
                Camera,
                mouse,
                out RaycastHit hit))
                return;

            if (((1 << hit.collider.gameObject.layer)
                & interactionMask.value) == 0)
                return;

            if (hit.collider.TryGetComponent<IInteractable>(
                out var interactable))
            {
                interactable.Interact();
            }
        }
    }
}

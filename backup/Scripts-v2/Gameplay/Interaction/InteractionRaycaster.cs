using UnityEngine;

namespace ProjectSpark.Gameplay.Interaction
{
    public sealed class InteractionRaycaster
    {
        public bool TryRaycast(
             UnityEngine.Camera camera,
            Vector2 screenPosition,
            out RaycastHit hit)
        {
            Ray ray =
                camera.ScreenPointToRay(screenPosition);

            return Physics.Raycast(
                ray,
                out hit,
                500f);
        }
    }
}

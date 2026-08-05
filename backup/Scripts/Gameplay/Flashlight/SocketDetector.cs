// ============================================================================
// SocketDetector.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class SocketDetector : MonoBehaviour
    {
        [SerializeField]
        float snapDistance = .02f;

        void OnMouseUp()
        {
            ComponentController component =
                GetComponent<ComponentController>();

            ComponentSocket[] sockets =
                FindObjectsByType<ComponentSocket>(
                    FindObjectsSortMode.None);

            foreach (ComponentSocket socket in sockets)
            {
                if (socket.Occupied)
                    continue;

                if (socket.ComponentId != component.ComponentId)
                    continue;

                if (Vector3.Distance(
                    transform.position,
                    socket.SnapPoint.position) >
                    snapDistance)
                    continue;

                socket.Place(component);

                break;
            }
        }
    }
}
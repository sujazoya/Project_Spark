// ============================================================================
// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Flashlight/ComponentSocket.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class ComponentSocket : MonoBehaviour
    {
        [SerializeField] string componentId;

        [SerializeField] Transform snapPoint;

        public string ComponentId => componentId;

        public Transform SnapPoint => snapPoint;

        public bool Occupied { get; private set; }

        public void Place(ComponentController component)
        {
            component.transform.SetPositionAndRotation(
                snapPoint.position,
                snapPoint.rotation);

            component.transform.SetParent(transform);

            component.SetPlaced();

            Occupied = true;
        }
    }
}
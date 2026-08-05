// ============================================================================
// ComponentController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    [RequireComponent(typeof(Collider))]
    public sealed class ComponentController : MonoBehaviour
    {
        [SerializeField] string componentId;

        UnityEngine.Camera cam;

        bool dragging;

        Plane plane;

        public string ComponentId => componentId;

        public bool IsPlaced { get; private set; }

        void Awake()
        {
            cam = UnityEngine.Camera.main;

            plane = new Plane(Vector3.up, Vector3.zero);
        }

        void OnMouseDown()
        {
            if (IsPlaced)
                return;

            dragging = true;
        }

        void OnMouseUp()
        {
            dragging = false;
        }

        void Update()
        {
            if (!dragging)
                return;

            Ray ray =
                cam.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (plane.Raycast(ray, out float enter))
            {
                transform.position =
                    ray.GetPoint(enter);
            }
        }

        public void SetPlaced()
        {
            IsPlaced = true;
        }
    }
}
// ============================================================================
// DragToolController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class DragToolController : MonoBehaviour
    {
        [SerializeField]
        float distance = .45f;

        UnityEngine.Camera cam;

        bool dragging;

        void Awake()
        {
            cam =   UnityEngine.Camera.main;
        }

        void OnMouseDown()
        {
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
                cam.ScreenPointToRay(
                    UnityEngine.Input.mousePosition);

            transform.position =
                ray.origin +
                ray.direction * distance;
        }
    }
}
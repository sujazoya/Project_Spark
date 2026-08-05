// ============================================================================
// ProbeController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class ProbeController : MonoBehaviour
    {
        [SerializeField]
        LineRenderer line;

        [SerializeField]
        Transform tip;

        UnityEngine.Camera cam;

        void Awake()
        {
            cam = UnityEngine.Camera.main;

            line.positionCount = 2;
        }

        void Update()
        {
            Ray ray =
                cam.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                tip.position = hit.point;
            }

            line.SetPosition(0, transform.position);
            line.SetPosition(1, tip.position);
        }
    }
}
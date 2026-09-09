using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicSnapTest
        : MonoBehaviour
    {
        [SerializeField]
        private Camera viewerCamera;

        [SerializeField]
        private HolographicSnapController controller;

        [SerializeField]
        private HolographicSnapMarker marker;

        private void Update()
        {
            if (viewerCamera == null ||
                controller == null ||
                marker == null)
            {
                return;
            }

            Vector2 mouse =
                Input.mousePosition;

            Ray ray =
                viewerCamera.ScreenPointToRay(
                    mouse
                );

            if (Physics.Raycast(
                    ray,
                    out RaycastHit hit))
            {
                HolographicSnapResult result =
                    controller.FindSnap(
                        mouse,
                        hit
                    );

                marker.Show(result);

                return;
            }

            marker.Hide();
        }
    }
}
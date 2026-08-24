
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicViewerUI : MonoBehaviour
    {
        [SerializeField] private HolographicViewerCamera viewerCamera;
        [SerializeField] private HolographicObjectController objectController;

        public void Rotate()
        {
            objectController.ToggleAutoRotate();
        }

        public void ZoomIn()
        {
            viewerCamera.ZoomIn();
        }

        public void ZoomOut()
        {
            viewerCamera.ZoomOut();
        }

        public void ResetView()
        {
            objectController.ResetView();
            viewerCamera.ResetView();
        }
    }
}
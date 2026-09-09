
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicViewerUI : MonoBehaviour
    {
        [SerializeField] private HolographicViewerCamera viewerCamera;
        [SerializeField] private HolographicObjectController objectController;
        [SerializeField]
private HolographicObjectVisualState visualState;

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
        public void SetNormal()
            {
                visualState.SetMode(0);
            }

            public void SetXRay()
            {
                visualState.SetMode(1);
            }

            public void SetInternal()
            {
                visualState.SetMode(2);
            }

            public void SetExploded()
            {
                visualState.SetMode(3);
            }

            public void SetWireframe()
            {
                visualState.SetMode(4);
       }
    }
}
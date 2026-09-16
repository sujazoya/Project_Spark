using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicViewerUI : MonoBehaviour
    {
        [SerializeField] private HolographicViewerCamera viewerCamera;
        [SerializeField] private HolographicObjectController objectController;
        [SerializeField] private HolographicObjectVisualState visualState;

        public void ZoomIn()
        {
            if (viewerCamera != null)
                viewerCamera.ZoomIn();
        }

        public void ZoomOut()
        {
            if (viewerCamera != null)
                viewerCamera.ZoomOut();
        }

        public void ResetView()
        {
            if (viewerCamera != null)
                viewerCamera.ResetView();

            if (objectController != null)
                objectController.ResetRotation();
        }

        public void SetNormal()
        {
            if (visualState != null)
                visualState.SetMode(0);
        }

        public void SetXRay()
        {
            if (visualState != null)
                visualState.SetMode(1);
        }

        public void SetInternal()
        {
            if (visualState != null)
                visualState.SetMode(2);
        }

        public void SetExploded()
        {
            if (visualState != null)
                visualState.SetMode(3);
        }

        public void SetWireframe()
        {
            if (visualState != null)
                visualState.SetMode(4);
        }
    }
}
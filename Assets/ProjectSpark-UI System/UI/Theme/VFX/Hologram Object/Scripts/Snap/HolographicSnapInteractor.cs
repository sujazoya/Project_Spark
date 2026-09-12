using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    [DisallowMultipleComponent]
    public sealed class HolographicSnapInteractor
        : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private Camera viewerCamera;

        [SerializeField]
        private HolographicSnapController snapController;

        [SerializeField]
        private HolographicSnapMarker snapMarker;

        [Header("Raycast")]

        [SerializeField]
        private LayerMask snapLayer;

        [SerializeField]
        private float rayDistance = 100f;

        [Header("State")]

        [SerializeField]
        private bool snapEnabled = true;

        public bool IsSnapEnabled =>
            snapEnabled;

        public HolographicSnapResult CurrentSnap =>
            currentSnap;

        private HolographicSnapResult currentSnap;

        private void Awake()
        {
            if (viewerCamera == null)
                viewerCamera = Camera.main;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            UpdateSnap();
#endif
        }

#if ENABLE_INPUT_SYSTEM

        private void UpdateSnap()
        {
            if (!snapEnabled)
            {
                HideMarker();
                return;
            }

            if (viewerCamera == null)
            {
                HideMarker();
                return;
            }

            if (Mouse.current == null)
            {
                HideMarker();
                return;
            }

            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            Ray ray =
                viewerCamera.ScreenPointToRay(
                    mousePosition
                );

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    rayDistance,
                    snapLayer,
                    QueryTriggerInteraction.Ignore))
            {
                ClearCurrentSnap();
                return;
            }

            if (snapController == null)
            {
                ClearCurrentSnap();
                return;
            }

            HolographicSnapResult result =
                snapController.FindSnap(
                    mousePosition,
                    hit
                );

            if (!result.Valid)
            {
                ClearCurrentSnap();
                return;
            }

            currentSnap = result;

            if (snapMarker != null)
                snapMarker.Show(result);
        }

#endif

        public HolographicSnapResult
            GetCurrentSnap()
        {
            return currentSnap;
        }

        public void StartSnapping()
        {
            snapEnabled = true;
        }

        public void StopSnapping()
        {
            snapEnabled = false;

            ClearCurrentSnap();
        }

        public void ToggleSnapping()
        {
            if (snapEnabled)
                StopSnapping();
            else
                StartSnapping();
        }

        private void ClearCurrentSnap()
        {
            currentSnap =
                HolographicSnapResult.Invalid;

            HideMarker();
        }

        private void HideMarker()
        {
            if (snapMarker != null)
                snapMarker.Hide();
        }
    }
}
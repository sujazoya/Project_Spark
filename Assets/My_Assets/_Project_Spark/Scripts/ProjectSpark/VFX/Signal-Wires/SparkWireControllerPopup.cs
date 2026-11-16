
using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkWireControllerPopup : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private GameObject popupPanel;

        [Header("Wire Manager")]
        [SerializeField]
        private SignalPath_Manager pathManager;

        [Header("Camera")]
        [SerializeField]
        private Camera gameplayCamera;

        [Header("Visibility")]
        [SerializeField]
        private bool hideWhenNoWireIsOnScreen = true;

        [SerializeField, Min(0.02f)]
        private float checkInterval = 0.12f;

        [SerializeField, Min(0f)]
        private float hideDelay = 0.25f;

        [Header("Debug")]
        [SerializeField]
        private bool logStateChanges = false;

        private float nextCheckTime;
        private float lastVisibleTime;
        private bool panelVisible;

        private readonly Plane[] cameraPlanes =
            new Plane[6];

        private void Awake()
        {
            ResolveReferences();

            lastVisibleTime = float.NegativeInfinity;

            SetPanelVisible(false);
        }

        private void OnEnable()
        {
            nextCheckTime = 0f;
            lastVisibleTime = float.NegativeInfinity;

            ResolveReferences();
            RefreshNow();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextCheckTime)
                return;

            nextCheckTime =
                Time.unscaledTime + checkInterval;

            RefreshNow();
        }

        private void ResolveReferences()
        {
            if (pathManager == null)
            {
                pathManager =
                    FindFirstObjectByType<
                        SignalPath_Manager>();
            }

            if (gameplayCamera == null)
            {
                gameplayCamera = Camera.main;
            }
        }

        public void RefreshNow()
        {
            if (popupPanel == null)
                return;

            if (pathManager == null)
                ResolveReferences();

            bool hasVisibleWire =
                HasVisiblePlayerWire();

            if (hasVisibleWire)
            {
                lastVisibleTime = Time.unscaledTime;

                SetPanelVisible(true);
                return;
            }

            // Hide immediately after the last wire is
            // removed, instead of leaving an empty panel.
            if (pathManager == null ||
                !pathManager.HasWires)
            {
                lastVisibleTime =
                    float.NegativeInfinity;

                SetPanelVisible(false);
                return;
            }

            // Preserve the panel briefly when a wire
            // is temporarily outside the camera view.
            if (Time.unscaledTime - lastVisibleTime
                >= hideDelay)
            {
                SetPanelVisible(false);
            }
        }

        private bool HasVisiblePlayerWire()
        {
            if (pathManager == null)
                return false;

            if (!pathManager.HasWires)
                return false;

            if (!hideWhenNoWireIsOnScreen)
                return true;

            if (gameplayCamera == null)
                gameplayCamera = Camera.main;

            if (gameplayCamera == null)
                return false;

            GeometryUtility.CalculateFrustumPlanes(
                gameplayCamera,
                cameraPlanes);

            SignalPath[] paths = pathManager.Paths;

            if (paths == null)
                return false;

            foreach (SignalPath path in paths)
            {
                if (path == null ||
                    !path.gameObject.activeInHierarchy)
                    continue;

                // Keep the toolbar visible while
                // actively drawing a new path.
                if (path == pathManager.CurrentPath)
                    return true;

                Renderer[] renderers =
                    path.GetComponentsInChildren<
                        Renderer>(false);

                foreach (Renderer renderer in renderers)
                {
                    if (renderer == null ||
                        !renderer.enabled)
                        continue;

                    if (GeometryUtility.TestPlanesAABB(
                        cameraPlanes,
                        renderer.bounds))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SetPanelVisible(bool visible)
        {
            if (popupPanel == null)
                return;

            if (panelVisible == visible &&
                popupPanel.activeSelf == visible)
                return;

            panelVisible = visible;

            popupPanel.SetActive(visible);

            if (logStateChanges)
            {
                int wireCount =
                    pathManager != null
                        ? pathManager.Count
                        : 0;

                Debug.Log(
                    "[WIRE POPUP] " +
                    (visible ? "SHOW" : "HIDE") +
                    " | Manager wires=" + wireCount,
                    this);
            }
        }

        private void OnDisable()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            panelVisible = false;
        }
    }
}

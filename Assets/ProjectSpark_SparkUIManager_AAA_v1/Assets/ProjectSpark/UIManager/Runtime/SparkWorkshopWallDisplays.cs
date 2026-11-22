using System.Collections.Generic;
using System.Linq;
using AAAUI.VFX;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Cameras;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UIManagement
{
    internal sealed class SparkWorkshopWallDisplays : MonoBehaviour
    {
        private Canvas left;
        private Canvas right;
        private CinemachineCamera leftCamera;
        private CinemachineCamera rightCamera;
        private SparkGameCameraRig cameraRig;
        private RectTransform safeControls;
        private Button masterButton;
        private Button monitorButton;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;
        private readonly List<Canvas> hiddenCanvases = new();
        private readonly List<GraphicRaycaster> hiddenRaycasters = new();
        private SparkTutorialCircuitAwareness liveCoach;
        private CanvasGroup coachOverlay;
        private float coachOpacity;
        private bool coachRaycasts, coachSuppressed;

        internal bool Mount(Transform room, SparkGameCameraRig rig)
        {
            if (!room) return false;
            var art = room.Find("_PS_ROOM_VISUAL_ART");
            if (!art) return false;
            var screens = new[]
            {
                art.Find("PS_Studio_MissionPlanningDisplayScreen"),
                art.Find("PS_Studio_ProjectSparkLogoScreen")
            }.Where(screen => screen && screen.GetComponentInChildren<Renderer>()).Select(screen =>
                RoomBounds(room, screen.GetComponentInChildren<Renderer>().bounds)).OrderBy(bounds => bounds.center.x).ToArray();
            if (screens.Length != 2) return false;

            cameraRig = rig;
            // Viewed from the operator side (+Z), +X in room space is left.
            left = MakeSurface(room, screens[1], "Left Wall - Master Monitor");
            right = MakeSurface(room, screens[0], "Right Wall - Workshop Computer");
            if (cameraRig)
            {
                leftCamera = MakeCamera(room, "Master Panel Camera");
                rightCamera = MakeCamera(room, "Right Wall Monitor Camera");
                BuildControls();
                cameraRig.WallDisplayChanged += RefreshControls;
            }
            return true;
        }

        internal void Connect(SparkUIMonitorDashboard dashboard, SparkUIManager manager, Canvas source,
            RectTransform computer, SparkTutorialCircuitAwareness liveCoach)
        {
            this.liveCoach = liveCoach;
            dashboard.AttachToWall(left, LocateMaster, LeaveMaster, () => Focus(right, rightCamera));
            if (!source || !computer) return;
            var oldControls = source.GetComponent<MonitorMaximizeController>();
            if (oldControls)
            {
                oldControls.enabled = false;
                if (oldControls.sizeSlider) oldControls.sizeSlider.gameObject.SetActive(false);
                foreach (var button in computer.GetComponentsInChildren<Button>(true))
                {
                    bool adjustsScreen = Enumerable.Range(0, button.onClick.GetPersistentEventCount())
                        .Any(index => button.onClick.GetPersistentTarget(index) == oldControls);
                    if (adjustsScreen) button.gameObject.SetActive(false);
                }
            }

            var backing = new GameObject("Workshop Computer display backing", typeof(RectTransform), typeof(Image));
            var backingRect = (RectTransform)backing.transform;
            backingRect.SetParent(right.transform, false);
            backingRect.anchorMin = Vector2.zero;
            backingRect.anchorMax = Vector2.one;
            backingRect.offsetMin = backingRect.offsetMax = Vector2.zero;
            var background = backing.GetComponent<Image>();
            background.color = new Color(.016f, .046f, .067f);
            background.raycastTarget = false;

            Vector2 originalSize = computer.rect.size;
            computer.SetParent(right.transform, false);
            computer.anchorMin = computer.anchorMax = computer.pivot = new Vector2(.5f, .5f);
            computer.anchoredPosition3D = Vector3.zero;
            computer.sizeDelta = originalSize;
            Vector2 surface = ((RectTransform)right.transform).rect.size;
            float fit = Mathf.Min(surface.x / originalSize.x, surface.y / originalSize.y);
            computer.localScale = Vector3.one * fit;

            computer.gameObject.SetActive(true);
            var panel = manager.RegisterNative("workshop", "Workshop Computer", SparkUILayer.HUD, "", false,
                visible => computer.gameObject.SetActive(true),
                () => computer && computer.gameObject.activeInHierarchy);
            if (panel)
            {
                panel.AlwaysVisible = true;
                manager.Open(panel.Id);
            }
        }

        private void LocateMaster() => Focus(left, leftCamera);
        private void LeaveMaster()
        {
            if (cameraRig && left) cameraRig.LeaveWallDisplay(left.transform);
            if (cameraRig && right) cameraRig.LeaveWallDisplay(right.transform);
        }

        private void Focus(Canvas surface, CinemachineCamera view)
        {
            if (cameraRig && surface)
                cameraRig.FocusWallDisplay((RectTransform)surface.transform, view);
        }

        private void Toggle(Canvas surface, CinemachineCamera view)
        {
            if (cameraRig.ActiveWallDisplay == surface.transform) cameraRig.LeaveWallDisplay(surface.transform);
            else Focus(surface, view);
        }

        private static CinemachineCamera MakeCamera(Transform room, string name)
        {
            var node = new GameObject(name);
            node.SetActive(false);
            node.transform.SetParent(room, false);
            var view = node.AddComponent<CinemachineCamera>();
            view.enabled = false;
            node.SetActive(true);
            return view;
        }

        private void BuildControls()
        {
            var root = new GameObject("Wall Camera Controls", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 31000;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            safeControls = new GameObject("Safe area", typeof(RectTransform)).GetComponent<RectTransform>();
            safeControls.SetParent(root.transform, false);
            masterButton = MakeButton("Master Panel", 20, () => Toggle(left, leftCamera));
            monitorButton = MakeButton("Right Monitor", 174, () => Toggle(right, rightCamera));
            UpdateSafeArea();
            RefreshControls();
        }

        private Button MakeButton(string caption, float x, UnityEngine.Events.UnityAction click)
        {
            var node = new GameObject(caption, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)node.transform;
            rect.SetParent(safeControls, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -10f);
            rect.sizeDelta = new Vector2(146f, 42f);
            var image = node.GetComponent<Image>();
            var button = node.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(click);
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            label.transform.SetParent(rect, false);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 17;
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(.88f, .96f, 1f);
            label.text = caption;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            return button;
        }

        private void UpdateSafeArea()
        {
            int width = Screen.width;
            int height = Screen.height;
            if (width <= 0 || height <= 0) return;
            lastScreenSize = new Vector2Int(width, height);
            lastSafeArea = Screen.safeArea;
            safeControls.anchorMin = new Vector2(lastSafeArea.xMin / width, lastSafeArea.yMin / height);
            safeControls.anchorMax = new Vector2(lastSafeArea.xMax / width, lastSafeArea.yMax / height);
            safeControls.offsetMin = safeControls.offsetMax = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (!safeControls || !cameraRig) return;
            if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height || lastSafeArea != Screen.safeArea)
            {
                UpdateSafeArea();
                cameraRig.RefreshWallFraming();
            }
            bool available = !SignalWireBuilder.AnyWireDrawing;
            masterButton.interactable = available;
            monitorButton.interactable = available;
        }

        private void RefreshControls()
        {
            bool focused = cameraRig.ActiveWallDisplay;
            masterButton.image.color = ButtonColor(cameraRig.ActiveWallDisplay == left.transform);
            monitorButton.image.color = ButtonColor(cameraRig.ActiveWallDisplay == right.transform);
            if (!focused)
            {
                RestoreHud();
                return;
            }
            if (!coachOverlay && liveCoach)
            {
                var overlay = liveCoach.transform.Find("Spark_AI_LiveCircuitHUD");
                if (overlay && !overlay.TryGetComponent<CanvasGroup>(out coachOverlay))
                    coachOverlay = overlay.gameObject.AddComponent<CanvasGroup>();
            }
            if (coachOverlay && !coachSuppressed)
            {
                coachOpacity = coachOverlay.alpha;
                coachRaycasts = coachOverlay.blocksRaycasts;
                coachOverlay.alpha = 0;
                coachOverlay.blocksRaycasts = false;
                coachSuppressed = true;
            }
            var layout = cameraRig.ActiveLayout;
            if (!layout || hiddenCanvases.Count > 0) return;
            foreach (var canvas in layout.GetComponentsInChildren<Canvas>())
            {
                if (!canvas.enabled || canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                canvas.enabled = false;
                hiddenCanvases.Add(canvas);
                if (!canvas.TryGetComponent<GraphicRaycaster>(out var raycaster) || !raycaster.enabled) continue;
                raycaster.enabled = false;
                hiddenRaycasters.Add(raycaster);
            }
        }

        private static Color ButtonColor(bool selected) => selected
            ? new Color(.07f, .33f, .45f, .99f) : new Color(.025f, .11f, .18f, .98f);

        private void RestoreHud()
        {
            if (coachOverlay && coachSuppressed)
            {
                coachOverlay.alpha = coachOpacity;
                coachOverlay.blocksRaycasts = coachRaycasts;
            }
            coachSuppressed = false;
            foreach (var canvas in hiddenCanvases) if (canvas) canvas.enabled = true;
            foreach (var raycaster in hiddenRaycasters) if (raycaster) raycaster.enabled = true;
            hiddenCanvases.Clear();
            hiddenRaycasters.Clear();
        }

        private void OnDestroy()
        {
            if (cameraRig)
            {
                cameraRig.WallDisplayChanged -= RefreshControls;
                LeaveMaster();
            }
            RestoreHud();
            if (leftCamera) Destroy(leftCamera.gameObject);
            if (rightCamera) Destroy(rightCamera.gameObject);
            if (left) Destroy(left.gameObject);
            if (right) Destroy(right.gameObject);
        }

        private static Bounds RoomBounds(Transform room, Bounds world)
        {
            var bounds = new Bounds(room.InverseTransformPoint(world.center), Vector3.zero);
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3((i & 1) == 0 ? world.min.x : world.max.x,
                    (i & 2) == 0 ? world.min.y : world.max.y, (i & 4) == 0 ? world.min.z : world.max.z);
                bounds.Encapsulate(room.InverseTransformPoint(corner));
            }
            return bounds;
        }

        private static Canvas MakeSurface(Transform room, Bounds screen, string label)
        {
            const float pixelsPerUnit = 250f;
            var mount = new GameObject(label, typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            var rect = (RectTransform)mount.transform;
            rect.SetParent(room, false);
            var outward = Mathf.Sign(screen.center.x) * Vector3.right;
            rect.localPosition = screen.center - outward * .045f;
            rect.localRotation = Quaternion.LookRotation(outward, Vector3.up);
            rect.localScale = Vector3.one / pixelsPerUnit;
            rect.sizeDelta = new Vector2(screen.size.z, screen.size.y) * pixelsPerUnit;
            var canvas = mount.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 |
                AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
            return canvas;
        }
    }
}

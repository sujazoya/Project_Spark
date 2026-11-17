using System.Linq;
using ProjectSpark.Cameras;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UIManagement
{
    internal sealed class SparkWorkshopWallDisplays : MonoBehaviour
    {
        private Canvas left;
        private Canvas right;
        private SparkGameCameraRig cameraRig;

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
            return true;
        }

        internal void Connect(SparkUIMonitorDashboard dashboard, SparkUIManager manager, Canvas source, RectTransform computer)
        {
            dashboard.AttachToWall(left, LocateMaster, LeaveMaster, () => Focus(right));
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

        private void LocateMaster() => Focus(left);
        private void LeaveMaster()
        {
            if (cameraRig && left) cameraRig.LeaveWallDisplay(left.transform);
            if (cameraRig && right) cameraRig.LeaveWallDisplay(right.transform);
        }

        private void Focus(Canvas surface)
        {
            if (cameraRig && surface)
                cameraRig.FocusWallDisplay((RectTransform)surface.transform);
        }

        private void OnDestroy()
        {
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
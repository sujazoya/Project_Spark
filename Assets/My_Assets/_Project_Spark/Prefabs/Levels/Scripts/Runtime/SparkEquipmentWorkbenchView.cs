using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Cameras;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ProjectSpark.Gameplay
{
    [DefaultExecutionOrder(9000)]
    public class SparkEquipmentWorkbenchView : MonoBehaviour
    {
        public Transform Bench;
        public Bounds FramingBounds;

        private Camera view;
        private SparkGameCameraRig sharedRig;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private bool originalOrthographic;
        private float originalSize, originalNearClip;
        private float pitch = 43f;
        private float yaw;
        private float zoom = 1f;
        private Vector2 pan;
        private readonly Dictionary<Renderer, bool> shelfRenderers = new();
        private bool shelfHidden;

        public void ResetView()
        {
            pitch = 43f;
            yaw = 0f;
            zoom = 1f;
            pan = Vector2.zero;
        }

        public void FocusPair(Vector3 first, Vector3 second)
        {
            if (!view || !Bench || SignalWireBuilder.AnyWireDrawing) return;
            if (sharedRig && sharedRig.OwnsCamera) sharedRig.ReturnToLevelView();
            ResetView();
            var rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 up = rotation * Vector3.up;
            Vector3 right = rotation * Vector3.right;
            Vector3 offset = Bench.InverseTransformPoint((first + second) * .5f) - FramingBounds.center;
            pan = new Vector2(Vector3.Dot(offset, right), Vector3.Dot(offset, up));
            Vector3 span = Bench.InverseTransformVector(second - first);
            float required = Mathf.Max(Mathf.Abs(Vector3.Dot(span, up)) / 1.1f,
                Mathf.Abs(Vector3.Dot(span, right)) / (1.55f * view.aspect));
            zoom = Mathf.Clamp(required / FramingSize(up, right), .34f, 1f);
        }

        protected virtual void Start()
        {
            view = Camera.main;
            if (!view) return;
            sharedRig = view.GetComponent<SparkGameCameraRig>();
            originalPosition = view.transform.position;
            originalRotation = view.transform.rotation;
            originalOrthographic = view.orthographic;
            originalSize = view.orthographicSize;
            originalNearClip = view.nearClipPlane;
            var shelf = GameObject.Find("ProjectSpark_Gameplay")?.transform.Find("Component_Shelf");
            if (shelf)
                foreach (var renderer in shelf.GetComponentsInChildren<Renderer>(true))
                    shelfRenderers.Add(renderer, renderer.forceRenderingOff);
        }

        protected virtual void LateUpdate()
        {
            if (!view || !Bench) return;
            bool usingSharedView = sharedRig && sharedRig.OwnsCamera;
            HideForegroundShelf(!usingSharedView);
            if (usingSharedView || SignalWireBuilder.AnyWireDrawing) return;
            var mouse = Mouse.current;
            if (Application.isFocused && mouse != null &&
                (!EventSystem.current || !EventSystem.current.IsPointerOverGameObject()) &&
                (!sharedRig || !sharedRig.IsPointerOverViewControls(mouse.position.ReadValue())))
            {
                var delta = mouse.delta.ReadValue();
                zoom = Mathf.Clamp(zoom * Mathf.Exp(-mouse.scroll.ReadValue().y * .0009f), .30f, 1.65f);
                if (mouse.rightButton.isPressed)
                {
                    yaw = Mathf.Clamp(yaw + delta.x * .24f, -155f, 155f);
                    pitch = Mathf.Clamp(pitch - delta.y * .19f, 20f, 78f);
                }
                if (mouse.middleButton.isPressed)
                {
                    pan -= delta * (.002f * zoom);
                    pan.x = Mathf.Clamp(pan.x, -1.45f, 1.45f);
                    pan.y = Mathf.Clamp(pan.y, -.90f, .90f);
                }
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 up = rotation * Vector3.up;
            Vector3 right = rotation * Vector3.right;
            Vector3 forward = rotation * Vector3.forward;
            Vector3 size = FramingBounds.extents;
            float frame = FramingSize(up, right) * zoom;
            // Leave the bottom quarter of the screen for the level controls.
            Vector3 target = FramingBounds.center + right * pan.x + up * (pan.y - .23f * frame);
            float depth = Mathf.Abs(forward.x) * size.x + Mathf.Abs(forward.y) * size.y + Mathf.Abs(forward.z) * size.z;
            // Stay inside the room and just in front of the fixture's nearest surface.
            Vector3 offset = -forward * (depth + .16f);
            view.transform.SetPositionAndRotation(Bench.TransformPoint(target + offset), Bench.rotation * rotation);
            view.orthographic = true;
            view.orthographicSize = Bench.TransformVector(Vector3.up * frame).magnitude;
            view.nearClipPlane = Bench.TransformVector(Vector3.forward * .11f).magnitude;
        }

        private float FramingSize(Vector3 up, Vector3 right)
        {
            Vector3 size = FramingBounds.extents;
            float vertical = Mathf.Abs(up.x) * size.x + Mathf.Abs(up.y) * size.y + Mathf.Abs(up.z) * size.z;
            float horizontal = Mathf.Abs(right.x) * size.x + Mathf.Abs(right.y) * size.y + Mathf.Abs(right.z) * size.z;
            return Mathf.Max(vertical / .66f, horizontal / (.91f * view.aspect));
        }

        private void HideForegroundShelf(bool hide)
        {
            if (shelfHidden == hide) return;
            foreach (var renderer in shelfRenderers)
                if (renderer.Key) renderer.Key.forceRenderingOff = hide || renderer.Value;
            shelfHidden = hide;
        }

        protected virtual void OnDisable()
        {
            HideForegroundShelf(false);
            if (!view || (sharedRig && sharedRig.OwnsCamera)) return;
            view.transform.SetPositionAndRotation(originalPosition, originalRotation);
            view.orthographic = originalOrthographic;
            view.orthographicSize = originalSize;
            view.nearClipPlane = originalNearClip;
        }
    }
}

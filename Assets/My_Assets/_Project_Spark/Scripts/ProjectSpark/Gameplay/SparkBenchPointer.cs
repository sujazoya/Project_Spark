using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
#endif

namespace ProjectSpark.Gameplay
{
    internal static class SparkBenchPointer
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private static readonly List<RaycastResult> hits = new();
        private static PointerEventData eventData;
        private static EventSystem eventOwner;
        private static int sampledFrame = -1;
        private static int fingerId;
        private static bool tracking, suppress, startedOverUi;
        private static bool cameraPairTracking, cameraPairBlocked;
        private static bool pressed, held, released, cancelled;
        private static bool pointerHitsUi;
        private static Vector2 position;
        private static Vector2Int viewport;
        private static TouchControl first, second;

        internal static Vector3 Position { get { Sample(); return position; } }
        internal static bool Pressed { get { Sample(); return pressed; } }
        internal static bool Held { get { Sample(); return held; } }
        internal static bool Released { get { Sample(); return released; } }
        internal static bool Cancelled { get { Sample(); return cancelled; } }
        internal static bool SecondaryPressed => Cancelled;
        internal static bool IsOverUI()
        {
            Sample();
            return startedOverUi || pointerHitsUi;
        }

        internal static bool KeyDown(KeyCode key) => key == KeyCode.Mouse0
            ? Pressed : ResolveKey(key)?.wasPressedThisFrame == true;
        internal static bool KeyHeld(KeyCode key) => ResolveKey(key)?.isPressed == true;
        internal static float ScrollY => 0f;

        private static KeyControl ResolveKey(KeyCode code)
        {
            Key key = code switch
            {
                KeyCode.Escape => Key.Escape,
                KeyCode.I => Key.I,
                KeyCode.Z => Key.Z,
                KeyCode.Y => Key.Y,
                KeyCode.G => Key.G,
                KeyCode.V => Key.V,
                KeyCode.Delete => Key.Delete,
                KeyCode.Backspace => Key.Backspace,
                KeyCode.LeftControl => Key.LeftCtrl,
                KeyCode.RightControl => Key.RightCtrl,
                KeyCode.LeftAlt => Key.LeftAlt,
                KeyCode.RightAlt => Key.RightAlt,
                KeyCode.LeftShift => Key.LeftShift,
                KeyCode.RightShift => Key.RightShift,
                _ => Key.None
            };
            return key == Key.None || Keyboard.current == null ? null : Keyboard.current[key];
        }

        internal static bool TryGetCameraPair(out int firstId, out int secondId, out Vector2 a, out Vector2 b)
        {
            Sample();
            firstId = secondId = 0;
            a = b = default;
            if (first == null || second == null || cameraPairBlocked) return false;
            a = first.position.ReadValue();
            b = second.position.ReadValue();
            if (HitsUi(a) || HitsUi(b)) return false;
            firstId = first.touchId.ReadValue();
            secondId = second.touchId.ReadValue();
            return true;
        }

        private static bool HitsUi(Vector2 screenPoint)
        {
            var camera = Camera.main;
            if (camera && camera.TryGetComponent<ProjectSpark.Cameras.SparkGameCameraRig>(out var rig) &&
                rig.IsPointerOverViewControls(screenPoint)) return true;
            var events = EventSystem.current;
            if (!events) return false;
            if (eventOwner != events)
            {
                eventOwner = events;
                eventData = new PointerEventData(events);
            }
            eventData.Reset();
            eventData.position = screenPoint;
            events.RaycastAll(eventData, hits);
            bool blocked = false;
            for (int i = 0; i < hits.Count && !blocked; i++)
                blocked = hits[i].module is GraphicRaycaster;
            hits.Clear();
            return blocked;
        }

        private static void Sample()
        {
            if (sampledFrame == Time.frameCount) return;
            sampledFrame = Time.frameCount;
            pressed = held = released = cancelled = pointerHitsUi = false;
            first = second = null;
            TouchControl owned = null;
            int count = 0;
            var screen = Touchscreen.current;
            if (screen != null)
            {
                foreach (TouchControl touch in screen.touches)
                {
                    if (tracking && touch.touchId.ReadValue() == fingerId) owned = touch;
                    if (!touch.press.isPressed) continue;
                    if (count == 0) first = touch;
                    else if (count == 1) second = touch;
                    count++;
                }
            }

            var currentViewport = new Vector2Int(Screen.width, Screen.height);
            if (tracking && currentViewport != viewport && !suppress && !startedOverUi)
            {
                cancelled = true;
                suppress = true;
            }
            viewport = currentViewport;

            if (count == 0)
            {
                if (tracking)
                {
                    if (owned != null) position = owned.position.ReadValue();
                    released = !suppress && !startedOverUi &&
                        owned?.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended;
                    cancelled |= !released && !suppress && !startedOverUi;
                }
                tracking = suppress = false;
                cameraPairTracking = cameraPairBlocked = false;
                if (!released && !cancelled) startedOverUi = false;
                if (released) pointerHitsUi = HitsUi(position);
                return;
            }
            if (!tracking && !suppress)
            {
                fingerId = first.touchId.ReadValue();
                position = first.position.ReadValue();
                startedOverUi = HitsUi(position);
                tracking = true;
                owned = first;
                pressed = count == 1 && !startedOverUi &&
                    first.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began;
                if (!pressed && !startedOverUi && count == 1) suppress = true;
            }
            if (count > 1 || owned == null || !owned.press.isPressed)
            {
                if (count > 1 && !cameraPairTracking)
                {
                    cameraPairTracking = true;
                    cameraPairBlocked = startedOverUi || HitsUi(first.position.ReadValue()) ||
                        HitsUi(second.position.ReadValue());
                }
                cancelled |= !suppress && !startedOverUi;
                suppress = true;
                return;
            }
            position = owned.position.ReadValue();
            held = !suppress && !startedOverUi;
            if (held) pointerHitsUi = HitsUi(position);
        }
#else
        internal static Vector3 Position => Input.mousePosition;
        internal static bool Pressed => Input.GetMouseButtonDown(0);
        internal static bool Held => Input.GetMouseButton(0);
        internal static bool Released => Input.GetMouseButtonUp(0);
        internal static bool Cancelled => false;
        internal static bool SecondaryPressed => Input.GetMouseButtonDown(1);
        internal static bool KeyDown(KeyCode key) => Input.GetKeyDown(key);
        internal static bool KeyHeld(KeyCode key) => Input.GetKey(key);
        internal static float ScrollY => Input.mouseScrollDelta.y;
#endif
    }
}

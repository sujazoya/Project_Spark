using UnityEngine;

namespace ProjectSpark.Gameplay.Camera
{
    public sealed class CameraManager : MonoBehaviour
    {
        [SerializeField]
        private CameraSettings settings;

        private CameraState _state;

        private CameraRig _rig;

        private CameraFocus _focus;

        private void Awake()
        {
            _state = new CameraState();

            _rig = new CameraRig(transform);

            _focus = new CameraFocus(_state);
        }

        private void LateUpdate()
        {
            UpdateInput();

            _rig.UpdateRig(_state);
        }

        private void UpdateInput()
        {
            if (UnityEngine.Input.GetMouseButton(1))
            {
                _state.Yaw +=
                    UnityEngine.Input.GetAxis("Mouse X") *
                    settings.RotationSpeed *
                    Time.deltaTime;

                _state.Pitch -=
                    UnityEngine.Input.GetAxis("Mouse Y") *
                    settings.RotationSpeed *
                    Time.deltaTime;
            }

            _state.Pitch = Mathf.Clamp(
                _state.Pitch,
                settings.MinPitch,
                settings.MaxPitch);

            _state.Distance -=
                UnityEngine.Input.mouseScrollDelta.y *
                settings.ZoomSpeed *
                Time.deltaTime;

            _state.Distance =
                Mathf.Clamp(
                    _state.Distance,
                    settings.MinDistance,
                    settings.MaxDistance);
        }

        public void Focus(Transform target)
        {
            _focus.Focus(target);
        }
    }
}

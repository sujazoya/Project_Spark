using UnityEngine;

namespace ProjectSpark.Gameplay.Camera
{
    public sealed class CameraRig
    {
        private readonly Transform _camera;

        public CameraRig(Transform camera)
        {
            _camera = camera;
        }

        public void UpdateRig(CameraState state)
        {
            Quaternion rotation =
                Quaternion.Euler(
                    state.Pitch,
                    state.Yaw,
                    0);

            Vector3 position =
                state.Target -
                rotation * Vector3.forward *
                state.Distance;

            _camera.SetPositionAndRotation(
                position,
                rotation);
        }
    }
}

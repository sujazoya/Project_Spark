using UnityEngine;

namespace ProjectSpark.Gameplay.Camera
{
    public sealed class CameraFocus
    {
        private readonly CameraState _state;

        public CameraFocus(CameraState state)
        {
            _state = state;
        }

        public void Focus(Transform target)
        {
            if (target == null)
                return;

            _state.Target = target.position;
        }
    }
}

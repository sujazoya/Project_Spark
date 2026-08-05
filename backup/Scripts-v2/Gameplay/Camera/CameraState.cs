using UnityEngine;

namespace ProjectSpark.Gameplay.Camera
{
    [System.Serializable]
    public class CameraState
    {
        public Vector3 Target;

        public float Distance = 8f;

        public float Pitch = 45f;

        public float Yaw = 45f;

        public float FieldOfView = 60f;

        public CameraMode Mode = CameraMode.Gameplay;
    }
}

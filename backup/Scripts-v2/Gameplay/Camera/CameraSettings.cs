using UnityEngine;

namespace ProjectSpark.Gameplay.Camera
{
    [CreateAssetMenu(
        menuName = "Project Spark/Camera/Settings")]
    public class CameraSettings : ScriptableObject
    {
        [Header("Movement")]

        public float RotationSpeed = 120f;

        public float PanSpeed = 12f;

        public float ZoomSpeed = 12f;

        [Header("Limits")]

        public float MinDistance = 2f;

        public float MaxDistance = 20f;

        public float MinPitch = 10f;

        public float MaxPitch = 85f;

        [Header("Smoothing")]

        public float PositionSmooth = 12f;

        public float RotationSmooth = 12f;

        public float ZoomSmooth = 10f;
    }
}

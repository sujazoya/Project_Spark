// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireSettings.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [CreateAssetMenu(
        fileName = "WireSettings",
        menuName = "Project Spark/Wiring/Wire Settings")]
    public sealed class WireSettings : ScriptableObject
    {
        [Header("Cable")]

        public float Radius = 0.0045f;

        public int RadialSegments = 12;

        public int LengthSegments = 40;

        [Header("Drag")]

        public float Sag = 0.06f;

        public float FollowSpeed = 14f;

        [Header("Snap")]

        public float SnapDistance = 0.025f;

        public float MagneticDistance = 0.08f;

        [Header("Animation")]

        public float CurrentSpeed = 2f;
    }
}
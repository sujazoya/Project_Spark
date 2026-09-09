using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public readonly struct HolographicSnapResult
    {
        public readonly bool Valid;

        public readonly HolographicSnapType Type;

        public readonly HolographicSnapGeometry Geometry;

        public readonly Transform Transform;

        public readonly Vector3 LocalPosition;

        public readonly Vector3 WorldPosition;

        public readonly Vector3 WorldNormal;

        public readonly float ScreenDistance;

        public readonly float Depth;

        public readonly int PrimaryIndex;

        public readonly int SecondaryIndex;

        public readonly float EdgeT;

        public HolographicSnapResult(
            HolographicSnapType type,
            HolographicSnapGeometry geometry,
            Transform transform,
            Vector3 localPosition,
            Vector3 worldPosition,
            Vector3 worldNormal,
            float screenDistance,
            float depth,
            int primaryIndex,
            int secondaryIndex,
            float edgeT)
        {
            Valid = true;

            Type = type;

            Geometry = geometry;

            Transform = transform;

            LocalPosition = localPosition;

            WorldPosition = worldPosition;

            WorldNormal = worldNormal;

            ScreenDistance = screenDistance;

            Depth = depth;

            PrimaryIndex = primaryIndex;

            SecondaryIndex = secondaryIndex;

            EdgeT = edgeT;
        }

        public static HolographicSnapResult Invalid =>
            default;
    }
}
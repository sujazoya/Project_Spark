using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public readonly struct HolographicCircularFeatureResult
    {
        public readonly bool Valid;

        public readonly HolographicCircularFeature Feature;

        public readonly HolographicCircularFeatureType Type;

        public readonly Vector3 WorldPosition;

        public readonly Vector3 WorldNormal;

        public readonly Vector3 WorldAxis;

        public readonly float Radius;

        public readonly float Diameter;

        public readonly float ScreenDistance;

        public readonly float AxialDistance;

        public HolographicCircularFeatureResult(
            HolographicCircularFeature feature,
            HolographicCircularFeatureType type,
            Vector3 worldPosition,
            Vector3 worldNormal,
            Vector3 worldAxis,
            float radius,
            float diameter,
            float screenDistance,
            float axialDistance)
        {
            Valid = feature != null;

            Feature = feature;

            Type = type;

            WorldPosition = worldPosition;

            WorldNormal = worldNormal;

            WorldAxis = worldAxis;

            Radius = radius;

            Diameter = diameter;

            ScreenDistance =
                screenDistance;

            AxialDistance =
                axialDistance;
        }

        public static HolographicCircularFeatureResult
            Invalid =>
            default;
    }
}
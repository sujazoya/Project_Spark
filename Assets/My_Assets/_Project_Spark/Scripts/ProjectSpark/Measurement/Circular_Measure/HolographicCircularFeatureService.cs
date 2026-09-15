using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    [DisallowMultipleComponent]
    public sealed class HolographicCircularFeatureService
        : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private Camera viewerCamera;

        [SerializeField]
        private HolographicCircularFeatureRegistry registry;

        [Header("Selection")]

        [SerializeField]
        private float centerPixelDistance = 18f;

        [SerializeField]
        private float rimPixelDistance = 14f;

        public HolographicCircularFeatureResult FindFeature(
            Vector2 screenPosition,
            Vector3 worldHit)
        {
            if (viewerCamera == null ||
                registry == null)
            {
                return
                    HolographicCircularFeatureResult.Invalid;
            }

            HolographicCircularFeatureResult best =
                HolographicCircularFeatureResult.Invalid;

            float bestDistance =
                float.MaxValue;

            var features =
                registry.Features;

            for (int i = 0;
                 i < features.Count;
                 i++)
            {
                HolographicCircularFeature feature =
                    features[i];

                if (feature == null)
                    continue;

                EvaluateCenter(
                    feature,
                    screenPosition,
                    ref best,
                    ref bestDistance
                );

                EvaluateRim(
                    feature,
                    screenPosition,
                    worldHit,
                    ref best,
                    ref bestDistance
                );
            }

            return best;
        }

        private void EvaluateCenter(
            HolographicCircularFeature feature,
            Vector2 screenPosition,
            ref HolographicCircularFeatureResult best,
            ref float bestDistance)
        {
            if (!feature.Supports(
                    HolographicCircularMeasurementCapabilities
                        .Center))
            {
                return;
            }

            Vector3 screen =
                viewerCamera.WorldToScreenPoint(
                    feature.WorldCenter
                );

            if (screen.z <= 0f)
                return;

            float distance =
                Vector2.Distance(
                    screenPosition,
                    new Vector2(
                        screen.x,
                        screen.y
                    )
                );

            if (distance >
                centerPixelDistance)
            {
                return;
            }

            if (distance >= bestDistance)
                return;

            bestDistance =
                distance;

            best =
                new HolographicCircularFeatureResult(
                    feature,
                    feature.FeatureType,
                    feature.WorldCenter,
                    feature.WorldAxis,
                    feature.WorldAxis,
                    feature.Radius,
                    feature.Diameter,
                    distance,
                    0f
                );
        }

        private void EvaluateRim(
            HolographicCircularFeature feature,
            Vector2 screenPosition,
            Vector3 worldHit,
            ref HolographicCircularFeatureResult best,
            ref float bestDistance)
        {
            if (!feature.Supports(
                    HolographicCircularMeasurementCapabilities
                        .Rim))
            {
                return;
            }

            Vector3 rim =
                feature.GetClosestPointOnRim(
                    worldHit
                );

            Vector3 screen =
                viewerCamera.WorldToScreenPoint(
                    rim
                );

            if (screen.z <= 0f)
                return;

            float distance =
                Vector2.Distance(
                    screenPosition,
                    new Vector2(
                        screen.x,
                        screen.y
                    )
                );

            if (distance >
                rimPixelDistance)
            {
                return;
            }

            if (distance >= bestDistance)
                return;

            bestDistance =
                distance;

            best =
                new HolographicCircularFeatureResult(
                    feature,
                    feature.FeatureType,
                    rim,
                    feature.WorldAxis,
                    feature.WorldAxis,
                    feature.Radius,
                    feature.Diameter,
                    distance,
                    feature.GetSignedAxialDistance(
                        worldHit
                    )
                );
        }
    }
}